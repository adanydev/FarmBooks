namespace FarmBooks.Core.DTOs.Expenses;

public sealed class ExpenseWorkflowStatusDto
{
    public bool IsVatReady { get; init; }
    public bool IsTaxReady { get; init; }

    public IReadOnlyList<ExpenseWorkflowIssueDto> VatIssues { get; init; } = [];
    public IReadOnlyList<ExpenseWorkflowIssueDto> TaxIssues { get; init; } = [];
}
