namespace FarmBooks.Core.Models;

public sealed class BankTransaction
{
    public string BankTransactionId { get; set; } = "";
    public string BankAccountId { get; set; } = "";
    public string? BankStatementId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public decimal? BalanceAfterTransaction { get; set; }
    public string? Reference { get; set; }
    public string? ExpenseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}