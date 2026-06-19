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
        public const string Air = "BillAir.mrt";
    }

    public static class BillLayoutFormKindHelper
    {
        public static string GetDefaultTemplateFile(BillLayoutFormKind kind) =>
            kind == BillLayoutFormKind.Air
                ? BillSeaReportTemplateNames.Air
                : BillSeaReportTemplateNames.Main;

        public static BillLayoutFormKind ParseFormKind(string? value) =>
            string.Equals(value, nameof(BillLayoutFormKind.Air), StringComparison.OrdinalIgnoreCase)
                ? BillLayoutFormKind.Air
                : BillLayoutFormKind.Sea;

        public static string ToStorageValue(BillLayoutFormKind kind) =>
            kind == BillLayoutFormKind.Air ? nameof(BillLayoutFormKind.Air) : nameof(BillLayoutFormKind.Sea);

        public static string GetDisplayName(BillLayoutFormKind kind) =>
            kind == BillLayoutFormKind.Air ? "Air" : "Sea";
    }

    public sealed class BillSeaLayoutFormSummary
    {
        public Guid BillSeaLayoutFormId { get; init; }
        public string FormName { get; init; } = string.Empty;
        public BillLayoutFormKind FormKind { get; init; } = BillLayoutFormKind.Sea;
        public DateTime UpdatedAt { get; init; }
        public bool IsCustomized { get; init; }
        public bool HasAttachForm { get; init; }
    }

    public class BillSeaLayoutFormService(IDbContextFactory<AppDbContext> dbFactory, IWebHostEnvironment env)
    {
        private async Task<T> WithDbAsync<T>(
            Func<AppDbContext, CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
            return await action(context, cancellationToken);
        }

        private async Task WithDbAsync(
            Func<AppDbContext, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
            await action(context, cancellationToken);
        }

        public string GetDefaultTemplatePath(string reportFileName) =>
            Path.Combine(env.WebRootPath, "Reports", reportFileName);

        public async Task<byte[]> GetDefaultTemplateBytesAsync(string reportFileName = BillSeaReportTemplateNames.Main, CancellationToken cancellationToken = default)
        {
            var defaultPath = GetDefaultTemplatePath(reportFileName);
            if (!File.Exists(defaultPath))
                throw new FileNotFoundException($"Không tìm thấy template mặc định: {reportFileName}", defaultPath);

            return await File.ReadAllBytesAsync(defaultPath, cancellationToken);
        }

        public async Task<List<BillSeaLayoutFormSummary>> ListFormsAsync(BillLayoutFormKind? formKind = null, CancellationToken cancellationToken = default)
        {
            var forms = await WithDbAsync(async (context, ct) =>
            {
                var query = context.BillSeaLayoutForms
                    .AsNoTracking()
                    .Where(x => x.IsActive);

                if (formKind is BillLayoutFormKind kind)
                {
                    var kindValue = BillLayoutFormKindHelper.ToStorageValue(kind);
                    query = query.Where(x => x.FormKind == kindValue);
                }

                return await query
                    .OrderBy(x => x.FormName)
                    .ToListAsync(ct);
            }, cancellationToken);

            var defaultSeaBytes = await GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Main, cancellationToken);
            var defaultAirBytes = await GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Air, cancellationToken);

            return forms.Select(x =>
            {
                var kind = BillLayoutFormKindHelper.ParseFormKind(x.FormKind);
                var defaultBytes = kind == BillLayoutFormKind.Air ? defaultAirBytes : defaultSeaBytes;
                return new BillSeaLayoutFormSummary
                {
                    BillSeaLayoutFormId = x.BillSeaLayoutFormId,
                    FormName = x.FormName,
                    FormKind = kind,
                    UpdatedAt = x.UpdatedAt,
                    IsCustomized = !ContentEquals(x.MrtContent, defaultBytes),
                    HasAttachForm = x.AttachMrtContent is { Length: > 0 }
                };
            }).ToList();
        }

        public Task<M_BillSeaLayoutForm?> GetFormAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync((context, ct) =>
                context.BillSeaLayoutForms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct), cancellationToken);

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

            var defaultBytes = await GetDefaultTemplateBytesAsync(form.SourceTemplate, cancellationToken);
            return !ContentEquals(form.MrtContent, defaultBytes);
        }

        public Task<byte[]?> GetCompanyLogoAsync(CancellationToken cancellationToken = default) =>
            WithDbAsync((context, ct) =>
                context.CompanyInfomation
                    .AsNoTracking()
                    .Select(x => x.Logo)
                    .FirstOrDefaultAsync(ct), cancellationToken);

        public Task<byte[]?> GetCompanyFormBillSeaAsync(CancellationToken cancellationToken = default) =>
            WithDbAsync((context, ct) =>
                context.CompanyInfomation
                    .AsNoTracking()
                    .Select(x => x.FormBillSea)
                    .FirstOrDefaultAsync(ct), cancellationToken);

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

        public Task SaveFormLogoAsync(Guid formId, byte[]? logo, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.Logo = logo is { Length: > 0 } ? logo : null;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ClearFormLogoAsync(Guid formId, CancellationToken cancellationToken = default) =>
            SaveFormLogoAsync(formId, null, cancellationToken);

        public async Task<byte[]?> GetFormBillAirAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.FormBillAir is { Length: > 0 } ? form.FormBillAir : null;
        }

        public async Task<byte[]?> GetDefaultBillAirImage1Async(CancellationToken cancellationToken = default)
        {
            var templateBytes = await GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Air, cancellationToken);
            return BillSeaLayoutMrtHelper.ExtractImageBytes(templateBytes, "Image1");
        }

        public async Task<bool> HasCustomFormBillAirAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.FormBillAir is { Length: > 0 };
        }

        public Task SaveFormBillAirAsync(Guid formId, byte[]? formBillAir, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill.");

                form.FormBillAir = formBillAir is { Length: > 0 } ? formBillAir : null;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ClearFormBillAirAsync(Guid formId, CancellationToken cancellationToken = default) =>
            SaveFormBillAirAsync(formId, null, cancellationToken);

        public async Task<M_BillSeaLayoutForm> CreateFormAsync(
            string formName,
            BillLayoutFormKind formKind = BillLayoutFormKind.Sea,
            string? createdBy = null,
            CancellationToken cancellationToken = default)
        {
            var trimmedName = formName.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new ArgumentException("Tên form không được để trống.", nameof(formName));

            var kindValue = BillLayoutFormKindHelper.ToStorageValue(formKind);
            var templateFile = BillLayoutFormKindHelper.GetDefaultTemplateFile(formKind);
            var defaultBytes = await GetDefaultTemplateBytesAsync(templateFile, cancellationToken);

            return await WithDbAsync(async (context, ct) =>
            {
                var exists = await context.BillSeaLayoutForms
                    .AnyAsync(x => x.IsActive && x.FormName == trimmedName && x.FormKind == kindValue, ct);
                if (exists)
                    throw new InvalidOperationException($"Đã tồn tại form {BillLayoutFormKindHelper.GetDisplayName(formKind)} tên '{trimmedName}'.");

                var now = DateTime.UtcNow;
                var form = new M_BillSeaLayoutForm
                {
                    BillSeaLayoutFormId = Guid.NewGuid(),
                    FormName = trimmedName,
                    FormKind = kindValue,
                    MrtContent = defaultBytes,
                    SourceTemplate = templateFile,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = createdBy,
                    IsActive = true
                };

                context.BillSeaLayoutForms.Add(form);
                await context.SaveChangesAsync(ct);
                return form;
            }, cancellationToken);
        }

        public Task SaveFormDocumentAsync(Guid formId, XDocument document, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.MrtContent = DocumentToBytes(document);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ResetFormToDefaultAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.MrtContent = await GetDefaultTemplateBytesAsync(form.SourceTemplate, ct);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task DeleteFormAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    return;

                form.IsActive = false;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

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

        public Task<M_BillSeaLayoutForm> CreateAttachFormAsync(Guid mainFormId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == mainFormId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                if (BillLayoutFormKindHelper.ParseFormKind(form.FormKind) != BillLayoutFormKind.Sea)
                    throw new InvalidOperationException("Form Attach chỉ áp dụng cho loại Sea.");

                if (form.AttachMrtContent is { Length: > 0 })
                    throw new InvalidOperationException($"Form '{form.FormName}' đã có form Attach.");

                form.AttachMrtContent = await BuildDefaultAttachBytesAsync(form.MrtContent, ct);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
                return form;
            }, cancellationToken);

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

        public Task SaveAttachFormDocumentAsync(Guid formId, XDocument document, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.AttachMrtContent = DocumentToBytes(document);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ResetAttachFormToDefaultAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");
                if (form.AttachMrtContent is not { Length: > 0 })
                    throw new InvalidOperationException("Form này chưa có form Attach.");

                form.AttachMrtContent = await BuildDefaultAttachBytesAsync(form.MrtContent, ct);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task DeleteAttachFormAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    return;

                form.AttachMrtContent = null;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

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
