using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Accounting;

namespace NVOAMASIS.Services.Accounting;

public sealed class FixedAssetService : IFixedAssetService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FixedAssetService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IReadOnlyList<FixedAsset>> SearchAsync(
        Guid companyId,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var query = db.FixedAssets
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        keyword = keyword?.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.AssetCode.Contains(keyword) ||
                x.AssetName.Contains(keyword) ||
                (x.SerialNo != null && x.SerialNo.Contains(keyword)) ||
                (x.DepartmentCode != null && x.DepartmentCode.Contains(keyword)));
        }

        return await query
            .OrderByDescending(x => x.RecognitionDate)
            .ThenBy(x => x.AssetCode)
            .Take(500)
            .ToListAsync(cancellationToken);
    }

    public async Task<FixedAsset?> GetAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.FixedAssets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.CompanyId == companyId && x.Id == id,
                cancellationToken);
    }

    public async Task<FixedAsset> SaveAsync(
        Guid companyId,
        FixedAsset model,
        string userName,
        CancellationToken cancellationToken = default)
    {
        ValidateAndCalculate(model);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var normalizedCode = model.AssetCode.Trim().ToUpperInvariant();

        var duplicated = await db.FixedAssets.AnyAsync(
            x => x.CompanyId == companyId &&
                 x.AssetCode == normalizedCode &&
                 x.Id != model.Id,
            cancellationToken);

        if (duplicated)
            throw new InvalidOperationException(
                $"Mã tài sản '{normalizedCode}' đã tồn tại trong công ty.");

        FixedAsset entity;

        if (model.Id == Guid.Empty)
        {
            entity = new FixedAsset
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userName
            };

            CopyEditableFields(entity, model);
            db.FixedAssets.Add(entity);
        }
        else
        {
            entity = await db.FixedAssets.SingleOrDefaultAsync(
                x => x.CompanyId == companyId && x.Id == model.Id,
                cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy tài sản cần cập nhật.");

            if (model.RowVersion.Length > 0 &&
                !entity.RowVersion.SequenceEqual(model.RowVersion))
            {
                throw new DbUpdateConcurrencyException(
                    "Tài sản đã được người khác cập nhật. Hãy tải lại dữ liệu.");
            }

            CopyEditableFields(entity, model);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userName;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException(
                "Dữ liệu đã thay đổi trong lúc lưu. Hãy tải lại và thao tác lại.");
        }

        return entity;
    }

    public async Task DeleteDraftAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entity = await db.FixedAssets.SingleOrDefaultAsync(
            x => x.CompanyId == companyId && x.Id == id,
            cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tài sản.");

        if (entity.Status != FixedAssetStatus.Draft)
            throw new InvalidOperationException(
                "Chỉ được xóa tài sản ở trạng thái Nháp. " +
                "Tài sản đã đưa vào sử dụng phải thực hiện nghiệp vụ ghi giảm/thanh lý.");

        db.FixedAssets.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateAndCalculate(FixedAsset model)
    {
        model.AssetCode = (model.AssetCode ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        model.AssetName = (model.AssetName ?? string.Empty).Trim();
        model.AssetGroupCode = NormalizeNullable(model.AssetGroupCode);
        model.SerialNo = NormalizeNullable(model.SerialNo);
        model.ModelNo = NormalizeNullable(model.ModelNo);
        model.Manufacturer = NormalizeNullable(model.Manufacturer);
        model.CountryOfOrigin = NormalizeNullable(model.CountryOfOrigin);
        model.DepartmentCode = NormalizeNullable(model.DepartmentCode);
        model.Location = NormalizeNullable(model.Location);
        model.Custodian = NormalizeNullable(model.Custodian);
        model.SupplierCode = NormalizeNullable(model.SupplierCode);
        model.InvoiceNo = NormalizeNullable(model.InvoiceNo);
        model.VoucherNo = NormalizeNullable(model.VoucherNo);
        model.SourceAccount = NormalizeNullable(model.SourceAccount);
        model.Description = NormalizeNullable(model.Description);
        model.Notes = NormalizeNullable(model.Notes);

        model.AssetAccount = (model.AssetAccount ?? string.Empty).Trim();
        model.DepreciationAccount =
            (model.DepreciationAccount ?? string.Empty).Trim();
        model.ExpenseAccount =
            (model.ExpenseAccount ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(model.AssetCode))
            throw new InvalidOperationException("Mã tài sản không được để trống.");

        if (string.IsNullOrWhiteSpace(model.AssetName))
            throw new InvalidOperationException("Tên tài sản không được để trống.");

        if (string.IsNullOrWhiteSpace(model.AssetAccount) ||
            string.IsNullOrWhiteSpace(model.DepreciationAccount) ||
            string.IsNullOrWhiteSpace(model.ExpenseAccount))
        {
            throw new InvalidOperationException(
                "Phải khai báo tài khoản nguyên giá, hao mòn và chi phí khấu hao.");
        }

        model.PurchaseDate = model.PurchaseDate?.Date;
        model.RecognitionDate = model.RecognitionDate.Date;
        model.DepreciationStartDate = model.DepreciationStartDate.Date;
        model.InvoiceDate = model.InvoiceDate?.Date;
        model.VoucherDate = model.VoucherDate?.Date;

        if (model.DepreciationStartDate < model.RecognitionDate)
            throw new InvalidOperationException(
                "Ngày bắt đầu khấu hao không được trước ngày ghi nhận tài sản.");

        var costs = new[]
        {
            model.PurchasePrice,
            model.NonRefundableTax,
            model.TransportCost,
            model.InstallationCost,
            model.OtherDirectCost,
            model.DiscountAmount,
            model.RecoverableVat,
            model.ResidualValue,
            model.OpeningAccumulatedDepreciation
        };

        if (costs.Any(x => x < 0))
            throw new InvalidOperationException("Các giá trị tiền không được âm.");

        model.OriginalCost = RoundMoney(
            model.PurchasePrice +
            model.NonRefundableTax +
            model.TransportCost +
            model.InstallationCost +
            model.OtherDirectCost -
            model.DiscountAmount);

        if (model.OriginalCost <= 0)
            throw new InvalidOperationException("Nguyên giá phải lớn hơn 0.");

        if (model.ResidualValue > model.OriginalCost)
            throw new InvalidOperationException(
                "Giá trị thu hồi không được lớn hơn nguyên giá.");

        if (model.DepreciationMethod != DepreciationMethod.NotDepreciated &&
            model.UsefulLifeMonths <= 0)
        {
            throw new InvalidOperationException(
                "Thời gian sử dụng phải lớn hơn 0 tháng.");
        }

        var maximumAccumulated =
            model.OriginalCost - model.ResidualValue;

        if (model.OpeningAccumulatedDepreciation > maximumAccumulated)
        {
            throw new InvalidOperationException(
                "Hao mòn lũy kế đầu kỳ vượt quá giá trị được khấu hao.");
        }

        model.RemainingValue = RoundMoney(
            model.OriginalCost -
            model.OpeningAccumulatedDepreciation);
    }

    private static void CopyEditableFields(
        FixedAsset target,
        FixedAsset source)
    {
        target.AssetCode = source.AssetCode;
        target.AssetName = source.AssetName;
        target.AssetType = source.AssetType;
        target.AssetGroupCode = source.AssetGroupCode;
        target.Description = source.Description;
        target.SerialNo = source.SerialNo;
        target.ModelNo = source.ModelNo;
        target.Manufacturer = source.Manufacturer;
        target.CountryOfOrigin = source.CountryOfOrigin;
        target.DepartmentCode = source.DepartmentCode;
        target.Location = source.Location;
        target.Custodian = source.Custodian;

        target.PurchaseDate = source.PurchaseDate;
        target.RecognitionDate = source.RecognitionDate;
        target.DepreciationStartDate = source.DepreciationStartDate;

        target.PurchasePrice = source.PurchasePrice;
        target.NonRefundableTax = source.NonRefundableTax;
        target.TransportCost = source.TransportCost;
        target.InstallationCost = source.InstallationCost;
        target.OtherDirectCost = source.OtherDirectCost;
        target.DiscountAmount = source.DiscountAmount;
        target.RecoverableVat = source.RecoverableVat;
        target.OriginalCost = source.OriginalCost;
        target.ResidualValue = source.ResidualValue;
        target.UsefulLifeMonths = source.UsefulLifeMonths;
        target.DepreciationMethod = source.DepreciationMethod;
        target.OpeningAccumulatedDepreciation =
            source.OpeningAccumulatedDepreciation;
        target.RemainingValue = source.RemainingValue;

        target.AssetAccount = source.AssetAccount;
        target.DepreciationAccount = source.DepreciationAccount;
        target.ExpenseAccount = source.ExpenseAccount;
        target.SourceAccount = source.SourceAccount;

        target.SupplierCode = source.SupplierCode;
        target.InvoiceNo = source.InvoiceNo;
        target.InvoiceDate = source.InvoiceDate;
        target.VoucherNo = source.VoucherNo;
        target.VoucherDate = source.VoucherDate;

        target.IsOpeningBalance = source.IsOpeningBalance;
        target.Status = source.Status;
        target.Notes = source.Notes;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal RoundMoney(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
