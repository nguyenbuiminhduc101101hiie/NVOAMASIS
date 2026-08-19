using NVOAMASIS.Accounting.B09.Domain;

namespace NVOAMASIS.Accounting.B09.Services;

public interface IB09LedgerDataProvider
{
    Task<decimal?> ExecuteMappingAsync(
        Guid companyId,
        DateTime fromDate,
        DateTime toDate,
        FinancialStatementNoteMapping mapping,
        CancellationToken cancellationToken = default);
}
