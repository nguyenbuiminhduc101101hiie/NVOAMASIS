using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    public static class BillSeaReportTemplateNames
    {
        public const string Main = "BillSea_NVOCC.mrt";
        public const string Attach = "BillSea_NVOCC_Att_1.mrt";

        public static bool IsSupported(string reportFileName) =>
            string.Equals(reportFileName, Main, StringComparison.OrdinalIgnoreCase)
            || string.Equals(reportFileName, Attach, StringComparison.OrdinalIgnoreCase);
    }

    public sealed class BillSeaTemplateInfo
    {
        public required string ReportFileName { get; init; }
        public bool UsesCustomTemplate { get; init; }
        public bool LoadedFromDefaultFile { get; init; }
    }

    public class BillSeaReportTemplateService(AppDbContext context, IWebHostEnvironment env)
    {
        public string GetDefaultTemplatePath(string reportFileName) =>
            Path.Combine(env.WebRootPath, "Reports", reportFileName);

        public async Task<BillSeaTemplateInfo> GetTemplateInfoAsync(string reportFileName, CancellationToken cancellationToken = default)
        {
            EnsureSupported(reportFileName);
            context.ChangeTracker.Clear();

            var company = await context.CompanyInfomation
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var customBytes = GetStoredTemplate(company, reportFileName);
            return new BillSeaTemplateInfo
            {
                ReportFileName = reportFileName,
                UsesCustomTemplate = customBytes is { Length: > 0 },
                LoadedFromDefaultFile = customBytes is not { Length: > 0 }
            };
        }

        public async Task<byte[]> GetTemplateBytesAsync(string reportFileName, CancellationToken cancellationToken = default)
        {
            EnsureSupported(reportFileName);
            context.ChangeTracker.Clear();

            var company = await context.CompanyInfomation
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var customBytes = GetStoredTemplate(company, reportFileName);
            if (customBytes is { Length: > 0 })
                return customBytes;

            var defaultPath = GetDefaultTemplatePath(reportFileName);
            if (!File.Exists(defaultPath))
                throw new FileNotFoundException($"Không tìm thấy template mặc định: {reportFileName}", defaultPath);

            return await File.ReadAllBytesAsync(defaultPath, cancellationToken);
        }

        public async Task<XDocument> LoadTemplateDocumentAsync(string reportFileName, CancellationToken cancellationToken = default)
        {
            var bytes = await GetTemplateBytesAsync(reportFileName, cancellationToken);
            using var stream = new MemoryStream(bytes);
            return XDocument.Load(stream);
        }

        public async Task SaveTemplateDocumentAsync(string reportFileName, XDocument document, CancellationToken cancellationToken = default)
        {
            EnsureSupported(reportFileName);

            var company = await context.CompanyInfomation.FirstOrDefaultAsync(cancellationToken);
            if (company is null)
            {
                company = new M_CompanyInfo { CompanyID = Guid.NewGuid() };
                context.CompanyInfomation.Add(company);
            }

            SetStoredTemplate(company, reportFileName, DocumentToBytes(document));
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetToDefaultAsync(string reportFileName, CancellationToken cancellationToken = default)
        {
            EnsureSupported(reportFileName);
            context.ChangeTracker.Clear();

            var company = await context.CompanyInfomation.FirstOrDefaultAsync(cancellationToken);
            if (company is null)
                return;

            SetStoredTemplate(company, reportFileName, null);
            await context.SaveChangesAsync(cancellationToken);
        }

        public static byte[] DocumentToBytes(XDocument document)
        {
            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private static void EnsureSupported(string reportFileName)
        {
            if (!BillSeaReportTemplateNames.IsSupported(reportFileName))
                throw new ArgumentException($"Report template không được hỗ trợ: {reportFileName}", nameof(reportFileName));
        }

        private static byte[]? GetStoredTemplate(M_CompanyInfo? company, string reportFileName) =>
            string.Equals(reportFileName, BillSeaReportTemplateNames.Attach, StringComparison.OrdinalIgnoreCase)
                ? company?.BillSeaLayoutAttMrt
                : company?.BillSeaLayoutMrt;

        private static void SetStoredTemplate(M_CompanyInfo company, string reportFileName, byte[]? content)
        {
            if (string.Equals(reportFileName, BillSeaReportTemplateNames.Attach, StringComparison.OrdinalIgnoreCase))
                company.BillSeaLayoutAttMrt = content;
            else
                company.BillSeaLayoutMrt = content;
        }
    }
}
