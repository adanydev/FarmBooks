using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;
using FarmBooks.Services;

namespace FarmBooks.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly ExpenseRepository _expenses;
    private readonly ExpenseLineItemRepository _lineItems;
    private readonly ExpenseMatchRepository _matches;
    private readonly ExpenseDocumentRepository _documents;
    private readonly AccountingCodeRepository _codes;
    private readonly AuditService _auditService;

    public ExpenseService(
        ExpenseRepository expenses,
        ExpenseLineItemRepository lineItems,
        ExpenseMatchRepository matches,
        ExpenseDocumentRepository documents,
        AccountingCodeRepository codes,
        AuditService auditService
    )
    {
        _expenses = expenses;
        _lineItems = lineItems;
        _matches = matches;
        _documents = documents;
        _codes = codes;
        _auditService = auditService;
    }

    public async Task<string> CreateExpenseAsync(
        DateTime expenseDate,
        DateTime? paidDate,
        ExpenseSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes
    )
    {
        if (expenseDate == default)
            throw new InvalidOperationException("Expense date is required.");

        if (total < 0)
            throw new InvalidOperationException("Expense total cannot be negative.");

        var now = DateTime.UtcNow;

        var expense = new Expense
        {
            ExpenseId = Guid.NewGuid().ToString(),
            ExpenseDate = expenseDate,
            PaidDate = paidDate,
            SourceType = sourceType,
            DocumentNumber = documentNumber,
            BusinessName = businessName,
            Description = description,
            Total = total,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _expenses.CreateAsync(expense);

        await _auditService.WriteAsync("Expense", expense.ExpenseId, "Created", null, expense);

        return expense.ExpenseId;
    }

    public async Task<ExpenseStatus> GetStatusAsync(string expenseId)
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            throw new InvalidOperationException("Expense not found.");

        var lineItems = await _lineItems.ListForExpenseAsync(expenseId);

        return ExpenseStatusCalculator.Calculate(expense, lineItems);
    }

    public Task<IReadOnlyList<Expense>> ListExpensesAsync()
    {
        return _expenses.ListAsync();
    }

    public async Task UpdateExpenseAsync(
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
    )
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            throw new InvalidOperationException("Expense not found.");

        if (expenseDate == default)
            throw new InvalidOperationException("Expense date is required.");

        if (total < 0)
            throw new InvalidOperationException("Expense total cannot be negative.");

        ValidateVat(vatApplicability, vatEntryMethod, vatC, vatS, isVatClassificationConfirmed);

        var oldExpense = new Expense
        {
            ExpenseId = expense.ExpenseId,
            ExpenseDate = expense.ExpenseDate,
            PaidDate = expense.PaidDate,
            SourceType = expense.SourceType,
            DocumentNumber = expense.DocumentNumber,
            BusinessName = expense.BusinessName,
            Description = expense.Description,
            Total = expense.Total,
            VatApplicability = expense.VatApplicability,
            VatEntryMethod = expense.VatEntryMethod,
            VATC = expense.VATC,
            VATS = expense.VATS,
            IsVatClassificationConfirmed = expense.IsVatClassificationConfirmed,
            Notes = expense.Notes,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt,
            DeletedAt = expense.DeletedAt,
        };

        expense.ExpenseDate = expenseDate;
        expense.PaidDate = paidDate;
        expense.SourceType = sourceType;
        expense.DocumentNumber = string.IsNullOrWhiteSpace(documentNumber)
            ? null
            : documentNumber.Trim();

        expense.BusinessName = string.IsNullOrWhiteSpace(businessName) ? null : businessName.Trim();

        expense.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        expense.Total = total;
        expense.VatApplicability = vatApplicability;

        if (vatApplicability == VatApplicability.Yes)
        {
            expense.VatEntryMethod = vatEntryMethod;
            expense.VATC = NormalizeVatAmount(vatC);
            expense.VATS = NormalizeVatAmount(vatS);
            expense.IsVatClassificationConfirmed = isVatClassificationConfirmed;
        }
        else
        {
            // A deliberate No or Not Sure clears stale VAT details.
            expense.VatEntryMethod = VatEntryMethod.None;
            expense.VATC = null;
            expense.VATS = null;
            expense.IsVatClassificationConfirmed = false;
        }

        expense.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        await _expenses.UpdateAsync(expense);

        await _auditService.WriteAsync(
            "Expense",
            expense.ExpenseId,
            "Updated",
            oldExpense,
            expense
        );
    }

    public async Task DeleteExpenseAsync(string expenseId)
    {
        var expense = await _expenses.GetAsync(expenseId);

        await _expenses.SoftDeleteAsync(expenseId);

        await _auditService.WriteAsync("Expense", expenseId, "Deleted", expense, null);
    }

    public async Task RestoreExpenseAsync(string expenseId)
    {
        await _expenses.RestoreAsync(expenseId);

        await _auditService.WriteAsync(
            "Expense",
            expenseId,
            "Restored",
            null,
            new { ExpenseId = expenseId }
        );
    }

    public async Task<IReadOnlyList<ExpenseListItemDto>> GetExpenseListAsync()
    {
        var expenses = await _expenses.ListAsync();
        var codes = await _codes.ListActiveAsync();

        var result = new List<ExpenseListItemDto>();

        foreach (var expense in expenses)
        {
            var lineItems = await _lineItems.ListForExpenseAsync(expense.ExpenseId);
            var workflow = ExpenseWorkflowStatusCalculator.Calculate(expense, lineItems);
            var status = ExpenseStatusCalculator.Calculate(expense, lineItems);
            var isMatched = await _matches.IsExpenseMatchedAsync(expense.ExpenseId);
            var documentCount = await _documents.CountForExpenseAsync(expense.ExpenseId);

            result.Add(
                new ExpenseListItemDto
                {
                    ExpenseId = expense.ExpenseId,
                    ExpenseDate = expense.ExpenseDate,
                    PaidDate = expense.PaidDate,
                    SourceType = expense.SourceType.ToString(),
                    DocumentNumber = expense.DocumentNumber,
                    BusinessName = expense.BusinessName,
                    Description = expense.Description,
                    Total = expense.Total,
                    Status = status.ToString(),
                    IsVatReady = workflow.IsVatReady,
                    IsTaxReady = workflow.IsTaxReady,
                    VatIssueCount = workflow.VatIssues.Count,
                    TaxIssueCount = workflow.TaxIssues.Count,
                    IsMatched = isMatched,
                    LineItemCount = lineItems.Count,
                    DocumentCount = documentCount,
                }
            );
        }

        return result;
    }

    public async Task<ExpenseDetailsDto?> GetExpenseDetailsAsync(string expenseId)
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            return null;

        var lineItems = await _lineItems.ListForExpenseAsync(expenseId);
        var workflow = ExpenseWorkflowStatusCalculator.Calculate(expense, lineItems);
        var documents = await _documents.ListForExpenseAsync(expenseId);
        var codes = await _codes.ListActiveAsync();
        var status = ExpenseStatusCalculator.Calculate(expense, lineItems);
        var isMatched = await _matches.IsExpenseMatchedAsync(expenseId);

        return new ExpenseDetailsDto
        {
            ExpenseId = expense.ExpenseId,
            ExpenseDate = expense.ExpenseDate,
            PaidDate = expense.PaidDate,
            SourceType = expense.SourceType.ToString(),
            DocumentNumber = expense.DocumentNumber,
            BusinessName = expense.BusinessName,
            Description = expense.Description,
            Total = expense.Total,
            Notes = expense.Notes,
            VatApplicability = expense.VatApplicability,
            VatEntryMethod = expense.VatEntryMethod,
            VATC = expense.VATC,
            VATS = expense.VATS,
            IsVatClassificationConfirmed = expense.IsVatClassificationConfirmed,
            Status = status.ToString(),
            WorkflowStatus = workflow,
            IsMatched = isMatched,

            LineItems = lineItems
                .Select(x =>
                {
                    var code = codes.FirstOrDefault(c => c.CodeId == x.CodeId);

                    return new ExpenseLineItemDto
                    {
                        ExpenseLineItemId = x.ExpenseLineItemId,
                        CodeId = x.CodeId,
                        Code = code?.Code,
                        CodeName = code?.Name,
                        Description = x.Description,
                        Total = x.Total,
                    };
                })
                .ToList(),

            Documents = documents
                .Select(x => new ExpenseDocumentDto
                {
                    ExpenseDocumentId = x.ExpenseDocumentId,
                    FileName = x.FileName,
                    MimeType = x.MimeType,
                    DocumentType = x.DocumentType,
                    UploadedAt = x.UploadedAt,
                })
                .ToList(),
        };
    }

    private static void ValidateVat(
        VatApplicability vatApplicability,
        VatEntryMethod vatEntryMethod,
        decimal? vatC,
        decimal? vatS,
        bool isVatClassificationConfirmed
    )
    {
        if (vatC is < 0)
            throw new InvalidOperationException("VATC cannot be negative.");

        if (vatS is < 0)
            throw new InvalidOperationException("VATS cannot be negative.");

        switch (vatApplicability)
        {
            case VatApplicability.NotSure:
                if (vatEntryMethod != VatEntryMethod.None)
                {
                    throw new InvalidOperationException(
                        "A VAT entry method cannot be selected while VAT is not sure."
                    );
                }

                if (HasVatAmount(vatC) || HasVatAmount(vatS))
                {
                    throw new InvalidOperationException(
                        "VATC and VATS cannot be entered while VAT is not sure."
                    );
                }

                if (isVatClassificationConfirmed)
                {
                    throw new InvalidOperationException(
                        "VATC/VATS cannot be confirmed while VAT is not sure."
                    );
                }

                break;

            case VatApplicability.No:
                if (vatEntryMethod != VatEntryMethod.None)
                {
                    throw new InvalidOperationException(
                        "A VAT entry method cannot be selected when VAT does not apply."
                    );
                }

                if (HasVatAmount(vatC) || HasVatAmount(vatS))
                {
                    throw new InvalidOperationException(
                        "An expense marked as having no VAT cannot contain VATC or VATS."
                    );
                }

                // Confirmation is intentionally not required for No VAT.
                break;

            case VatApplicability.Yes:
                if (vatEntryMethod == VatEntryMethod.None)
                {
                    throw new InvalidOperationException(
                        "Choose whether the VAT was entered or calculated."
                    );
                }

                // Do not require amounts or confirmation here.
                // Unfinished work must still be saveable and will be flagged
                // by the workflow calculator.
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(vatApplicability),
                    vatApplicability,
                    "Unknown VAT applicability value."
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
