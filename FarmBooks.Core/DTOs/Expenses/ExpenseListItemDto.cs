namespace FarmBooks.Core.DTOs.Expenses;

public sealed class ExpenseListItemDto
{
    public string ExpenseId { get; set; } = "";
    public DateTime ExpenseDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string SourceType { get; set; } = "";
    public string? DocumentNumber { get; set; }
    public string? BusinessName { get; set; }
    public string? Description { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "";
    public bool IsMatched { get; set; }
    public int LineItemCount { get; set; }
    public int DocumentCount { get; set; }
}
