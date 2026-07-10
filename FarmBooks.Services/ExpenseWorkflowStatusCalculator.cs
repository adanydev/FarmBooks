using FarmBooks.Core.Constants;
using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public static class ExpenseWorkflowStatusCalculator
{
    private const decimal TotalTolerance = 0.01m;

    public static ExpenseWorkflowStatusDto Calculate(
        Expense expense,
        IReadOnlyList<ExpenseLineItem> lineItems
    )
    {
        ArgumentNullException.ThrowIfNull(expense);
        ArgumentNullException.ThrowIfNull(lineItems);

        var vatIssues = CalculateVatIssues(expense);
        var taxIssues = CalculateTaxIssues(expense, lineItems);

        return new ExpenseWorkflowStatusDto
        {
            IsVatReady = vatIssues.Count == 0,
            IsTaxReady = taxIssues.Count == 0,
            VatIssues = vatIssues,
            TaxIssues = taxIssues,
        };
    }

    private static IReadOnlyList<ExpenseWorkflowIssueDto> CalculateVatIssues(Expense expense)
    {
        var issues = new List<ExpenseWorkflowIssueDto>();

        if (expense.PaidDate is null)
        {
            issues.Add(
                new ExpenseWorkflowIssueDto
                {
                    Code = ExpenseWorkflowIssueCodes.MissingPaidDate,
                    Message = "Payment date is missing.",
                }
            );
        }

        // VAT review and confirmation rules will be added after the
        // corresponding persisted fields are introduced.

        return issues;
    }

    private static IReadOnlyList<ExpenseWorkflowIssueDto> CalculateTaxIssues(
        Expense expense,
        IReadOnlyList<ExpenseLineItem> lineItems
    )
    {
        var issues = new List<ExpenseWorkflowIssueDto>();

        if (lineItems.Count == 0)
        {
            issues.Add(
                new ExpenseWorkflowIssueDto
                {
                    Code = ExpenseWorkflowIssueCodes.MissingLineItems,
                    Message = "The expense has no line items.",
                }
            );

            return issues;
        }

        if (lineItems.Any(item => string.IsNullOrWhiteSpace(item.CodeId)))
        {
            issues.Add(
                new ExpenseWorkflowIssueDto
                {
                    Code = ExpenseWorkflowIssueCodes.MissingAccountingCode,
                    Message = "One or more line items need an accounting code.",
                }
            );
        }

        var lineItemTotal = lineItems.Sum(item => item.Total);
        var difference = Math.Abs(expense.Total - lineItemTotal);

        if (difference > TotalTolerance)
        {
            issues.Add(
                new ExpenseWorkflowIssueDto
                {
                    Code = ExpenseWorkflowIssueCodes.LineTotalMismatch,
                    Message = "The line-item total does not match the expense total.",
                }
            );
        }

        return issues;
    }
}
