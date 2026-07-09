namespace FarmBooks.Core.DTOs.Expenses;

public sealed class ExpenseLineItemDto
{
    public string ExpenseLineItemId { get; set; } = "";
    public string? CodeId { get; set; }
    public string? Code { get; set; }
    public string? CodeName { get; set; }
    public string? Description { get; set; }
    public decimal Total { get; set; }
    public string? VATTreatment { get; set; }
}