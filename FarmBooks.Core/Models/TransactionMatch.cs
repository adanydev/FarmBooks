namespace FarmBooks.Core.Models;

public sealed class TransactionMatch
{
    public string TransactionMatchId { get; set; } = "";

    public string TransactionId { get; set; } = "";

    public string BankTransactionId { get; set; } = "";

    public DateTime MatchedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}