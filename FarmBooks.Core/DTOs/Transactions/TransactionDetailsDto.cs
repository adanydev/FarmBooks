using FarmBooks.Core.Models;

namespace FarmBooks.Core.DTOs.Transactions;

public sealed class TransactionDetailsDto
{
    public string TransactionId { get; set; } = "";

    public DateTime TransactionDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public string SourceType { get; set; } = "";

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

    public string Status { get; set; } = "";
    public bool IsMatched { get; set; }

    public List<TransactionLineItemDto> LineItems { get; set; } = [];
    public List<TransactionDocumentDto> Documents { get; set; } = [];

    public TransactionWorkflowStatusDto WorkflowStatus { get; set; } = new();
}
