namespace FarmBooks.Core.DTOs.Transactions;

public sealed class TransactionListItemDto
{
    public string TransactionId { get; set; } = "";
    public DateTime? ReceiptDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string SourceType { get; set; } = "";
    public string? DocumentNumber { get; set; }
    public string? BusinessName { get; set; }
    public string? Description { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "";
    public bool IsMatched { get; set; }
    public int LineItemCount { get; set; }
    public int DocumentCount { get; set; }
    public bool IsVatReady { get; set; }
    public bool IsTaxReady { get; set; }
    public int VatIssueCount { get; set; }
    public int TaxIssueCount { get; set; }
    public int StatementOrder { get; set; }
    public IReadOnlyList<TransactionWorkflowIssueDto> VatIssues { get; set; } = [];
    public IReadOnlyList<TransactionWorkflowIssueDto> TaxIssues { get; set; } = [];
}
