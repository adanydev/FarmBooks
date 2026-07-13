using FarmBooks.Core.Constants;
using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public static class TransactionWorkflowStatusCalculator
{
    private const decimal TotalTolerance = 0.01m;

    public static TransactionWorkflowStatusDto Calculate(
        Transaction transaction,
        IReadOnlyList<TransactionLineItem> lineItems
    )
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(lineItems);

        var vatIssues = CalculateVatIssues(transaction);
        var taxIssues = CalculateTaxIssues(transaction, lineItems);

        return new TransactionWorkflowStatusDto
        {
            IsVatReady = vatIssues.Count == 0,
            IsTaxReady = taxIssues.Count == 0,
            VatIssues = vatIssues,
            TaxIssues = taxIssues,
        };
    }

    private static IReadOnlyList<TransactionWorkflowIssueDto> CalculateVatIssues(
        Transaction transaction
    )
    {
        var issues = new List<TransactionWorkflowIssueDto>();

        if (transaction.PaidDate is null)
        {
            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.MissingPaidDate,
                    Message = TransactionWorkflowMessages.PaymentDateMissing,
                }
            );
        }

        if (transaction.VatApplicability == VatApplicability.NotSure)
        {
            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.VatNotSure,
                    Message = TransactionWorkflowMessages.ChooseVatApplicability,
                }
            );

            return issues;
        }

        if (transaction.VatApplicability == VatApplicability.No)
        {
            return issues;
        }

        if (transaction.VatEntryMethod == VatEntryMethod.None)
        {
            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.VatNotReviewed,
                    Message = TransactionWorkflowMessages.ChooseVatEntryMethod,
                }
            );
        }

        var vatTotal = (transaction.VATC ?? 0m) + (transaction.VATS ?? 0m);

        if (!transaction.IsVatClassificationConfirmed)
        {
            if (vatTotal <= 0m)
            {
                issues.Add(
                    new TransactionWorkflowIssueDto
                    {
                        Code = TransactionWorkflowIssueCodes.VatAmountMissing,
                        Message = TransactionWorkflowMessages.VatAmountsMissing,
                    }
                );
            }

            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.VatClassificationNotConfirmed,
                    Message = TransactionWorkflowMessages.ConfirmVatClassification,
                }
            );
        }

        return issues;
    }

    private static IReadOnlyList<TransactionWorkflowIssueDto> CalculateTaxIssues(
        Transaction transaction,
        IReadOnlyList<TransactionLineItem> lineItems
    )
    {
        var issues = new List<TransactionWorkflowIssueDto>();

        if (lineItems.Count == 0)
        {
            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.MissingLineItems,
                    Message = TransactionWorkflowMessages.MissingLineItems,
                }
            );

            return issues;
        }

        if (lineItems.Any(item => string.IsNullOrWhiteSpace(item.CodeId)))
        {
            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.MissingAccountingCode,
                    Message = TransactionWorkflowMessages.MissingAccountingCode,
                }
            );
        }

        var lineItemTotal = lineItems.Sum(item => item.Total);
        var difference = Math.Abs(transaction.Total - lineItemTotal);

        if (difference > TotalTolerance)
        {
            issues.Add(
                new TransactionWorkflowIssueDto
                {
                    Code = TransactionWorkflowIssueCodes.LineTotalMismatch,
                    Message = TransactionWorkflowMessages.LineItemTotalMismatch,
                }
            );
        }

        return issues;
    }
}
