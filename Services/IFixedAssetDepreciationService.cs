using NVOAMASIS.Models.Accounting;

namespace NVOAMASIS.Services.Accounting;

public interface IFixedAssetDepreciationService
{
    Task<IReadOnlyList<FixedAssetDepreciationRow>> CalculateAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        bool prorateByDay,
        CancellationToken cancellationToken = default);

    Task<int> SaveDraftAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        bool prorateByDay,
        IReadOnlyCollection<FixedAssetDepreciationRow> rows,
        string userName,
        CancellationToken cancellationToken = default);

    Task<int> DeleteDraftAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        IReadOnlyCollection<Guid> fixedAssetIds,
        CancellationToken cancellationToken = default);

    Task<FixedAssetDepreciationPostResult> PostDraftAsync(
        Guid companyId,
        int fiscalYear,
        int fiscalPeriod,
        IReadOnlyCollection<Guid> fixedAssetIds,
        string userName,
        CancellationToken cancellationToken = default);
}
