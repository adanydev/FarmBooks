namespace FarmBooks.Core.DTOs.Transactions;

public sealed class TransactionLineItemDto
{
    public string TransactionLineItemId { get; set; } = "";
    public string? CodeId { get; set; }
    public string? Code { get; set; }
    public string? CodeName { get; set; }
    public string? Description { get; set; }
    public decimal Total { get; set; }
    public int StatementOrder { get; set; }
}
