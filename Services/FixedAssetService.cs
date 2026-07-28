using Microsoft.Data.SqlClient;
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
        // Giữ tham số companyId để tương thích IFixedAssetService hiện tại.
        // Hệ thống TSCĐ dùng chung dữ liệu, không phân tách theo công ty.
        _ = companyId;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var query = db.FixedAssets
            .AsNoTracking()
            .AsQueryable();

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
        // Giữ tham số companyId để tương thích IFixedAssetService hiện tại.
        _ = companyId;

        if (id == Guid.Empty)
            return null;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.FixedAssets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<FixedAsset> SaveAsync(
        Guid companyId,
        FixedAsset model,
        string userName,
        CancellationToken cancellationToken = default)
    {
        // Giữ tham số companyId để tương thích IFixedAssetService hiện tại.
        // Không kiểm tra hoặc dùng CompanyId để phân tách dữ liệu.
        _ = companyId;

        ArgumentNullException.ThrowIfNull(model);

        userName = string.IsNullOrWhiteSpace(userName)
            ? "system"
            : userName.Trim();

        // Luôn tính và kiểm tra lại ở server, không tin số liệu gửi từ giao diện.
        ValidateAndCalculate(model);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var transaction = await db.Database.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var normalizedCode = model.AssetCode;

            var duplicated = await db.FixedAssets
                .AsNoTracking()
                .AnyAsync(
                    x => x.AssetCode == normalizedCode &&
                         x.Id != model.Id,
                    cancellationToken);

            if (duplicated)
            {
                throw new InvalidOperationException(
                    $"Mã tài sản '{normalizedCode}' đã tồn tại.");
            }

            FixedAsset entity;

            if (model.Id == Guid.Empty)
            {
                entity = new FixedAsset
                {
                    Id = Guid.NewGuid(),
                    // Cột CompanyId vẫn tồn tại trong entity/schema cũ.
                    // Dùng Guid.Empty làm giá trị chung toàn hệ thống.
                    CompanyId = Guid.Empty,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName
                };

                CopyDraftFields(entity, model);
                entity.Status = GetRequestedInitialStatus(model.Status);

                db.FixedAssets.Add(entity);
            }
            else
            {
                entity = await db.FixedAssets
                    .SingleOrDefaultAsync(
                        x => x.Id == model.Id,
                        cancellationToken)
                    ?? throw new KeyNotFoundException(
                        "Không tìm thấy tài sản cần cập nhật.");

                // RowVersion của bản người dùng đang sửa được dùng làm điều kiện UPDATE.
                // Nếu dữ liệu đã bị người khác thay đổi, SaveChanges sẽ phát hiện xung đột.
                if (model.RowVersion is { Length: > 0 })
                {
                    db.Entry(entity)
                        .Property(x => x.RowVersion)
                        .OriginalValue = model.RowVersion;
                }

                if (entity.Status == FixedAssetStatus.Draft)
                {
                    CopyDraftFields(entity, model);
                    entity.Status = GetRequestedInitialStatus(model.Status);
                }
                else
                {
                    // Tài sản đã đưa vào sử dụng không được sửa số liệu kế toán
                    // bằng nút Lưu thông thường.
                    EnsureAccountingFieldsNotChanged(entity, model);
                    CopyManagementFields(entity, model);
                }

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = userName;
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // SQL Server đã cập nhật RowVersion mới trên entity sau SaveChanges.
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new DbUpdateConcurrencyException(
                "Tài sản đã được người khác thay đổi. " +
                "Vui lòng tải lại dữ liệu trước khi lưu.",
                ex);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Mã tài sản '{model.AssetCode}' đã tồn tại.",
                ex);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteDraftAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Giữ tham số companyId để tương thích IFixedAssetService hiện tại.
        _ = companyId;

        if (id == Guid.Empty)
            throw new InvalidOperationException("Mã tài sản không hợp lệ.");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entity = await db.FixedAssets.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tài sản.");

        if (entity.Status != FixedAssetStatus.Draft)
        {
            throw new InvalidOperationException(
                "Chỉ được xóa tài sản ở trạng thái Nháp. " +
                "Tài sản đã đưa vào sử dụng phải thực hiện nghiệp vụ ghi giảm/thanh lý.");
        }

        db.FixedAssets.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static FixedAssetStatus GetRequestedInitialStatus(
        FixedAssetStatus requestedStatus)
    {
        // Chỉ nút "Lưu & đưa vào sử dụng" được phép gửi InUse.
        // Các trạng thái tạm ngưng/ghi giảm phải đi qua nghiệp vụ riêng.
        return requestedStatus == FixedAssetStatus.InUse
            ? FixedAssetStatus.InUse
            : FixedAssetStatus.Draft;
    }

    private static void ValidateAndCalculate(FixedAsset model)
    {
        model.AssetCode = (model.AssetCode ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        model.AssetName = (model.AssetName ?? string.Empty).Trim();
        model.AssetGroupCode = NormalizeNullable(model.AssetGroupCode);
        model.Description = NormalizeNullable(model.Description);
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

        if (model.RecognitionDate == default)
            throw new InvalidOperationException("Ngày ghi nhận tài sản không hợp lệ.");

        if (model.DepreciationStartDate == default)
            throw new InvalidOperationException("Ngày bắt đầu khấu hao không hợp lệ.");

        if (string.IsNullOrWhiteSpace(model.AssetAccount))
            throw new InvalidOperationException("Tài khoản nguyên giá không được để trống.");

        if (string.IsNullOrWhiteSpace(model.DepreciationAccount))
            throw new InvalidOperationException("Tài khoản hao mòn không được để trống.");

        if (string.IsNullOrWhiteSpace(model.ExpenseAccount))
        {
            throw new InvalidOperationException(
                "Tài khoản chi phí khấu hao không được để trống.");
        }

        model.PurchaseDate = model.PurchaseDate?.Date;
        model.RecognitionDate = model.RecognitionDate.Date;
        model.DepreciationStartDate = model.DepreciationStartDate.Date;
        model.InvoiceDate = model.InvoiceDate?.Date;
        model.VoucherDate = model.VoucherDate?.Date;

        if (model.DepreciationStartDate < model.RecognitionDate)
        {
            throw new InvalidOperationException(
                "Ngày bắt đầu khấu hao không được trước ngày ghi nhận tài sản.");
        }

        var moneyValues = new[]
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

        if (moneyValues.Any(x => x < 0))
            throw new InvalidOperationException("Các giá trị tiền không được âm.");

        var totalFormationCost =
            model.PurchasePrice +
            model.NonRefundableTax +
            model.TransportCost +
            model.InstallationCost +
            model.OtherDirectCost;

        if (model.DiscountAmount > totalFormationCost)
        {
            throw new InvalidOperationException(
                "Chiết khấu/giảm giá vượt quá tổng chi phí hình thành tài sản.");
        }

        // VAT được khấu trừ không cộng vào nguyên giá.
        model.OriginalCost = RoundMoney(
            totalFormationCost - model.DiscountAmount);

        if (model.OriginalCost <= 0)
            throw new InvalidOperationException("Nguyên giá phải lớn hơn 0.");

        if (model.ResidualValue > model.OriginalCost)
        {
            throw new InvalidOperationException(
                "Giá trị thu hồi không được lớn hơn nguyên giá.");
        }

        if (model.DepreciationMethod != DepreciationMethod.NotDepreciated &&
            model.UsefulLifeMonths <= 0)
        {
            throw new InvalidOperationException(
                "Thời gian sử dụng phải lớn hơn 0 tháng.");
        }

        var maximumAccumulatedDepreciation =
            model.OriginalCost - model.ResidualValue;

        if (model.OpeningAccumulatedDepreciation >
            maximumAccumulatedDepreciation)
        {
            throw new InvalidOperationException(
                "Hao mòn lũy kế đầu kỳ vượt quá giá trị được khấu hao.");
        }

        model.RemainingValue = RoundMoney(
            model.OriginalCost - model.OpeningAccumulatedDepreciation);
    }

    private static void CopyDraftFields(
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
        target.Notes = source.Notes;
    }

    private static void CopyManagementFields(
        FixedAsset target,
        FixedAsset source)
    {
        target.AssetName = source.AssetName;
        target.AssetGroupCode = source.AssetGroupCode;
        target.Description = source.Description;
        target.SerialNo = source.SerialNo;
        target.ModelNo = source.ModelNo;
        target.Manufacturer = source.Manufacturer;
        target.CountryOfOrigin = source.CountryOfOrigin;
        target.DepartmentCode = source.DepartmentCode;
        target.Location = source.Location;
        target.Custodian = source.Custodian;
        target.Notes = source.Notes;
    }

    private static void EnsureAccountingFieldsNotChanged(
        FixedAsset current,
        FixedAsset input)
    {
        var changed =
            current.AssetCode != input.AssetCode ||
            current.AssetType != input.AssetType ||
            current.PurchaseDate?.Date != input.PurchaseDate?.Date ||
            current.RecognitionDate.Date != input.RecognitionDate.Date ||
            current.DepreciationStartDate.Date !=
                input.DepreciationStartDate.Date ||
            current.PurchasePrice != input.PurchasePrice ||
            current.NonRefundableTax != input.NonRefundableTax ||
            current.TransportCost != input.TransportCost ||
            current.InstallationCost != input.InstallationCost ||
            current.OtherDirectCost != input.OtherDirectCost ||
            current.DiscountAmount != input.DiscountAmount ||
            current.RecoverableVat != input.RecoverableVat ||
            current.OriginalCost != input.OriginalCost ||
            current.ResidualValue != input.ResidualValue ||
            current.UsefulLifeMonths != input.UsefulLifeMonths ||
            current.DepreciationMethod != input.DepreciationMethod ||
            current.OpeningAccumulatedDepreciation !=
                input.OpeningAccumulatedDepreciation ||
            current.RemainingValue != input.RemainingValue ||
            current.AssetAccount != input.AssetAccount ||
            current.DepreciationAccount != input.DepreciationAccount ||
            current.ExpenseAccount != input.ExpenseAccount ||
            current.SourceAccount != input.SourceAccount ||
            current.SupplierCode != input.SupplierCode ||
            current.InvoiceNo != input.InvoiceNo ||
            current.InvoiceDate?.Date != input.InvoiceDate?.Date ||
            current.VoucherNo != input.VoucherNo ||
            current.VoucherDate?.Date != input.VoucherDate?.Date ||
            current.IsOpeningBalance != input.IsOpeningBalance;

        if (changed)
        {
            throw new InvalidOperationException(
                "Tài sản đã đưa vào sử dụng nên không thể sửa nguyên giá, " +
                "tài khoản, chứng từ hoặc thông tin khấu hao bằng nút Lưu. " +
                "Hãy thực hiện nghiệp vụ điều chỉnh tài sản.");
        }
    }

    private static bool IsDuplicateKeyException(DbUpdateException exception)
    {
        Exception? current = exception;

        while (current is not null)
        {
            if (current is SqlException sqlException &&
                sqlException.Number is 2601 or 2627)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal RoundMoney(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}