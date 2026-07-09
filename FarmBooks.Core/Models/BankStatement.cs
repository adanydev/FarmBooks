namespace FarmBooks.Core.Models;

public sealed class BankStatement
{
    public string BankStatementId { get; set; } = "";
    public string BankAccountId { get; set; } = "";
    public DateTime StatementStartDate { get; set; }
    public DateTime StatementEndDate { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string? StatementNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}