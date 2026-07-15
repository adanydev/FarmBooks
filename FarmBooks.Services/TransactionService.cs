using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly TransactionRepository _transactions;
    private readonly TransactionLineItemRepository _lineItems;
    private readonly TransactionMatchRepository _matches;
    private readonly TransactionDocumentRepository _documents;
    private readonly AccountingCodeRepository _codes;
    private readonly AuditService _auditService;

    public TransactionService(
        TransactionRepository transactions,
        TransactionLineItemRepository lineItems,
        TransactionMatchRepository matches,
        TransactionDocumentRepository documents,
        AccountingCodeRepository codes,
        AuditService auditService
    )
    {
        _transactions = transactions;
        _lineItems = lineItems;
        _matches = matches;
        _documents = documents;
        _codes = codes;
        _auditService = auditService;
    }

    public async Task<string> CreateTransactionAsync(
        DateTime? receiptDate,
        DateTime? paymentDate,
        TransactionSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes,
        string? insertAfterTransactionId = null
    )
    {
        int? insertAtStatementOrder = null;

        if (!string.IsNullOrWhiteSpace(insertAfterTransactionId))
        {
            var selectedTransaction = await _transactions.GetAsync(insertAfterTransactionId);

            if (selectedTransaction is null)
            {
                throw new InvalidOperationException("The selected transaction could not be found.");
            }

            if (!AreSamePaymentDate(selectedTransaction.PaymentDate, paymentDate))
            {
                throw new InvalidOperationException(
                    "The new transaction can only be inserted after a transaction with the same payment date."
                );
            }

            insertAtStatementOrder = selectedTransaction.StatementOrder + 1;
        }

        var now = DateTime.UtcNow;

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            ReceiptDate = receiptDate,
            PaymentDate = paymentDate,
            SourceType = sourceType,
            DocumentNumber = NullIfWhiteSpace(documentNumber),
            BusinessName = NullIfWhiteSpace(businessName),
            Description = NullIfWhiteSpace(description),
            Total = total,
            StatementOrder = 0,
            Notes = NullIfWhiteSpace(notes),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _transactions.CreateAsync(transaction, insertAtStatementOrder);

        await _auditService.WriteAsync(
            "Transaction",
            transaction.TransactionId,
            "Created",
            null,
            transaction
        );

        return transaction.TransactionId;
    }

    public async Task<TransactionStatus> GetStatusAsync(string transactionId)
    {
        var transaction = await _transactions.GetAsync(transactionId);

        if (transaction is null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }

        var lineItems = await _lineItems.ListForTransactionAsync(transactionId);

        return TransactionStatusCalculator.Calculate(transaction, lineItems);
    }

    public Task<IReadOnlyList<Transaction>> ListTransactionsAsync()
    {
        return _transactions.ListAsync();
    }

    public async Task UpdateTransactionAsync(
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
        string? notes
    )
    {
        var transaction = await _transactions.GetAsync(transactionId);

        if (transaction is null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }

        ValidateVatSigns(total, vatC, vatS);

        var oldTransaction = CopyTransaction(transaction);

        var originalPaymentDate = transaction.PaymentDate;

        var originalStatementOrder = transaction.StatementOrder;

        transaction.ReceiptDate = receiptDate;
        transaction.PaymentDate = paymentDate;
        transaction.SourceType = sourceType;
        transaction.DocumentNumber = NullIfWhiteSpace(documentNumber);
        transaction.BusinessName = NullIfWhiteSpace(businessName);
        transaction.Description = NullIfWhiteSpace(description);
        transaction.Total = total;
        transaction.VatApplicability = vatApplicability;

        if (vatApplicability == VatApplicability.Yes)
        {
            transaction.VatEntryMethod = vatEntryMethod;

            transaction.VATC = NormalizeVatAmount(vatC);

            transaction.VATS = NormalizeVatAmount(vatS);

            transaction.IsVatClassificationConfirmed = isVatClassificationConfirmed;
        }
        else
        {
            transaction.VatEntryMethod = VatEntryMethod.None;

            transaction.VATC = null;
            transaction.VATS = null;

            transaction.IsVatClassificationConfirmed = false;
        }

        transaction.Notes = NullIfWhiteSpace(notes);

        await _transactions.UpdateAsync(transaction, originalPaymentDate, originalStatementOrder);

        await _auditService.WriteAsync(
            "Transaction",
            transaction.TransactionId,
            "Updated",
            oldTransaction,
            transaction
        );
    }

    public async Task DeleteTransactionAsync(string transactionId)
    {
        var transaction = await _transactions.GetAsync(transactionId);

        if (transaction is null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }

        await _transactions.DeleteAsync(transaction);

        await _auditService.WriteAsync("Transaction", transactionId, "Deleted", transaction, null);
    }

    public async Task RestoreTransactionAsync(string transactionId)
    {
        await _transactions.RestoreAsync(transactionId);

        await _auditService.WriteAsync(
            "Transaction",
            transactionId,
            "Restored",
            null,
            new { TransactionId = transactionId }
        );
    }

    public async Task<IReadOnlyList<TransactionListItemDto>> GetTransactionListAsync()
    {
        var transactions = await _transactions.ListAsync();

        var result = new List<TransactionListItemDto>();

        foreach (var transaction in transactions)
        {
            var lineItems = await _lineItems.ListForTransactionAsync(transaction.TransactionId);

            var workflow = TransactionWorkflowStatusCalculator.Calculate(transaction, lineItems);

            var status = TransactionStatusCalculator.Calculate(transaction, lineItems);

            var isMatched = await _matches.IsTransactionMatchedAsync(transaction.TransactionId);

            var documentCount = await _documents.CountForTransactionAsync(
                transaction.TransactionId
            );

            result.Add(
                new TransactionListItemDto
                {
                    TransactionId = transaction.TransactionId,
                    ReceiptDate = transaction.ReceiptDate,
                    PaymentDate = transaction.PaymentDate,
                    SourceType = transaction.SourceType.ToString(),
                    DocumentNumber = transaction.DocumentNumber,
                    BusinessName = transaction.BusinessName,
                    Description = transaction.Description,
                    Total = transaction.Total,
                    Status = status.ToString(),
                    IsVatReady = workflow.IsVatReady,
                    IsTaxReady = workflow.IsTaxReady,
                    VatIssueCount = workflow.VatIssues.Count,
                    TaxIssueCount = workflow.TaxIssues.Count,
                    VatIssues = workflow.VatIssues,
                    TaxIssues = workflow.TaxIssues,
                    IsMatched = isMatched,
                    LineItemCount = lineItems.Count,
                    DocumentCount = documentCount,
                    StatementOrder = transaction.StatementOrder,
                }
            );
        }

        return result;
    }

    public async Task<TransactionDetailsDto?> GetTransactionDetailsAsync(string transactionId)
    {
        var transaction = await _transactions.GetAsync(transactionId);

        if (transaction is null)
            return null;

        var lineItems = await _lineItems.ListForTransactionAsync(transactionId);

        var workflow = TransactionWorkflowStatusCalculator.Calculate(transaction, lineItems);

        var documents = await _documents.ListForTransactionAsync(transactionId);

        var codes = await _codes.ListActiveAsync();

        var status = TransactionStatusCalculator.Calculate(transaction, lineItems);

        var isMatched = await _matches.IsTransactionMatchedAsync(transactionId);

        return new TransactionDetailsDto
        {
            TransactionId = transaction.TransactionId,
            ReceiptDate = transaction.ReceiptDate,
            PaymentDate = transaction.PaymentDate,
            SourceType = transaction.SourceType.ToString(),
            DocumentNumber = transaction.DocumentNumber,
            BusinessName = transaction.BusinessName,
            Description = transaction.Description,
            Total = transaction.Total,
            Notes = transaction.Notes,
            VatApplicability = transaction.VatApplicability,
            VatEntryMethod = transaction.VatEntryMethod,
            VATC = transaction.VATC,
            VATS = transaction.VATS,
            IsVatClassificationConfirmed = transaction.IsVatClassificationConfirmed,
            Status = status.ToString(),
            WorkflowStatus = workflow,
            IsMatched = isMatched,
            StatementOrder = transaction.StatementOrder,

            LineItems = lineItems
                .Select(item =>
                {
                    var code = codes.FirstOrDefault(candidate => candidate.CodeId == item.CodeId);

                    return new TransactionLineItemDto
                    {
                        TransactionLineItemId = item.TransactionLineItemId,
                        CodeId = item.CodeId,
                        Code = code?.Code,
                        CodeName = code?.Name,
                        Description = item.Description,
                        Total = item.Total,
                    };
                })
                .ToList(),

            Documents = documents
                .Select(document => new TransactionDocumentDto
                {
                    TransactionDocumentId = document.TransactionDocumentId,
                    FileName = document.FileName,
                    MimeType = document.MimeType,
                    DocumentType = document.DocumentType,
                    UploadedAt = document.UploadedAt,
                })
                .ToList(),
        };
    }

    public async Task MoveTransactionAsync(string transactionId, TransactionMoveDirection direction)
    {
        var current = await _transactions.GetAsync(transactionId);

        if (current is null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }

        await _transactions.NormalizeStatementOrdersAsync(current.PaymentDate);

        current = await _transactions.GetAsync(transactionId);

        if (current is null)
        {
            throw new InvalidOperationException("Transaction could not be reloaded.");
        }

        var transactions = (await _transactions.ListForPaymentDateAsync(current.PaymentDate))
            .OrderBy(transaction => transaction.StatementOrder)
            .ThenBy(transaction => transaction.CreatedAt)
            .ToList();

        var currentIndex = transactions.FindIndex(transaction =>
            transaction.TransactionId == transactionId
        );

        if (currentIndex < 0)
            return;

        var targetIndex =
            direction == TransactionMoveDirection.Up ? currentIndex - 1 : currentIndex + 1;

        if (targetIndex < 0 || targetIndex >= transactions.Count)
        {
            return;
        }

        var target = transactions[targetIndex];

        var originalCurrentOrder = current.StatementOrder;

        var originalTargetOrder = target.StatementOrder;

        await _transactions.UpdateStatementOrdersAsync(
            current.TransactionId,
            target.StatementOrder,
            target.TransactionId,
            current.StatementOrder
        );

        await _auditService.WriteAsync(
            "Transaction",
            transactionId,
            direction == TransactionMoveDirection.Up ? "Moved Up" : "Moved Down",
            new
            {
                StatementOrder = originalCurrentOrder,

                TargetTransactionId = target.TransactionId,

                TargetStatementOrder = originalTargetOrder,
            },
            new
            {
                StatementOrder = originalTargetOrder,

                TargetTransactionId = target.TransactionId,

                TargetStatementOrder = originalCurrentOrder,
            }
        );
    }

    private static Transaction CopyTransaction(Transaction transaction)
    {
        return new Transaction
        {
            TransactionId = transaction.TransactionId,
            ReceiptDate = transaction.ReceiptDate,
            PaymentDate = transaction.PaymentDate,
            SourceType = transaction.SourceType,
            DocumentNumber = transaction.DocumentNumber,
            BusinessName = transaction.BusinessName,
            Description = transaction.Description,
            Total = transaction.Total,
            VatApplicability = transaction.VatApplicability,
            VatEntryMethod = transaction.VatEntryMethod,
            VATC = transaction.VATC,
            VATS = transaction.VATS,
            IsVatClassificationConfirmed = transaction.IsVatClassificationConfirmed,
            StatementOrder = transaction.StatementOrder,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            DeletedAt = transaction.DeletedAt,
        };
    }

    private static bool AreSamePaymentDate(DateTime? first, DateTime? second)
    {
        if (first is null || second is null)
            return first is null && second is null;

        return first.Value.Date == second.Value.Date;
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateVatSigns(decimal transactionTotal, decimal? vatC, decimal? vatS)
    {
        if (transactionTotal > 0m && ((vatC ?? 0m) < 0m || (vatS ?? 0m) < 0m))
        {
            throw new InvalidOperationException("VAT amounts cannot be negative for an expense.");
        }

        if (transactionTotal < 0m && ((vatC ?? 0m) > 0m || (vatS ?? 0m) > 0m))
        {
            throw new InvalidOperationException(
                "VAT amounts must be negative for an income transaction."
            );
        }
    }

    private static decimal? NormalizeVatAmount(decimal? value)
    {
        if (value is null || value.Value == 0m)
        {
            return null;
        }

        return decimal.Round(value.Value, 2);
    }
}
