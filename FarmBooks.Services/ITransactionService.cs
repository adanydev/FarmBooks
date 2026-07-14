using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public interface ITransactionService
{
    Task<string> CreateTransactionAsync(
        DateTime? receiptDate,
        DateTime? paymentDate,
        TransactionSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        int statementOrder,
        string? notes
    );

    Task UpdateTransactionAsync(
        string transactionId,
        DateTime? receiptDate,
        DateTime? paymentDate,
        TransactionSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        VatApplicability vatApplicability,
        VatEntryMethod vatEntryMethod,
        decimal? vatC,
        decimal? vatS,
        bool isVatClassificationConfirmed,
        int statementOrder,
        string? notes
    );

    Task<IReadOnlyList<TransactionListItemDto>> GetTransactionListAsync();

    Task<TransactionDetailsDto?> GetTransactionDetailsAsync(string transactionId);

    Task DeleteTransactionAsync(string transactionId);
}
