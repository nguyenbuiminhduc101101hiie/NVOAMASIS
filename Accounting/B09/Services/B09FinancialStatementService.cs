using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Accounting.B09.Domain;
using NVOAMASIS.Accounting.B09.DTOs;
using NVOAMASIS.Accounting.B09.Seed;
using NVOAMASIS.Data;

namespace NVOAMASIS.Accounting.B09.Services;

public sealed class B09FinancialStatementService : IB09FinancialStatementService
{
    private readonly AppDbContext _db;
    private readonly IB09LedgerDataProvider _ledger;
    private readonly B09TemplateSeeder _seeder;

    public B09FinancialStatementService(AppDbContext db, IB09LedgerDataProvider ledger, B09TemplateSeeder seeder)
    {
        _db = db;
        _ledger = ledger;
        _seeder = seeder;
    }

    public Task EnsureTemplateSeededAsync(CancellationToken cancellationToken = default) =>
        _seeder.SeedAsync(cancellationToken);

    public async Task<B09ReportDto> GenerateAsync(
        Guid companyId,
        int fiscalYear,
        string? userName = null,
        CancellationToken cancellationToken = default)
    {
        if (fiscalYear < 2000 || fiscalYear > 2200)
            throw new ArgumentOutOfRangeException(nameof(fiscalYear));

        await EnsureTemplateSeededAsync(cancellationToken);

        var template = await _db.Set<FinancialStatementNoteTemplate>()
            .Include(x => x.Sections)
                .ThenInclude(x => x.Lines)
                    .ThenInclude(x => x.Mappings)
            .Where(x => x.Code == "B09-DN" && x.IsActive)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstAsync(cancellationToken);

        var report = await _db.Set<FinancialStatementNoteReport>()
            .Include(x => x.Values)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.TemplateId == template.Id && x.FiscalYear == fiscalYear, cancellationToken);

        if (report is null)
        {
            report = new FinancialStatementNoteReport
            {
                CompanyId = companyId,
                TemplateId = template.Id,
                FiscalYear = fiscalYear,
                CreatedBy = userName,
                Status = B09ReportStatus.Draft
            };
            _db.Add(report);
        }
        else if (report.Status == B09ReportStatus.Locked)
        {
            throw new InvalidOperationException("B09-DN đã Locked. Phải unlock theo workflow trước khi Generate lại.");
        }

        var existingValues = report.Values.ToDictionary(x => x.NoteLineId);
        var currentFrom = new DateTime(fiscalYear, 1, 1);
        var currentTo = new DateTime(fiscalYear, 12, 31, 23, 59, 59, 997);
        var previousFrom = currentFrom.AddYears(-1);
        var previousTo = currentTo.AddYears(-1);

        foreach (var line in template.Sections.SelectMany(x => x.Lines).Where(x => x.IsVisible))
        {
            if (!existingValues.TryGetValue(line.Id, out var value))
            {
                value = new FinancialStatementNoteValue
                {
                    Report = report,
                    NoteLineId = line.Id
                };
                report.Values.Add(value);
                existingValues[line.Id] = value;
            }

            if (line.SourceType is B09SourceType.GeneralLedger or B09SourceType.CustomSql or B09SourceType.Hybrid)
            {
                decimal? current = null;
                decimal? previous = null;

                foreach (var mapping in line.Mappings.Where(x => x.IsEnabled))
                {
                    var cv = await _ledger.ExecuteMappingAsync(companyId, currentFrom, currentTo, mapping, cancellationToken);
                    var pv = await _ledger.ExecuteMappingAsync(companyId, previousFrom, previousTo, mapping, cancellationToken);
                    if (cv.HasValue) current = (current ?? 0m) + cv.Value;
                    if (pv.HasValue) previous = (previous ?? 0m) + pv.Value;
                }

                value.CurrentSystemValue = current;
                value.PreviousSystemValue = previous;
                value.UpdatedAt = DateTime.UtcNow;
                value.UpdatedBy = userName;
            }
        }

