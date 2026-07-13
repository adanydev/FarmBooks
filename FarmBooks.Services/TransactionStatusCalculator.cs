using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public enum TransactionStatus
{
    NeedsLineItems,
    NeedsCodes,
    NeedsReview,
    Complete,
}

public static class TransactionStatusCalculator
{
    private const decimal Tolerance = 0.01m;

    public static TransactionStatus Calculate(
        Transaction transaction,
        IReadOnlyList<TransactionLineItem> lineItems
    )
    {
        if (lineItems.Count == 0)
            return TransactionStatus.NeedsLineItems;

        if (lineItems.Any(x => x.CodeId == null))
            return TransactionStatus.NeedsCodes;

        var lineTotal = lineItems.Sum(x => x.Total);
        var difference = Math.Abs(transaction.Total - lineTotal);

        if (difference > Tolerance)
            return TransactionStatus.NeedsReview;

        return TransactionStatus.Complete;
    }
}
