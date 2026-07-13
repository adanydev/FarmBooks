using FarmBooks.Core.DTOs;
using FarmBooks.Data.Repositories;
using FarmBooks.Services;

namespace FarmBooks.Services;

public sealed class DashboardService
{
    private readonly DashboardRepository _dashboardRepository;

    public DashboardService(DashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var transactions = await _dashboardRepository.ListActiveTransactionsAsync();
        var lineItems = await _dashboardRepository.ListActiveLineItemsAsync();

        var summary = new DashboardSummaryDto
        {
            TotalTransactionCount = transactions.Count,
            TotalBankTransactionCount =
                await _dashboardRepository.GetTotalBankTransactionCountAsync(),

            UnmatchedTransactionCount =
                await _dashboardRepository.GetUnmatchedTransactionCountAsync(),

            UnmatchedBankTransactionCount =
                await _dashboardRepository.GetUnmatchedBankTransactionCountAsync(),
        };

        foreach (var transaction in transactions)
        {
            var transactionLineItems = lineItems
                .Where(x => x.TransactionId == transaction.TransactionId)
                .ToList();

            var status = TransactionStatusCalculator.Calculate(transaction, transactionLineItems);

            switch (status)
            {
                case TransactionStatus.NeedsLineItems:
                    summary.NeedsLineItemsCount++;
                    break;

                case TransactionStatus.NeedsCodes:
                    summary.NeedsCodesCount++;
                    break;

                case TransactionStatus.NeedsReview:
                    summary.NeedsReviewCount++;
                    break;

                case TransactionStatus.Complete:
                    summary.CompleteCount++;
                    break;
            }
        }

        return summary;
    }
}