        report.Status = B09ReportStatus.Generated;
        report.GeneratedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(report.Id, cancellationToken)
            ?? throw new InvalidOperationException("Không thể load B09-DN vừa generate.");
    }

    public Task<B09ReportDto?> GetAsync(Guid reportId, CancellationToken cancellationToken = default) =>
        BuildDtoAsync(reportId, cancellationToken);

    public async Task<B09ReportDto?> GetByCompanyYearAsync(Guid companyId, int fiscalYear, CancellationToken cancellationToken = default)
    {
        var id = await _db.Set<FinancialStatementNoteReport>()
            .Where(x => x.CompanyId == companyId && x.FiscalYear == fiscalYear)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return id.HasValue ? await BuildDtoAsync(id.Value, cancellationToken) : null;
    }

    public async Task UpdateNarrativeAsync(Guid reportId, int noteLineId, string? narrative, string? userName = null, CancellationToken cancellationToken = default)
    {
        var value = await GetEditableValueAsync(reportId, noteLineId, cancellationToken);
        value.Narrative = narrative;
        value.UpdatedBy = userName;
        value.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApplyAdjustmentAsync(
        Guid reportId,
        int noteLineId,
        decimal currentAdjustment,
        decimal previousAdjustment,
        string reason,
        string? userName = null,
        CancellationToken cancellationToken = default)
    {
        if ((currentAdjustment != 0m || previousAdjustment != 0m) && string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Manual adjustment bắt buộc phải có lý do.");

        var value = await GetEditableValueAsync(reportId, noteLineId, cancellationToken);
        value.CurrentAdjustment = currentAdjustment;
        value.PreviousAdjustment = previousAdjustment;
        value.AdjustmentReason = reason;
        value.UpdatedBy = userName;
        value.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<B09ValidationResultDto> ValidateAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var report = await _db.Set<FinancialStatementNoteReport>()
            .Include(x => x.Values)
                .ThenInclude(x => x.NoteLine)
                    .ThenInclude(x => x.Mappings)
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy B09-DN.");

        var result = new B09ValidationResultDto();

        foreach (var value in report.Values)
        {
            var line = value.NoteLine;

            if (line.IsRequiredNarrative && string.IsNullOrWhiteSpace(value.Narrative))
            {
                result.Items.Add(new B09ValidationItemDto
                {
                    Severity = B09ValidationSeverity.Warning,
                    LineCode = line.Code,
                    Message = $"{line.Code} - {line.Name}: chưa nhập thuyết minh/giải trình."
                });
            }

            if (line.SourceType is B09SourceType.GeneralLedger or B09SourceType.CustomSql or B09SourceType.Hybrid)
            {
                if (!line.Mappings.Any(x => x.IsEnabled))
                {
                    result.Items.Add(new B09ValidationItemDto
                    {
                        Severity = B09ValidationSeverity.Error,
                        LineCode = line.Code,
                        Message = $"{line.Code} - {line.Name}: chưa có mapping nguồn dữ liệu."
                    });
                }
            }

            if ((value.CurrentAdjustment != 0m || value.PreviousAdjustment != 0m) && string.IsNullOrWhiteSpace(value.AdjustmentReason))
            {
                result.Items.Add(new B09ValidationItemDto
                {
                    Severity = B09ValidationSeverity.Error,
                    LineCode = line.Code,
                    Message = $"{line.Code}: có manual adjustment nhưng thiếu lý do."
                });
            }
        }

        // Logistics-specific check: unreleased custody assets should have a commodity group.
        var missingCommodityCount = await _db.Set<CustodyAsset>()
            .CountAsync(x => x.CompanyId == report.CompanyId && x.ReleasedDate == null && (x.CommodityGroup == null || x.CommodityGroup == ""), cancellationToken);

        if (missingCommodityCount > 0)
        {
            result.Items.Add(new B09ValidationItemDto
            {
                Severity = B09ValidationSeverity.Warning,
                LineCode = "V.30",
                Message = $"Có {missingCommodityCount} tài sản/hàng nhận giữ hộ đang active nhưng chưa có CommodityGroup."
            });
        }

        if (result.IsValid)
        {
            report.Status = B09ReportStatus.Validated;
            report.ValidatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task LockAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default)
    {
        var report = await _db.Set<FinancialStatementNoteReport>()
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy B09-DN.");

        if (report.Status < B09ReportStatus.Validated)
            throw new InvalidOperationException("B09-DN phải Validate trước khi Lock.");

        report.Status = B09ReportStatus.Locked;
        report.LockedAt = DateTime.UtcNow;
        report.LockedBy = userName;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<FinancialStatementNoteValue> GetEditableValueAsync(Guid reportId, int noteLineId, CancellationToken cancellationToken)
    {
        var report = await _db.Set<FinancialStatementNoteReport>()
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy B09-DN.");

        if (report.Status == B09ReportStatus.Locked)
            throw new InvalidOperationException("B09-DN đã Locked.");

        var value = await _db.Set<FinancialStatementNoteValue>()
            .FirstOrDefaultAsync(x => x.ReportId == reportId && x.NoteLineId == noteLineId, cancellationToken);

        if (value is null)
        {
            value = new FinancialStatementNoteValue { ReportId = reportId, NoteLineId = noteLineId };
            _db.Add(value);
        }
        return value;
    }

    private async Task<B09ReportDto?> BuildDtoAsync(Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _db.Set<FinancialStatementNoteReport>()
            .AsNoTracking()
            .Include(x => x.Template)
            .Include(x => x.Values)
                .ThenInclude(x => x.NoteLine)
                    .ThenInclude(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken);

        if (report is null) return null;

        return new B09ReportDto
        {
            Id = report.Id,
            CompanyId = report.CompanyId,
            FiscalYear = report.FiscalYear,
            TemplateCode = report.Template.Code,
            TemplateVersion = report.Template.Version,
            Status = report.Status,
            CreatedAt = report.CreatedAt,
            CreatedBy = report.CreatedBy,
            GeneratedAt = report.GeneratedAt,
            ValidatedAt = report.ValidatedAt,
            LockedAt = report.LockedAt,
            LockedBy = report.LockedBy,
            Lines = report.Values
                .OrderBy(x => x.NoteLine.Section.DisplayOrder)
                .ThenBy(x => x.NoteLine.DisplayOrder)
                .Select(x => new B09LineDto
                {
                    NoteLineId = x.NoteLineId,
                    SectionCode = x.NoteLine.Section.Code,
                    SectionName = x.NoteLine.Section.Name,
                    Code = x.NoteLine.Code,
                    Name = x.NoteLine.Name,
                    ValueType = x.NoteLine.ValueType,
                    SourceType = x.NoteLine.SourceType,
                    CurrentSystemValue = x.CurrentSystemValue,
                    CurrentAdjustment = x.CurrentAdjustment,
                    PreviousSystemValue = x.PreviousSystemValue,
                    PreviousAdjustment = x.PreviousAdjustment,
                    CurrentReportedValue = x.CurrentReportedValue,
                    PreviousReportedValue = x.PreviousReportedValue,
                    Narrative = x.Narrative,
                    AdjustmentReason = x.AdjustmentReason,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt,
                    IsRequiredNarrative = x.NoteLine.IsRequiredNarrative,
                    DisplayOrder = x.NoteLine.DisplayOrder
                })
                .ToList()
        };
    }

    public async Task<IReadOnlyList<B09ReportListItemDto>> ListReportsAsync(Guid? companyId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<FinancialStatementNoteReport>().AsNoTracking().AsQueryable();
        if (companyId.HasValue && companyId.Value != Guid.Empty)
            query = query.Where(x => x.CompanyId == companyId.Value);

        return await query
            .OrderByDescending(x => x.FiscalYear)
            .ThenByDescending(x => x.GeneratedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new B09ReportListItemDto
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                FiscalYear = x.FiscalYear,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                GeneratedAt = x.GeneratedAt,
                CreatedBy = x.CreatedBy,
                LockedBy = x.LockedBy
            })
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task ReviewAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default)
    {
        var report = await GetWritableReportAsync(reportId, cancellationToken);
        if (report.Status < B09ReportStatus.Validated)
            throw new InvalidOperationException("B09-DN phải Validate trước khi Review.");
        report.Status = B09ReportStatus.Reviewed;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default)
    {
        var report = await GetWritableReportAsync(reportId, cancellationToken);
        if (report.Status < B09ReportStatus.Reviewed)
            throw new InvalidOperationException("B09-DN phải Review trước khi Approve.");
        report.Status = B09ReportStatus.Approved;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default)
    {
        var report = await _db.Set<FinancialStatementNoteReport>()
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy B09-DN.");

        if (report.Status != B09ReportStatus.Locked)
            throw new InvalidOperationException("Chỉ Unlock được báo cáo đang Locked.");

        report.Status = B09ReportStatus.Validated;
        report.LockedAt = null;
        report.LockedBy = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<B09MappingDto>> GetMappingsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureTemplateSeededAsync(cancellationToken);

        return await _db.Set<FinancialStatementNoteMapping>()
            .AsNoTracking()
            .Include(x => x.NoteLine)
                .ThenInclude(x => x.Section)
            .OrderBy(x => x.NoteLine.Section.DisplayOrder)
            .ThenBy(x => x.NoteLine.DisplayOrder)
            .Select(x => new B09MappingDto
            {
                MappingId = x.Id,
                NoteLineId = x.NoteLineId,
                LineCode = x.NoteLine.Code,
                LineName = x.NoteLine.Name,
                SectionCode = x.NoteLine.Section.Code,
                Mode = x.Mode,
                AccountPrefixesCsv = x.AccountPrefixesCsv,
                SignMultiplier = x.SignMultiplier,
                DetailThresholdPercent = x.DetailThresholdPercent,
                IsEnabled = x.IsEnabled,
                HasCustomSql = x.CustomSql != null && x.CustomSql != ""
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SaveMappingAsync(B09MappingDto mapping, string? userName = null, CancellationToken cancellationToken = default)
    {
        if (mapping.Mode == B09MappingMode.CustomSql)
            throw new InvalidOperationException("Không được sửa CustomSql từ màn hình người dùng.");

        var entity = await _db.Set<FinancialStatementNoteMapping>()
            .FirstOrDefaultAsync(x => x.Id == mapping.MappingId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy mapping.");

        entity.Mode = mapping.Mode;
        entity.AccountPrefixesCsv = mapping.AccountPrefixesCsv;
        entity.SignMultiplier = mapping.SignMultiplier;
        entity.DetailThresholdPercent = mapping.DetailThresholdPercent;
        entity.IsEnabled = mapping.IsEnabled;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RelatedPartyProfile>> ListRelatedPartiesAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _db.Set<RelatedPartyProfile>()
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

    public async Task SaveRelatedPartyAsync(RelatedPartyProfile party, CancellationToken cancellationToken = default)
    {
        if (party.CompanyId == Guid.Empty)
            throw new InvalidOperationException("Chưa chọn công ty.");
        if (string.IsNullOrWhiteSpace(party.RelationshipDescription))
            throw new InvalidOperationException("Nhập tên / mô tả bên liên quan.");

        if (party.Id == 0)
            _db.Add(party);
        else
            _db.Update(party);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRelatedPartyAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<RelatedPartyProfile>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy bên liên quan.");
        _db.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustodyAsset>> ListCustodyAssetsAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _db.Set<CustodyAsset>()
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.ReceivedDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task SaveCustodyAssetAsync(CustodyAsset asset, CancellationToken cancellationToken = default)
    {
        if (asset.CompanyId == Guid.Empty)
            throw new InvalidOperationException("Chưa chọn công ty.");

        if (asset.Id == 0)
            _db.Add(asset);
        else
            _db.Update(asset);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCustodyAssetAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<CustodyAsset>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy hàng giữ hộ.");
        _db.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<B09ConcentrationDto>> GetConcentrationsAsync(
        Guid companyId,
        int fiscalYear,
        string accountPrefix,
        decimal thresholdPercent,
        CancellationToken cancellationToken = default)
    {
        var toDate = new DateTime(fiscalYear, 12, 31, 23, 59, 59, 997);
        var prefix = accountPrefix.Trim();
        var isPayable = prefix.StartsWith("33", StringComparison.Ordinal);

        var rows = await _db.GeneralLedgerEntries.AsNoTracking()
            .Where(x => x.CompanyId == companyId
                        && x.PostingDate <= toDate
                        && x.AccountCode.StartsWith(prefix))
            .Select(x => new
            {
                PartyId = isPayable ? (x.SupplierId ?? x.CustomerId) : (x.CustomerId ?? x.SupplierId),
                Amount = x.Debit - x.Credit
            })
            .ToListAsync(cancellationToken);

        var grouped = rows
            .GroupBy(x => x.PartyId)
            .Select(g => new
            {
                PartyId = g.Key,
                Amount = isPayable ? -g.Sum(x => x.Amount) : g.Sum(x => x.Amount)
            })
            .Where(x => x.Amount != 0m)
            .ToList();

        var total = grouped.Sum(x => Math.Abs(x.Amount));
        if (total == 0m)
            return Array.Empty<B09ConcentrationDto>();

        var customerIds = grouped.Where(x => x.PartyId.HasValue).Select(x => x.PartyId!.Value).Distinct().ToList();
        var customers = await _db.Customer.AsNoTracking()
            .Where(x => customerIds.Contains(x.Customer_ID))
            .Select(x => new { x.Customer_ID, x.TaxCode, x.COMPANY, x.BIZName })
            .ToListAsync(cancellationToken);
        var labels = customers.ToDictionary(
            x => x.Customer_ID,
            x => string.Join("_", new[] { x.TaxCode, x.COMPANY ?? x.BIZName }.Where(v => !string.IsNullOrWhiteSpace(v))));

        return grouped
            .Select(x =>
            {
                var pct = Math.Abs(x.Amount) / total * 100m;
                return new B09ConcentrationDto
                {
                    Kind = isPayable ? "AP" : "AR",
                    PartyId = x.PartyId,
                    PartyLabel = x.PartyId.HasValue && labels.TryGetValue(x.PartyId.Value, out var label) && !string.IsNullOrWhiteSpace(label)
                        ? label
                        : (x.PartyId.HasValue ? x.PartyId.Value.ToString() : "(không gắn đối tượng)"),
                    Amount = x.Amount,
                    PercentOfTotal = Math.Round(pct, 2),
                    MeetsThreshold = pct >= thresholdPercent
                };
            })
            .OrderByDescending(x => x.MeetsThreshold)
            .ThenByDescending(x => Math.Abs(x.Amount))
            .ToList();
    }

    public async Task<IReadOnlyList<B09FixedAssetDto>> GetFixedAssetsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.FixedAssets.AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.AssetCode)
            .Select(x => new B09FixedAssetDto
            {
                AssetCode = x.AssetCode,
                AssetName = x.AssetName,
                AssetType = x.AssetType.ToString(),
                OriginalCost = x.OriginalCost,
                OpeningAccumulatedDepreciation = x.OpeningAccumulatedDepreciation,
                RemainingValue = x.RemainingValue,
                Status = x.Status.ToString(),
                RecognitionDate = x.RecognitionDate,
                PurchaseDate = x.PurchaseDate
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<FinancialStatementNoteReport> GetWritableReportAsync(Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _db.Set<FinancialStatementNoteReport>()
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy B09-DN.");

        if (report.Status == B09ReportStatus.Locked)
            throw new InvalidOperationException("B09-DN đã Locked. Unlock trước khi sửa.");

        return report;
    }
}
