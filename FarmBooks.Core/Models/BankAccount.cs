namespace FarmBooks.Core.Models;

public sealed class BankAccount
{
    public string BankAccountId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? BankName { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningBalanceDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}