using NVOAMASIS.Accounting.B09.Domain;
using NVOAMASIS.Accounting.B09.DTOs;

namespace NVOAMASIS.Accounting.B09.Services;

public interface IB09FinancialStatementService
{
    Task EnsureTemplateSeededAsync(CancellationToken cancellationToken = default);
    Task<B09ReportDto> GenerateAsync(Guid companyId, int fiscalYear, string? userName = null, CancellationToken cancellationToken = default);
    Task<B09ReportDto?> GetAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task<B09ReportDto?> GetByCompanyYearAsync(Guid companyId, int fiscalYear, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<B09ReportListItemDto>> ListReportsAsync(Guid? companyId = null, CancellationToken cancellationToken = default);
    Task UpdateNarrativeAsync(Guid reportId, int noteLineId, string? narrative, string? userName = null, CancellationToken cancellationToken = default);
    Task ApplyAdjustmentAsync(Guid reportId, int noteLineId, decimal currentAdjustment, decimal previousAdjustment, string reason, string? userName = null, CancellationToken cancellationToken = default);
    Task<B09ValidationResultDto> ValidateAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task ReviewAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default);
    Task ApproveAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default);
    Task LockAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default);
    Task UnlockAsync(Guid reportId, string? userName = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<B09MappingDto>> GetMappingsAsync(CancellationToken cancellationToken = default);
    Task SaveMappingAsync(B09MappingDto mapping, string? userName = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RelatedPartyProfile>> ListRelatedPartiesAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task SaveRelatedPartyAsync(RelatedPartyProfile party, CancellationToken cancellationToken = default);
    Task DeleteRelatedPartyAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustodyAsset>> ListCustodyAssetsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task SaveCustodyAssetAsync(CustodyAsset asset, CancellationToken cancellationToken = default);
    Task DeleteCustodyAssetAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<B09ConcentrationDto>> GetConcentrationsAsync(Guid companyId, int fiscalYear, string accountPrefix, decimal thresholdPercent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<B09FixedAssetDto>> GetFixedAssetsAsync(Guid companyId, CancellationToken cancellationToken = default);
}
