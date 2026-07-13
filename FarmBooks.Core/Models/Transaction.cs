namespace FarmBooks.Core.Models;

public sealed class Transaction
{
    public string TransactionId { get; set; } = "";

    public DateTime ReceiptDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    public TransactionSourceType SourceType { get; set; }

    public string? DocumentNumber { get; set; }
    public string? BusinessName { get; set; }
    public string? Description { get; set; }

    public decimal Total { get; set; }

    public VatApplicability VatApplicability { get; set; } = VatApplicability.NotSure;

    public VatEntryMethod VatEntryMethod { get; set; } = VatEntryMethod.None;

    public decimal? VATC { get; set; }
    public decimal? VATS { get; set; }

    public bool IsVatClassificationConfirmed { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
