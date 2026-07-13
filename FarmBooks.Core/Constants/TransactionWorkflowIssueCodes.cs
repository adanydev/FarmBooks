namespace FarmBooks.Core.Constants;

public static class TransactionWorkflowIssueCodes
{
    public const string MissingPaymentDate = nameof(MissingPaymentDate);
    public const string VatNotReviewed = nameof(VatNotReviewed);
    public const string VatNotSure = nameof(VatNotSure);
    public const string VatAmountMissing = nameof(VatAmountMissing);
    public const string VatClassificationNotConfirmed = nameof(VatClassificationNotConfirmed);

    public const string MissingLineItems = nameof(MissingLineItems);
    public const string MissingAccountingCode = nameof(MissingAccountingCode);
    public const string LineTotalMismatch = nameof(LineTotalMismatch);
}
