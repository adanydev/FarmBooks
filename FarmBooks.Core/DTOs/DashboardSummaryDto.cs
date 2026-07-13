namespace FarmBooks.Core.DTOs;

public sealed class DashboardSummaryDto
{
    public int NeedsLineItemsCount { get; set; }
    public int NeedsCodesCount { get; set; }
    public int NeedsReviewCount { get; set; }
    public int CompleteCount { get; set; }

    public int UnmatchedTransactionCount { get; set; }
    public int UnmatchedBankTransactionCount { get; set; }

    public int TotalTransactionCount { get; set; }
    public int TotalBankTransactionCount { get; set; }
}