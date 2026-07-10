namespace FarmBooks.Core.Models;

public sealed class ExpenseLineItem
{
    public string ExpenseLineItemId { get; set; } = "";

    public string ExpenseId { get; set; } = "";

    public string? CodeId { get; set; }

    public string? Description { get; set; }

    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
