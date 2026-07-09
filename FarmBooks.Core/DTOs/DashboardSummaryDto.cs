namespace FarmBooks.Core.DTOs;

public sealed class DashboardSummaryDto
{
    public int NeedsLineItemsCount { get; set; }
    public int NeedsCodesCount { get; set; }
    public int NeedsReviewCount { get; set; }
    public int CompleteCount { get; set; }

    public int UnmatchedExpenseCount { get; set; }
    public int UnmatchedBankTransactionCount { get; set; }

    public int TotalExpenseCount { get; set; }
    public int TotalBankTransactionCount { get; set; }
}