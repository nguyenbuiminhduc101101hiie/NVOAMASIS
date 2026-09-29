using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        _ = companyId; // Giữ tham số để tương thích interface cũ.
        ValidatePeriod(fiscalYear, fiscalPeriod);

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
        _ = companyId; // Giữ tham số để tương thích interface cũ.
        ValidatePeriod(fiscalYear, fiscalPeriod);
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

        // Khóa sổ kỳ kế toán (10.8)
        await AccountingPeriodLock.EnsureOpenAsync(db, fiscalYear, fiscalPeriod, $"lưu nháp khấu hao kỳ {fiscalPeriod:D2}/{fiscalYear}", cancellationToken);

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
                .ToHashSet();

            // Không dùng requestedIds.Contains(...) trực tiếp trong EF query.
            // EF Core mới có thể dịch thành OPENJSON(... WITH ...), gây lỗi
            // trên SQL Server có compatibility level cũ.
            var existingRecordList = await db.FixedAssetDepreciations
                .Where(x =>
                    x.FiscalYear == fiscalYear &&
                    x.FiscalPeriod == fiscalPeriod)
                .ToListAsync(cancellationToken);

            var existingRecords = existingRecordList
                .Where(x => requestedIds.Contains(x.FixedAssetId))
                .ToDictionary(x => x.FixedAssetId);

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
                        // Dữ liệu TSCĐ dùng chung toàn hệ thống.
                        // Giữ Guid.Empty để tương thích cột CompanyId hiện có.
                        CompanyId = Guid.Empty,
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

    public async Task<FixedAssetDepreciationPostResult> PostDraftAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        IReadOnlyCollection<Guid> fixedAssetIds,
        string userName,
        CancellationToken cancellationToken = default)
    {
        _ = companyId; // Giữ tham số để tương thích interface cũ.
        ValidatePeriod(fiscalYear, fiscalPeriod);
        ArgumentNullException.ThrowIfNull(fixedAssetIds);

        userName = string.IsNullOrWhiteSpace(userName)
            ? "system"
            : userName.Trim();

        var selectedIds = fixedAssetIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToHashSet();

        if (selectedIds.Count == 0)
        {
            throw new InvalidOperationException(
                "Chưa chọn bản nháp khấu hao cần ghi sổ.");
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Khóa sổ kỳ kế toán (10.8)
        await AccountingPeriodLock.EnsureOpenAsync(db, fiscalYear, fiscalPeriod, $"ghi sổ khấu hao kỳ {fiscalPeriod:D2}/{fiscalYear}", cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            // Không dùng selectedIds.Contains trực tiếp trong EF query để tránh
            // EF sinh OPENJSON trên SQL Server compatibility level cũ.
            var periodDrafts = await db.FixedAssetDepreciations
                .Where(x =>
                    x.FiscalYear == fiscalYear &&
                    x.FiscalPeriod == fiscalPeriod &&
                    x.Status == FixedAssetDepreciationStatus.Draft)
                .ToListAsync(cancellationToken);

            var records = periodDrafts
                .Where(x => selectedIds.Contains(x.FixedAssetId))
                .OrderBy(x => x.FixedAssetId)
                .ToList();

            if (records.Count != selectedIds.Count)
            {
                throw new InvalidOperationException(
                    "Một hoặc nhiều bản khấu hao không còn ở trạng thái Nháp. " +
                    "Hãy tải lại dữ liệu trước khi ghi sổ.");
            }

            if (records.Any(x => x.DepreciationAmount <= 0))
            {
                throw new InvalidOperationException(
                    "Không thể ghi sổ bản khấu hao có số tiền bằng 0.");
            }

            foreach (var record in records)
            {
                record.ExpenseAccount = NormalizeAccount(
                    record.ExpenseAccount,
                    "tài khoản chi phí khấu hao");

                record.DepreciationAccount = NormalizeAccount(
                    record.DepreciationAccount,
                    "tài khoản hao mòn");
            }

            var assetIdSet = records
                .Select(x => x.FixedAssetId)
                .ToHashSet();

            var allAssets = await db.FixedAssets
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var assets = allAssets
                .Where(x => assetIdSet.Contains(x.Id))
                .ToDictionary(x => x.Id);

            if (assets.Count != assetIdSet.Count)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy đầy đủ tài sản của các bản khấu hao đã chọn.");
            }

            var voucherId = Guid.NewGuid();
            var postingDate = new DateTime(
                fiscalYear,
                fiscalPeriod,
                DateTime.DaysInMonth(fiscalYear, fiscalPeriod));

            var voucherNo = GenerateDepreciationVoucherNo(
                fiscalYear,
                fiscalPeriod);

            var description =
                $"Trích khấu hao TSCĐ kỳ {fiscalPeriod:00}/{fiscalYear}";

            var connection = db.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var dbTransaction = db.Database.CurrentTransaction
                ?.GetDbTransaction()
                ?? throw new InvalidOperationException(
                    "Không lấy được transaction ghi sổ khấu hao.");

            await InsertDepreciationVoucherAsync(
                connection,
                dbTransaction,
                voucherId,
                voucherNo,
                postingDate,
                fiscalYear,
                fiscalPeriod,
                description,
                userName,
                cancellationToken);

            var lineNo = 1;
            var totalAmount = 0m;

            foreach (var record in records)
            {
                var asset = assets[record.FixedAssetId];
                var amount = RoundMoney(record.DepreciationAmount);
                totalAmount += amount;

                var expenseAccountId = await ResolveAccountIdAsync(
                    connection,
                    dbTransaction,
                    record.ExpenseAccount,
                    cancellationToken);

                var depreciationAccountId = await ResolveAccountIdAsync(
                    connection,
                    dbTransaction,
                    record.DepreciationAccount,
                    cancellationToken);

                var lineDescription =
                    $"Khấu hao TSCĐ {asset.AssetCode} - {asset.AssetName} " +
                    $"kỳ {fiscalPeriod:00}/{fiscalYear}";

                await InsertDepreciationVoucherLineAsync(
                    connection,
                    dbTransaction,
                    voucherId,
                    lineNo++,
                    expenseAccountId,
                    record.ExpenseAccount,
                    debit: amount,
                    credit: 0,
                    lineDescription,
                    postingDate,
                    cancellationToken);

                await InsertDepreciationVoucherLineAsync(
                    connection,
                    dbTransaction,
                    voucherId,
                    lineNo++,
                    depreciationAccountId,
                    record.DepreciationAccount,
                    debit: 0,
                    credit: amount,
                    lineDescription,
                    postingDate,
                    cancellationToken);
            }

            await PostDepreciationVoucherToLedgerAsync(
                connection,
                dbTransaction,
                voucherId,
                Guid.Empty,
                userName,
                cancellationToken);

            var ledgerLineCount = await CountLedgerLinesAsync(
                connection,
                dbTransaction,
                voucherId,
                cancellationToken);

            if (ledgerLineCount != records.Count * 2)
            {
                throw new InvalidOperationException(
                    "Số dòng Sổ cái phát sinh không đúng với số dòng khấu hao.");
            }

            foreach (var record in records)
            {
                record.Status = FixedAssetDepreciationStatus.Posted;
                record.VoucherId = voucherId;
                record.VoucherNo = voucherNo;
                record.PostedAt = DateTime.UtcNow;
                record.PostedBy = userName;
                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = userName;
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new FixedAssetDepreciationPostResult
            {
                VoucherId = voucherId,
                VoucherNo = voucherNo,
                PostingDate = postingDate,
                PostedAssetCount = records.Count,
                LedgerLineCount = ledgerLineCount,
                TotalAmount = RoundMoney(totalAmount)
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new DbUpdateConcurrencyException(
                "Bản khấu hao đã được người khác thay đổi. " +
                "Hãy tải lại dữ liệu trước khi ghi sổ.",
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
        _ = companyId; // Giữ tham số để tương thích interface cũ.
        ValidatePeriod(fiscalYear, fiscalPeriod);
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

        // Khóa sổ kỳ kế toán (10.8)
        await AccountingPeriodLock.EnsureOpenAsync(db, fiscalYear, fiscalPeriod, $"xóa nháp khấu hao kỳ {fiscalPeriod:D2}/{fiscalYear}", cancellationToken);

        // Không dùng ids.Contains(...) trực tiếp trong EF query để tránh
        // EF sinh OPENJSON(... WITH ...) trên SQL Server compatibility cũ.
        var periodDraftRecords = await db.FixedAssetDepreciations
            .Where(x =>
                x.FiscalYear == fiscalYear &&
                x.FiscalPeriod == fiscalPeriod &&
                x.Status == FixedAssetDepreciationStatus.Draft)
            .ToListAsync(cancellationToken);

        var idSet = ids.ToHashSet();

        var records = periodDraftRecords
            .Where(x => idSet.Contains(x.FixedAssetId))
            .ToList();

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
                x.Status == FixedAssetStatus.InUse &&
                x.DepreciationMethod != DepreciationMethod.NotDepreciated &&
                x.DepreciationStartDate <= periodEnd &&
                x.OriginalCost > x.ResidualValue)
            .OrderBy(x => x.AssetCode)
            .ToListAsync(cancellationToken);

        if (assets.Count == 0)
            return Array.Empty<FixedAssetDepreciationRow>();

        var assetIds = assets
            .Select(x => x.Id)
            .ToHashSet();

        // Tránh List<Guid>.Contains trong LINQ-to-Entities vì EF Core có thể
        // sinh OPENJSON(@ids) WITH (...), không chạy trên compatibility cũ.
        var recordedRowsForPeriod = await db.FixedAssetDepreciations
            .AsNoTracking()
            .Where(x => x.PeriodStartDate <= periodEnd)
            .ToListAsync(cancellationToken);

        var recordedRows = recordedRowsForPeriod
            .Where(x => assetIds.Contains(x.FixedAssetId))
            .ToList();

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

    private static async Task<Guid> ResolveAccountIdAsync(
        DbConnection connection,
        DbTransaction transaction,
        string accountCode,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
SELECT TOP (1) Id
FROM dbo.DanhMucTaiKhoan
WHERE LTRIM(RTRIM(Taikhoan)) = @AccountCode
ORDER BY Id;";

        AddParameter(command, "@AccountCode", accountCode.Trim());

        var value = await command.ExecuteScalarAsync(cancellationToken);

        if (value is Guid accountId && accountId != Guid.Empty)
            return accountId;

        if (value is not null &&
            value != DBNull.Value &&
            Guid.TryParse(Convert.ToString(value), out accountId) &&
            accountId != Guid.Empty)
        {
            return accountId;
        }

        throw new InvalidOperationException(
            $"Không tìm thấy tài khoản {accountCode} trong DanhMucTaiKhoan.");
    }

    private static async Task InsertDepreciationVoucherAsync(
        DbConnection connection,
        DbTransaction transaction,
        Guid voucherId,
        string voucherNo,
        DateTime postingDate,
        int fiscalYear,
        int fiscalPeriod,
        string description,
        string userName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
INSERT INTO dbo.AccountingVouchers
(
    Id,
    CompanyId,
    VoucherNo,
    VoucherDate,
    PostingDate,
    FiscalYear,
    FiscalPeriod,
    TransactionTypeCode,
    Description,
    CurrencyCode,
    ExchangeRate,
    Status,
    ReferenceNo,
    ReferenceDate,
    BookScope,
    SourceModule,
    SourceId,
    CreatedBy,
    CreatedDate,
    ModifiedBy,
    ModifiedDate,
    Ghiso,
    Approve,
    Both
)
VALUES
(
    @Id,
    @CompanyId,
    @VoucherNo,
    @VoucherDate,
    @PostingDate,
    @FiscalYear,
    @FiscalPeriod,
    N'GENERAL',
    @Description,
    N'VND',
    1,
    1,
    @ReferenceNo,
    @ReferenceDate,
    N'BOTH',
    N'FIXED_ASSET_DEPRECIATION_TT99',
    @SourceId,
    @CreatedBy,
    SYSDATETIME(),
    @CreatedBy,
    SYSDATETIME(),
    0,
    1,
    1
);";

        AddParameter(command, "@Id", voucherId);
        AddParameter(command, "@CompanyId", Guid.Empty);
        AddParameter(command, "@VoucherNo", voucherNo);
        AddParameter(command, "@VoucherDate", postingDate);
        AddParameter(command, "@PostingDate", postingDate);
        AddParameter(command, "@FiscalYear", fiscalYear);
        AddParameter(command, "@FiscalPeriod", fiscalPeriod);
        AddParameter(command, "@Description", description);
        AddParameter(command, "@ReferenceNo", voucherNo);
        AddParameter(command, "@ReferenceDate", postingDate);
        AddParameter(command, "@SourceId", voucherId);
        AddParameter(command, "@CreatedBy", userName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertDepreciationVoucherLineAsync(
        DbConnection connection,
        DbTransaction transaction,
        Guid voucherId,
        int lineNo,
        Guid accountId,
        string accountCode,
        decimal debit,
        decimal credit,
        string description,
        DateTime postingDate,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
INSERT INTO dbo.AccountingVoucherLines
(
    Id,
    VoucherId,
    LineNo_,
    DanhMucTaiKhoanID,
    AccountCode,
    DebitAmount,
    CreditAmount,
    DebitAmountFC,
    CreditAmountFC,
    LineDescription,
    IsTaxBook,
    IsManagementBook,
    LedgerType,
    InvoiceNo,
    InvoiceDate,
    SortKey
)
VALUES
(
    NEWID(),
    @VoucherId,
    @LineNo,
    @DanhMucTaiKhoanID,
    @AccountCode,
    @DebitAmount,
    @CreditAmount,
    @DebitAmount,
    @CreditAmount,
    @LineDescription,
    1,
    1,
    N'BOTH',
    NULL,
    @InvoiceDate,
    @SortKey
);";

        AddParameter(command, "@VoucherId", voucherId);
        AddParameter(command, "@LineNo", lineNo.ToString());
        AddParameter(command, "@DanhMucTaiKhoanID", accountId);
        AddParameter(command, "@AccountCode", accountCode.Trim());
        AddParameter(command, "@DebitAmount", RoundMoney(debit));
        AddParameter(command, "@CreditAmount", RoundMoney(credit));
        AddParameter(command, "@LineDescription", description);
        AddParameter(command, "@InvoiceDate", postingDate);
        AddParameter(command, "@SortKey", lineNo);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task PostDepreciationVoucherToLedgerAsync(
        DbConnection connection,
        DbTransaction transaction,
        Guid voucherId,
        Guid companyId,
        string userName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
DECLARE @TotalDebit DECIMAL(18, 2);
DECLARE @TotalCredit DECIMAL(18, 2);

SELECT
    @TotalDebit = ISNULL(SUM(ISNULL(DebitAmount, 0)), 0),
    @TotalCredit = ISNULL(SUM(ISNULL(CreditAmount, 0)), 0)
FROM dbo.AccountingVoucherLines
WHERE VoucherId = @VoucherId;

IF @TotalDebit = 0 AND @TotalCredit = 0
BEGIN
    RAISERROR(N'Chứng từ khấu hao không có dòng hạch toán.', 16, 1);
    RETURN;
END;

IF ABS(@TotalDebit - @TotalCredit) > 0.01
BEGIN
    RAISERROR(N'Chứng từ khấu hao không cân Nợ/Có.', 16, 1);
    RETURN;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.GeneralLedgerEntries
    WHERE VoucherId = @VoucherId
)
BEGIN
    RAISERROR(N'Chứng từ khấu hao đã được ghi sổ.', 16, 1);
    RETURN;
END;

INSERT INTO dbo.GeneralLedgerEntries
(
    Id,
    VoucherId,
    VoucherLineId,
    CompanyId,
    FiscalYear,
    FiscalPeriod,
    PostingDate,
    VoucherDate,
    VoucherNo,
    AccountCode,
    Debit,
    Credit,
    CurrencyCode,
    ExchangeRate,
    DebitFC,
    CreditFC,
    CustomerId,
    SupplierId,
    EmployeeId,
    ShipmentId,
    ContractId,
    BranchCode,
    Description,
    SourceModule,
    SourceId,
    CreatedAt,
    CreatedBy,
    IsTaxBook,
    IsManagementBook,
    LedgerType
)
SELECT
    NEWID(),
    V.Id,
    L.Id,
    V.CompanyId,
    V.FiscalYear,
    V.FiscalPeriod,
    CAST(ISNULL(V.PostingDate, V.VoucherDate) AS DATE),
    CAST(V.VoucherDate AS DATE),
    V.VoucherNo,
    CAST(L.AccountCode AS NVARCHAR(20)),
    ISNULL(L.DebitAmount, 0),
    ISNULL(L.CreditAmount, 0),
    ISNULL(V.CurrencyCode, N'VND'),
    ISNULL(V.ExchangeRate, 1),
    ISNULL(L.DebitAmountFC, ISNULL(L.DebitAmount, 0)),
    ISNULL(L.CreditAmountFC, ISNULL(L.CreditAmount, 0)),
    L.CustomerId,
    NULL,
    NULL,
    L.ShipmentId,
    TRY_CONVERT(UNIQUEIDENTIFIER, L.ContractId),
    L.BranchId,
    COALESCE(L.LineDescription, V.Description),
    N'ACCOUNTING_VOUCHER',
    CONVERT(NVARCHAR(100), V.Id),
    SYSDATETIME(),
    @CreatedBy,
    ISNULL(L.IsTaxBook, 1),
    ISNULL(L.IsManagementBook, 1),
    ISNULL(L.LedgerType, N'GENERAL')
FROM dbo.AccountingVouchers V
INNER JOIN dbo.AccountingVoucherLines L
    ON L.VoucherId = V.Id
WHERE V.Id = @VoucherId
  AND V.CompanyId = @CompanyId
  AND ISNULL(V.Approve, 0) = 1
  AND ISNULL(V.Status, 1) <> 3
  AND ISNULL(L.AccountCode, N'') <> N''
  AND (ISNULL(L.DebitAmount, 0) <> 0
       OR ISNULL(L.CreditAmount, 0) <> 0);

UPDATE dbo.AccountingVouchers
SET Ghiso = 1,
    Status = 2,
    PostedBy = @CreatedBy,
    PostedDate = SYSDATETIME(),
    ModifiedBy = @CreatedBy,
    ModifiedDate = SYSDATETIME()
WHERE Id = @VoucherId
  AND CompanyId = @CompanyId;";

        AddParameter(command, "@VoucherId", voucherId);
        AddParameter(command, "@CompanyId", companyId);
        AddParameter(command, "@CreatedBy", userName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> CountLedgerLinesAsync(
        DbConnection connection,
        DbTransaction transaction,
        Guid voucherId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
SELECT COUNT(*)
FROM dbo.GeneralLedgerEntries
WHERE VoucherId = @VoucherId;";

        AddParameter(command, "@VoucherId", voucherId);

        var value = await command.ExecuteScalarAsync(cancellationToken);

        return value is null || value == DBNull.Value
            ? 0
            : Convert.ToInt32(value);
    }

    private static void AddParameter(
        DbCommand command,
        string name,
        object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string GenerateDepreciationVoucherNo(
        int fiscalYear,
        int fiscalPeriod)
        => $"KHTS-{fiscalYear}{fiscalPeriod:00}-{DateTime.Now:HHmmssfff}";

    private static void ValidatePeriod(
        int fiscalYear,
        int fiscalPeriod)
    {
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
