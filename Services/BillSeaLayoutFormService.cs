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
    }

    public sealed class BillSeaLayoutFormSummary
    {
        public Guid BillSeaLayoutFormId { get; init; }
        public string FormName { get; init; } = string.Empty;
        public DateTime UpdatedAt { get; init; }
        public bool IsCustomized { get; init; }
        public bool HasAttachForm { get; init; }
    }

    public class BillSeaLayoutFormService(AppDbContext context, IWebHostEnvironment env)
    {
        public string GetDefaultTemplatePath(string reportFileName) =>
            Path.Combine(env.WebRootPath, "Reports", reportFileName);

        public async Task<byte[]> GetDefaultTemplateBytesAsync(string reportFileName = BillSeaReportTemplateNames.Main, CancellationToken cancellationToken = default)
        {
            var defaultPath = GetDefaultTemplatePath(reportFileName);
            if (!File.Exists(defaultPath))
                throw new FileNotFoundException($"Không tìm thấy template mặc định: {reportFileName}", defaultPath);

            return await File.ReadAllBytesAsync(defaultPath, cancellationToken);
        }

        public async Task<List<BillSeaLayoutFormSummary>> ListFormsAsync(CancellationToken cancellationToken = default)
        {
            context.ChangeTracker.Clear();
            var defaultBytes = await GetDefaultTemplateBytesAsync(cancellationToken: cancellationToken);

            var forms = await context.BillSeaLayoutForms
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.FormName)
                .ToListAsync(cancellationToken);

            return forms.Select(x => new BillSeaLayoutFormSummary
            {
                BillSeaLayoutFormId = x.BillSeaLayoutFormId,
                FormName = x.FormName,
                UpdatedAt = x.UpdatedAt,
                IsCustomized = !ContentEquals(x.MrtContent, defaultBytes),
                HasAttachForm = x.AttachMrtContent is { Length: > 0 }
            }).ToList();
        }

        public async Task<M_BillSeaLayoutForm?> GetFormAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            context.ChangeTracker.Clear();
            return await context.BillSeaLayoutForms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
        }

        public async Task<byte[]> GetFormBytesAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            return form.MrtContent;
        }

        public async Task<XDocument> LoadFormDocumentAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var bytes = await GetFormBytesAsync(formId, cancellationToken);
            using var stream = new MemoryStream(bytes);
            return XDocument.Load(stream);
        }

        public async Task<bool> IsFormCustomizedAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form is null)
                return false;

            var defaultBytes = await GetDefaultTemplateBytesAsync(cancellationToken: cancellationToken);
            return !ContentEquals(form.MrtContent, defaultBytes);
        }

        public async Task<byte[]?> GetCompanyLogoAsync(CancellationToken cancellationToken = default)
        {
            context.ChangeTracker.Clear();
            return await context.CompanyInfomation
                .AsNoTracking()
                .Select(x => x.Logo)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<byte[]?> GetFormLogoAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.Logo is { Length: > 0 } ? form.Logo : null;
        }

        public async Task<byte[]?> GetEffectiveFormLogoAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var formLogo = await GetFormLogoAsync(formId, cancellationToken);
            return formLogo ?? await GetCompanyLogoAsync(cancellationToken);
        }

        public async Task<bool> HasCustomFormLogoAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.Logo is { Length: > 0 };
        }

        public async Task SaveFormLogoAsync(Guid formId, byte[]? logo, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            form.Logo = logo is { Length: > 0 } ? logo : null;
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearFormLogoAsync(Guid formId, CancellationToken cancellationToken = default) =>
            await SaveFormLogoAsync(formId, null, cancellationToken);

        public async Task<M_BillSeaLayoutForm> CreateFormAsync(string formName, string? createdBy = null, CancellationToken cancellationToken = default)
        {
            var trimmedName = formName.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new ArgumentException("Tên form không được để trống.", nameof(formName));

            context.ChangeTracker.Clear();
            var exists = await context.BillSeaLayoutForms
                .AnyAsync(x => x.IsActive && x.FormName == trimmedName, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Đã tồn tại form tên '{trimmedName}'.");

            var defaultBytes = await GetDefaultTemplateBytesAsync(cancellationToken: cancellationToken);
            var now = DateTime.UtcNow;
            var form = new M_BillSeaLayoutForm
            {
                BillSeaLayoutFormId = Guid.NewGuid(),
                FormName = trimmedName,
                MrtContent = defaultBytes,
                SourceTemplate = BillSeaReportTemplateNames.Main,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = createdBy,
                IsActive = true
            };

            context.BillSeaLayoutForms.Add(form);
            await context.SaveChangesAsync(cancellationToken);
            return form;
        }

        public async Task SaveFormDocumentAsync(Guid formId, XDocument document, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            form.MrtContent = DocumentToBytes(document);
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetFormToDefaultAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            form.MrtContent = await GetDefaultTemplateBytesAsync(form.SourceTemplate, cancellationToken);
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteFormAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                return;

            form.IsActive = false;
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasAttachFormAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.AttachMrtContent is { Length: > 0 };
        }

        public async Task<byte[]> GetAttachSkeletonBytesAsync(CancellationToken cancellationToken = default) =>
            await GetDefaultTemplateBytesAsync(BillSeaAttachMrtHelper.AttachSkeletonFileName, cancellationToken);

        public async Task<byte[]> BuildDefaultAttachBytesAsync(byte[] mainFormBytes, CancellationToken cancellationToken = default)
        {
            var skeletonBytes = await GetAttachSkeletonBytesAsync(cancellationToken);
            return BillSeaAttachMrtHelper.BuildAttachMrt(mainFormBytes, skeletonBytes);
        }

        public async Task<M_BillSeaLayoutForm> CreateAttachFormAsync(Guid mainFormId, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == mainFormId && x.IsActive, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            if (form.AttachMrtContent is { Length: > 0 })
                throw new InvalidOperationException($"Form '{form.FormName}' đã có form Attach.");

            form.AttachMrtContent = await BuildDefaultAttachBytesAsync(form.MrtContent, cancellationToken);
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            return form;
        }

        public async Task<byte[]> GetAttachFormBytesAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");
            if (form.AttachMrtContent is not { Length: > 0 })
                throw new InvalidOperationException("Form này chưa có form Attach. Tạo tại menu 1.17.");

            return BillSeaAttachMrtHelper.EnsureExportableAttachMrt(form.AttachMrtContent);
        }

        public async Task<XDocument> LoadAttachFormDocumentAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var bytes = await GetAttachFormBytesAsync(formId, cancellationToken);
            using var stream = new MemoryStream(bytes);
            return XDocument.Load(stream);
        }

        public async Task<bool> IsAttachFormCustomizedAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form?.AttachMrtContent is not { Length: > 0 })
                return false;

            var defaultAttachBytes = await BuildDefaultAttachBytesAsync(form.MrtContent, cancellationToken);
            return !ContentEquals(form.AttachMrtContent, defaultAttachBytes);
        }

        public async Task SaveAttachFormDocumentAsync(Guid formId, XDocument document, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            form.AttachMrtContent = DocumentToBytes(document);
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetAttachFormToDefaultAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");
            if (form.AttachMrtContent is not { Length: > 0 })
                throw new InvalidOperationException("Form này chưa có form Attach.");

            form.AttachMrtContent = await BuildDefaultAttachBytesAsync(form.MrtContent, cancellationToken);
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAttachFormAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await context.BillSeaLayoutForms
                .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, cancellationToken);
            if (form is null)
                return;

            form.AttachMrtContent = null;
            form.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public static byte[] DocumentToBytes(XDocument document)
        {
            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private static bool ContentEquals(byte[] left, byte[] right) =>
            left.AsSpan().SequenceEqual(right);
    }
}
