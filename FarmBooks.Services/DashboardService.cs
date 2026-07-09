using FarmBooks.Core.DTOs;
using FarmBooks.Services;
using FarmBooks.Data.Repositories;

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
        var expenses = await _dashboardRepository.ListActiveExpensesAsync();
        var lineItems = await _dashboardRepository.ListActiveLineItemsAsync();

        var summary = new DashboardSummaryDto
        {
            TotalExpenseCount = expenses.Count,
            TotalBankTransactionCount =
                await _dashboardRepository.GetTotalBankTransactionCountAsync(),

            UnmatchedExpenseCount =
                await _dashboardRepository.GetUnmatchedExpenseCountAsync(),

            UnmatchedBankTransactionCount =
                await _dashboardRepository.GetUnmatchedBankTransactionCountAsync()
        };

        foreach (var expense in expenses)
        {
            var expenseLineItems = lineItems
                .Where(x => x.ExpenseId == expense.ExpenseId)
                .ToList();

            var status = ExpenseStatusCalculator.Calculate(expense, expenseLineItems);

            switch (status)
            {
                case ExpenseStatus.NeedsLineItems:
                    summary.NeedsLineItemsCount++;
                    break;

                case ExpenseStatus.NeedsCodes:
                    summary.NeedsCodesCount++;
                    break;

                case ExpenseStatus.NeedsReview:
                    summary.NeedsReviewCount++;
                    break;

                case ExpenseStatus.Complete:
                    summary.CompleteCount++;
                    break;
            }
        }

        return summary;
    }
}