using FarmBooks.Core.Models;

namespace FarmBooks.Data.Services;

public enum ExpenseStatus
{
    NeedsLineItems,
    NeedsCodes,
    NeedsReview,
    Complete
}

public static class ExpenseStatusCalculator
{
    private const decimal Tolerance = 0.01m;

    public static ExpenseStatus Calculate(
        Expense expense,
        IReadOnlyList<ExpenseLineItem> lineItems)
    {
        if (lineItems.Count == 0)
            return ExpenseStatus.NeedsLineItems;

        if (lineItems.Any(x => x.CodeId == null))
            return ExpenseStatus.NeedsCodes;

        var lineTotal = lineItems.Sum(x => x.Total);
        var difference = Math.Abs(expense.Total - lineTotal);

        if (difference > Tolerance)
            return ExpenseStatus.NeedsReview;

        return ExpenseStatus.Complete;
    }
}