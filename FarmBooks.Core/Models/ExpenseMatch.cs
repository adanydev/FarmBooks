namespace FarmBooks.Core.Models;

public sealed class ExpenseMatch
{
    public string ExpenseMatchId { get; set; } = "";

    public string ExpenseId { get; set; } = "";

    public string BankTransactionId { get; set; } = "";

    public DateTime MatchedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}