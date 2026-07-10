namespace FarmBooks.Core.DTOs.Expenses;

public sealed class ExpenseDetailsDto
{
    public string ExpenseId { get; set; } = "";
    public DateTime ExpenseDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string SourceType { get; set; } = "";
    public string? DocumentNumber { get; set; }
    public string? BusinessName { get; set; }
    public string? Description { get; set; }
    public decimal Total { get; set; }
    public decimal? VATC { get; set; }
    public decimal? VATS { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "";
    public bool IsMatched { get; set; }

    public List<ExpenseLineItemDto> LineItems { get; set; } = [];
    public List<ExpenseDocumentDto> Documents { get; set; } = [];

    public ExpenseWorkflowStatusDto WorkflowStatus { get; set; } = new();
}
