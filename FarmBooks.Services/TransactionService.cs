using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;
using FarmBooks.Services;

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
        DateTime paymentDate,
        TransactionSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes
    )
    {
        var now = DateTime.UtcNow;

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            ReceiptDate = receiptDate,
            PaymentDate = paymentDate,
            SourceType = sourceType,
            DocumentNumber = documentNumber,
            BusinessName = businessName,
            Description = description,
            Total = total,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _transactions.CreateAsync(transaction);

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
            throw new InvalidOperationException("Transaction not found.");

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
        DateTime paymentDate,
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
            throw new InvalidOperationException("Transaction not found.");

        ValidateVatSigns(total, vatC, vatS);

        var oldTransaction = new Transaction
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
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            DeletedAt = transaction.DeletedAt,
        };

        transaction.ReceiptDate = receiptDate;
        transaction.PaymentDate = paymentDate;
        transaction.SourceType = sourceType;
        transaction.DocumentNumber = string.IsNullOrWhiteSpace(documentNumber)
            ? null
            : documentNumber.Trim();

        transaction.BusinessName = string.IsNullOrWhiteSpace(businessName)
            ? null
            : businessName.Trim();

        transaction.Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

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
            // A deliberate No or Not Sure clears stale VAT details.
            transaction.VatEntryMethod = VatEntryMethod.None;
            transaction.VATC = null;
            transaction.VATS = null;
            transaction.IsVatClassificationConfirmed = false;
        }

        transaction.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        await _transactions.UpdateAsync(transaction);

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

        await _transactions.SoftDeleteAsync(transactionId);

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
        var codes = await _codes.ListActiveAsync();

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

            LineItems = lineItems
                .Select(x =>
                {
                    var code = codes.FirstOrDefault(c => c.CodeId == x.CodeId);

                    return new TransactionLineItemDto
                    {
                        TransactionLineItemId = x.TransactionLineItemId,
                        CodeId = x.CodeId,
                        Code = code?.Code,
                        CodeName = code?.Name,
                        Description = x.Description,
                        Total = x.Total,
                    };
                })
                .ToList(),

            Documents = documents
                .Select(x => new TransactionDocumentDto
                {
                    TransactionDocumentId = x.TransactionDocumentId,
                    FileName = x.FileName,
                    MimeType = x.MimeType,
                    DocumentType = x.DocumentType,
                    UploadedAt = x.UploadedAt,
                })
                .ToList(),
        };
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

    private static bool HasVatAmount(decimal? value)
    {
        return value is not null && value.Value != 0m;
    }

    private static decimal? NormalizeVatAmount(decimal? value)
    {
        if (value is null || value.Value == 0m)
            return null;

        return decimal.Round(value.Value, 2);
    }
}
