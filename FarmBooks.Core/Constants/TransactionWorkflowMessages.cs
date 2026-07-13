namespace FarmBooks.Core.Constants;

public static class TransactionWorkflowMessages
{
    public const string PaymentDateMissing = "Payment date missing.";

    public const string ChooseVatApplicability = "Choose whether VAT applies.";

    public const string ChooseVatEntryMethod = "Choose how VAT was determined.";

    public const string VatAmountsMissing = "VATC/VATS missing.";

    public const string ConfirmVatClassification = "Confirm VATC/VATS.";

    public const string MissingLineItems = "Add at least one line item.";

    public const string MissingAccountingCode = "Accounting code missing.";

    public const string LineItemTotalMismatch =
        "Line-item total does not match the transaction total.";
}
