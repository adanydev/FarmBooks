using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public interface IExpenseService
{
    Task<string> CreateExpenseAsync(
        DateTime expenseDate,
        DateTime? paidDate,
        ExpenseSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes
    );

    Task UpdateExpenseAsync(
        string expenseId,
        DateTime expenseDate,
        DateTime? paidDate,
        ExpenseSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        VatApplicability vatApplicability,
        VatEntryMethod vatEntryMethod,
        decimal? vatC,
        decimal? vatS,
        bool isVatClassificationConfirmed,
        string? notes
    );

    Task<IReadOnlyList<ExpenseListItemDto>> GetExpenseListAsync();

    Task<ExpenseDetailsDto?> GetExpenseDetailsAsync(string expenseId);
}
