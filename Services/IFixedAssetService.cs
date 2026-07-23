using NVOAMASIS.Models.Accounting;

namespace NVOAMASIS.Services.Accounting;

public interface IFixedAssetService
{
    Task<IReadOnlyList<FixedAsset>> SearchAsync(
        Guid companyId,
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<FixedAsset?> GetAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FixedAsset> SaveAsync(
        Guid companyId,
        FixedAsset model,
        string userName,
        CancellationToken cancellationToken = default);

    Task DeleteDraftAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);
}
