namespace FarmBooks.Core.Models;

public sealed class TransactionLineItem
{
    public string TransactionLineItemId { get; set; } = "";

    public string TransactionId { get; set; } = "";

    public string? CodeId { get; set; }

    public string? Description { get; set; }

    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
