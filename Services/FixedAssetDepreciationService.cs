using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Accounting;

namespace NVOAMASIS.Services.Accounting;

public sealed class FixedAssetDepreciationService
    : IFixedAssetDepreciationService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FixedAssetDepreciationService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IReadOnlyList<FixedAssetDepreciationRow>> CalculateAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        bool prorateByDay,
        CancellationToken cancellationToken = default)
    {
        ValidateCompanyAndPeriod(companyId, fiscalYear, fiscalPeriod);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await CalculateInternalAsync(
            db,
            companyId,
            fiscalYear,
            fiscalPeriod,
            prorateByDay,
            cancellationToken);
    }

    public async Task<int> SaveDraftAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        bool prorateByDay,
        IReadOnlyCollection<FixedAssetDepreciationRow> rows,
        string userName,
        CancellationToken cancellationToken = default)
    {
        ValidateCompanyAndPeriod(companyId, fiscalYear, fiscalPeriod);
        ArgumentNullException.ThrowIfNull(rows);

        userName = string.IsNullOrWhiteSpace(userName)
            ? "system"
            : userName.Trim();

        var requestedRows = rows
            .Where(x => x.Selected)
            .GroupBy(x => x.FixedAssetId)
            .Select(x => x.First())
            .ToList();

        if (requestedRows.Count == 0)
        {
            throw new InvalidOperationException(
                "Chưa chọn tài sản cần lưu khấu hao.");
        }

        if (requestedRows.Any(x => x.FixedAssetId == Guid.Empty))
        {
            throw new InvalidOperationException(
                "Danh sách có tài sản không hợp lệ.");
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            // Luôn tính lại trên server để không tin số tiền gửi từ giao diện.
            var calculatedRows = await CalculateInternalAsync(
                db,
                companyId,
                fiscalYear,
                fiscalPeriod,
                prorateByDay,
                cancellationToken);

            var calculatedByAssetId = calculatedRows.ToDictionary(
                x => x.FixedAssetId);

            var requestedIds = requestedRows
                .Select(x => x.FixedAssetId)
                .ToList();

            var existingRecords = await db.FixedAssetDepreciations
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.FiscalYear == fiscalYear &&
                    x.FiscalPeriod == fiscalPeriod &&
                    requestedIds.Contains(x.FixedAssetId))
                .ToDictionaryAsync(
                    x => x.FixedAssetId,
                    cancellationToken);

            var savedCount = 0;

            foreach (var requested in requestedRows)
            {
                if (!calculatedByAssetId.TryGetValue(
                        requested.FixedAssetId,
                        out var calculated))
                {
                    throw new InvalidOperationException(
                        "Tài sản không còn đủ điều kiện tính khấu hao.");
                }

                if (!calculated.CanSave || calculated.DepreciationAmount <= 0)
                {
                    throw new InvalidOperationException(
                        $"Tài sản {calculated.AssetCode}: " +
                        (calculated.Message ?? "không thể lưu khấu hao."));
                }

                var expenseAccount = NormalizeAccount(
                    requested.ExpenseAccount,
                    "tài khoản chi phí khấu hao");

                var depreciationAccount = NormalizeAccount(
                    requested.DepreciationAccount,
                    "tài khoản hao mòn");

                if (!existingRecords.TryGetValue(
                        requested.FixedAssetId,
                        out var entity))
                {
                    entity = new FixedAssetDepreciationRecord
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = companyId,
                        FixedAssetId = requested.FixedAssetId,
                        FiscalYear = fiscalYear,
                        FiscalPeriod = fiscalPeriod,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userName,
                        Status = FixedAssetDepreciationStatus.Draft
                    };

                    db.FixedAssetDepreciations.Add(entity);
                    existingRecords.Add(requested.FixedAssetId, entity);
                }
                else
                {
                    if (entity.Status == FixedAssetDepreciationStatus.Posted)
                    {
                        throw new InvalidOperationException(
                            $"Tài sản {calculated.AssetCode} đã ghi sổ " +
                            $"khấu hao kỳ {fiscalPeriod:00}/{fiscalYear}.");
                    }

                    if (requested.RowVersion is { Length: > 0 })
                    {
                        db.Entry(entity)
                            .Property(x => x.RowVersion)
                            .OriginalValue = requested.RowVersion;
                    }

                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = userName;
                }

                entity.PeriodStartDate = new DateTime(
                    fiscalYear,
                    fiscalPeriod,
                    1);

                entity.PeriodEndDate = entity.PeriodStartDate
                    .AddMonths(1)
                    .AddDays(-1);

                entity.DepreciationMethod = calculated.DepreciationMethod;
                entity.ProrateByDay = prorateByDay;
                entity.DaysInPeriod = calculated.DaysInPeriod;
                entity.DepreciableDays = calculated.DepreciableDays;

                entity.OriginalCost = calculated.OriginalCost;
                entity.ResidualValue = calculated.ResidualValue;
                entity.MonthlyDepreciation = calculated.MonthlyDepreciation;
                entity.OpeningAccumulatedDepreciation =
                    calculated.OpeningAccumulatedDepreciation;
                entity.DepreciationAmount = calculated.DepreciationAmount;
                entity.ClosingAccumulatedDepreciation =
                    calculated.ClosingAccumulatedDepreciation;
                entity.ClosingRemainingValue =
                    calculated.ClosingRemainingValue;

                entity.ExpenseAccount = expenseAccount;
                entity.DepreciationAccount = depreciationAccount;
                entity.Status = FixedAssetDepreciationStatus.Draft;
                entity.VoucherId = null;
                entity.VoucherNo = null;
                entity.PostedAt = null;
                entity.PostedBy = null;

                savedCount++;
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return savedCount;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new DbUpdateConcurrencyException(
                "Kết quả khấu hao đã được người khác thay đổi. " +
                "Vui lòng tải lại dữ liệu trước khi lưu.",
                ex);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Kỳ {fiscalPeriod:00}/{fiscalYear} đã có dữ liệu " +
                "khấu hao của một hoặc nhiều tài sản. Hãy tải lại rồi lưu lại.",
                ex);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> DeleteDraftAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        IReadOnlyCollection<Guid> fixedAssetIds,
        CancellationToken cancellationToken = default)
    {
        ValidateCompanyAndPeriod(companyId, fiscalYear, fiscalPeriod);
        ArgumentNullException.ThrowIfNull(fixedAssetIds);

        var ids = fixedAssetIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            throw new InvalidOperationException(
                "Chưa chọn bản nháp khấu hao cần xóa.");
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var records = await db.FixedAssetDepreciations
            .Where(x =>
                x.CompanyId == companyId &&
                x.FiscalYear == fiscalYear &&
                x.FiscalPeriod == fiscalPeriod &&
                x.Status == FixedAssetDepreciationStatus.Draft &&
                ids.Contains(x.FixedAssetId))
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
            return 0;

        db.FixedAssetDepreciations.RemoveRange(records);
        await db.SaveChangesAsync(cancellationToken);

        return records.Count;
    }

    private static async Task<IReadOnlyList<FixedAssetDepreciationRow>>
        CalculateInternalAsync(
            AppDbContext db,
            Guid companyId,
            int fiscalYear,
            int fiscalPeriod,
            bool prorateByDay,
            CancellationToken cancellationToken)
    {
        var periodStart = new DateTime(fiscalYear, fiscalPeriod, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var daysInPeriod = DateTime.DaysInMonth(fiscalYear, fiscalPeriod);

        var assets = await db.FixedAssets
            .AsNoTracking()
            .Where(x =>
                x.CompanyId == companyId &&
                x.Status == FixedAssetStatus.InUse &&
                x.DepreciationMethod != DepreciationMethod.NotDepreciated &&
                x.DepreciationStartDate <= periodEnd &&
                x.OriginalCost > x.ResidualValue)
            .OrderBy(x => x.AssetCode)
            .ToListAsync(cancellationToken);

        if (assets.Count == 0)
            return Array.Empty<FixedAssetDepreciationRow>();

        var assetIds = assets.Select(x => x.Id).ToList();

        var recordedRows = await db.FixedAssetDepreciations
            .AsNoTracking()
            .Where(x =>
                x.CompanyId == companyId &&
                assetIds.Contains(x.FixedAssetId) &&
                x.PeriodStartDate <= periodEnd)
            .ToListAsync(cancellationToken);

        var recordsByAssetId = recordedRows
            .GroupBy(x => x.FixedAssetId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var result = new List<FixedAssetDepreciationRow>(assets.Count);

        foreach (var asset in assets)
        {
            recordsByAssetId.TryGetValue(asset.Id, out var assetRecords);
            assetRecords ??= [];

            var currentRecord = assetRecords.SingleOrDefault(x =>
                x.FiscalYear == fiscalYear &&
                x.FiscalPeriod == fiscalPeriod);

            if (currentRecord?.Status == FixedAssetDepreciationStatus.Posted)
            {
                result.Add(CreatePostedRow(asset, currentRecord));
                continue;
            }

            var priorRecordedDepreciation = assetRecords
                .Where(x => x.PeriodEndDate < periodStart)
                .Sum(x => x.DepreciationAmount);

            var depreciableBase = RoundMoney(
                Math.Max(0, asset.OriginalCost - asset.ResidualValue));

            var openingAccumulated = RoundMoney(
                Math.Min(
                    depreciableBase,
                    asset.OpeningAccumulatedDepreciation +
                    priorRecordedDepreciation));

            var remainingDepreciable = RoundMoney(
                Math.Max(0, depreciableBase - openingAccumulated));

            var row = new FixedAssetDepreciationRow
            {
                DepreciationId = currentRecord?.Id,
                FixedAssetId = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                DepartmentCode = asset.DepartmentCode,
                DepreciationStartDate = asset.DepreciationStartDate,
                DepreciationMethod = asset.DepreciationMethod,
                UsefulLifeMonths = asset.UsefulLifeMonths,
                OriginalCost = asset.OriginalCost,
                ResidualValue = asset.ResidualValue,
                OpeningAccumulatedDepreciation = openingAccumulated,
                ExpenseAccount = currentRecord?.ExpenseAccount
                    ?? asset.ExpenseAccount,
                DepreciationAccount = currentRecord?.DepreciationAccount
                    ?? asset.DepreciationAccount,
                SavedStatus = currentRecord?.Status,
                IsSavedDraft = currentRecord?.Status ==
                    FixedAssetDepreciationStatus.Draft,
                IsPosted = false,
                DaysInPeriod = daysInPeriod,
                RowVersion = currentRecord?.RowVersion ?? Array.Empty<byte>()
            };

            if (asset.DepreciationMethod != DepreciationMethod.StraightLine)
            {
                row.CanSave = false;
                row.Selected = false;
                row.DepreciableDays = 0;
                row.DepreciationAmount = 0;
                row.ClosingAccumulatedDepreciation = openingAccumulated;
                row.ClosingRemainingValue = RoundMoney(
                    Math.Max(
                        asset.ResidualValue,
                        asset.OriginalCost - openingAccumulated));
                row.Message =
                    "Màn hình hiện chỉ tính tự động phương pháp đường thẳng.";

                result.Add(row);
                continue;
            }

            if (asset.UsefulLifeMonths <= 0)
            {
                row.CanSave = false;
                row.Selected = false;
                row.Message = "Thời gian sử dụng phải lớn hơn 0 tháng.";
                row.ClosingAccumulatedDepreciation = openingAccumulated;
                row.ClosingRemainingValue = RoundMoney(
                    Math.Max(
                        asset.ResidualValue,
                        asset.OriginalCost - openingAccumulated));

                result.Add(row);
                continue;
            }

            if (remainingDepreciable <= 0)
            {
                row.CanSave = false;
                row.Selected = false;
                row.DepreciableDays = 0;
                row.MonthlyDepreciation = 0;
                row.DepreciationAmount = 0;
                row.ClosingAccumulatedDepreciation = openingAccumulated;
                row.ClosingRemainingValue = asset.ResidualValue;
                row.Message = "Tài sản đã khấu hao đủ đến giá trị thu hồi.";

                result.Add(row);
                continue;
            }

            var monthlyDepreciation = RoundMoney(
                depreciableBase / asset.UsefulLifeMonths);

            var activeDate = asset.DepreciationStartDate > periodStart
                ? asset.DepreciationStartDate.Date
                : periodStart;

            var depreciableDays = prorateByDay
                ? Math.Max(0, (periodEnd - activeDate).Days + 1)
                : daysInPeriod;

            depreciableDays = Math.Min(daysInPeriod, depreciableDays);

            var calculatedAmount = prorateByDay
                ? RoundMoney(
                    monthlyDepreciation *
                    depreciableDays /
                    daysInPeriod)
                : monthlyDepreciation;

            calculatedAmount = RoundMoney(
                Math.Min(remainingDepreciable, calculatedAmount));

            row.MonthlyDepreciation = monthlyDepreciation;
            row.DepreciableDays = depreciableDays;
            row.DepreciationAmount = calculatedAmount;
            row.ClosingAccumulatedDepreciation = RoundMoney(
                openingAccumulated + calculatedAmount);
            row.ClosingRemainingValue = RoundMoney(
                Math.Max(
                    asset.ResidualValue,
                    asset.OriginalCost -
                    row.ClosingAccumulatedDepreciation));
            row.CanSave = calculatedAmount > 0;
            row.Selected = row.CanSave;
            row.Message = currentRecord is null
                ? null
                : "Đã có bản nháp; lưu lại sẽ cập nhật kết quả.";

            result.Add(row);
        }

        return result;
    }

    private static FixedAssetDepreciationRow CreatePostedRow(
        FixedAsset asset,
        FixedAssetDepreciationRecord record)
    {
        return new FixedAssetDepreciationRow
        {
            DepreciationId = record.Id,
            FixedAssetId = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            DepartmentCode = asset.DepartmentCode,
            DepreciationStartDate = asset.DepreciationStartDate,
            DepreciationMethod = record.DepreciationMethod,
            UsefulLifeMonths = asset.UsefulLifeMonths,
            OriginalCost = record.OriginalCost,
            ResidualValue = record.ResidualValue,
            MonthlyDepreciation = record.MonthlyDepreciation,
            OpeningAccumulatedDepreciation =
                record.OpeningAccumulatedDepreciation,
            DepreciationAmount = record.DepreciationAmount,
            ClosingAccumulatedDepreciation =
                record.ClosingAccumulatedDepreciation,
            ClosingRemainingValue = record.ClosingRemainingValue,
            DaysInPeriod = record.DaysInPeriod,
            DepreciableDays = record.DepreciableDays,
            ExpenseAccount = record.ExpenseAccount,
            DepreciationAccount = record.DepreciationAccount,
            SavedStatus = record.Status,
            IsSavedDraft = false,
            IsPosted = true,
            CanSave = false,
            Selected = false,
            Message = string.IsNullOrWhiteSpace(record.VoucherNo)
                ? "Kỳ này đã được ghi sổ."
                : $"Đã ghi sổ theo chứng từ {record.VoucherNo}.",
            RowVersion = record.RowVersion
        };
    }

    private static void ValidateCompanyAndPeriod(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod)
    {
        if (companyId == Guid.Empty)
            throw new InvalidOperationException("Không xác định được công ty.");

        if (fiscalYear is < 2000 or > 9999)
            throw new InvalidOperationException("Năm tài chính không hợp lệ.");

        if (fiscalPeriod is < 1 or > 12)
            throw new InvalidOperationException("Kỳ kế toán phải từ 1 đến 12.");
    }

    private static string NormalizeAccount(
        string? value,
        string fieldName)
    {
        value = value?.Trim();

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Chưa nhập {fieldName}.");

        if (value.Length > 20)
        {
            throw new InvalidOperationException(
                $"{fieldName} không được vượt quá 20 ký tự.");
        }

        return value;
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

    private static decimal RoundMoney(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
