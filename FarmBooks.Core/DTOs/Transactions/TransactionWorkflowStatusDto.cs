namespace FarmBooks.Core.DTOs.Transactions;

public sealed class TransactionWorkflowStatusDto
{
    public bool IsVatReady { get; init; }
    public bool IsTaxReady { get; init; }

    public IReadOnlyList<TransactionWorkflowIssueDto> VatIssues { get; init; } = [];
    public IReadOnlyList<TransactionWorkflowIssueDto> TaxIssues { get; init; } = [];
}
