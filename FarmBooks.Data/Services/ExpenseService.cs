using FarmBooks.Core.Models;
using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Data.Services;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Data.Services;

public sealed class ExpenseService
{
    private readonly ExpenseRepository _expenses;
    private readonly ExpenseLineItemRepository _lineItems;
    private readonly ExpenseMatchRepository _matches;
    private readonly ExpenseDocumentRepository _documents;
    private readonly AccountingCodeRepository _codes;

    public ExpenseService(
        ExpenseRepository expenses,
        ExpenseLineItemRepository lineItems,
        ExpenseMatchRepository matches,
        ExpenseDocumentRepository documents,
        AccountingCodeRepository codes)
    {
        _expenses = expenses;
        _lineItems = lineItems;
        _matches = matches;
        _documents = documents;
        _codes = codes;
    }

    public async Task<string> CreateExpenseAsync(
        DateTime expenseDate,
        DateTime? paidDate,
        ExpenseSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes)
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
            UpdatedAt = now
        };

        await _expenses.CreateAsync(expense);

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
        string? notes)
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            throw new InvalidOperationException("Expense not found.");

        if (total < 0)
            throw new InvalidOperationException("Expense total cannot be negative.");

        expense.ExpenseDate = expenseDate;
        expense.PaidDate = paidDate;
        expense.SourceType = sourceType;
        expense.DocumentNumber = documentNumber;
        expense.BusinessName = businessName;
        expense.Description = description;
        expense.Total = total;
        expense.Notes = notes;

        await _expenses.UpdateAsync(expense);
    }

    public Task DeleteExpenseAsync(string expenseId)
    {
        return _expenses.SoftDeleteAsync(expenseId);
    }

    public Task RestoreExpenseAsync(string expenseId)
    {
        return _expenses.RestoreAsync(expenseId);
    }

    public async Task<IReadOnlyList<ExpenseListItemDto>> GetExpenseListAsync()
    {
        var expenses = await _expenses.ListAsync();
        var codes = await _codes.ListActiveAsync();

        var result = new List<ExpenseListItemDto>();

        foreach (var expense in expenses)
        {
            var lineItems = await _lineItems.ListForExpenseAsync(expense.ExpenseId);
            var status = ExpenseStatusCalculator.Calculate(expense, lineItems);
            var isMatched = await _matches.IsExpenseMatchedAsync(expense.ExpenseId);
            var documentCount = await _documents.CountForExpenseAsync(expense.ExpenseId);

            result.Add(new ExpenseListItemDto
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
                IsMatched = isMatched,
                LineItemCount = lineItems.Count,
                DocumentCount = documentCount
            });
        }

        return result;
    }

    public async Task<ExpenseDetailsDto?> GetExpenseDetailsAsync(string expenseId)
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            return null;

        var lineItems = await _lineItems.ListForExpenseAsync(expenseId);
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
            VATC = expense.VATC,
            VATS = expense.VATS,
            Notes = expense.Notes,
            Status = status.ToString(),
            IsMatched = isMatched,

            LineItems = lineItems.Select(x =>
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
                    VATTreatment = x.VATTreatment
                };
            }).ToList(),

            Documents = documents.Select(x => new ExpenseDocumentDto
            {
                ExpenseDocumentId = x.ExpenseDocumentId,
                FileName = x.FileName,
                MimeType = x.MimeType,
                DocumentType = x.DocumentType,
                UploadedAt = x.UploadedAt
            }).ToList()
        };
    }
}