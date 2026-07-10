using System.ComponentModel;
using FarmBooks.Core.Models;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseDetailsViewModel : ViewModelBase, IDataErrorInfo
{
    private string _expenseId = "";
    private DateTime _expenseDate = DateTime.Today;
    private DateTime? _paidDate;
    private string _sourceType = "Receipt";
    private string _documentNumber = "";
    private string _businessName = "";
    private string _description = "";
    private decimal _total;
    private decimal _vatc;
    private decimal _vats;
    private VatApplicability _vatApplicability = VatApplicability.NotSure;

    private VatEntryMethod _vatEntryMethod = VatEntryMethod.None;

    private bool _isVatClassificationConfirmed;
    private string _notes = "";
    private string _status = "";

    public string ExpenseId
    {
        get => _expenseId;
        set => SetProperty(ref _expenseId, value);
    }

    public DateTime ExpenseDate
    {
        get => _expenseDate;
        set => SetProperty(ref _expenseDate, value);
    }

    public DateTime? PaidDate
    {
        get => _paidDate;
        set => SetProperty(ref _paidDate, value);
    }

    public string SourceType
    {
        get => _sourceType;
        set => SetProperty(ref _sourceType, value);
    }

    public string DocumentNumber
    {
        get => _documentNumber;
        set => SetProperty(ref _documentNumber, value);
    }

    public string BusinessName
    {
        get => _businessName;
        set => SetProperty(ref _businessName, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    public decimal VATC
    {
        get => _vatc;
        set => SetProperty(ref _vatc, value);
    }

    public decimal VATS
    {
        get => _vats;
        set => SetProperty(ref _vats, value);
    }

    public VatApplicability VatApplicability
    {
        get => _vatApplicability;
        set => SetProperty(ref _vatApplicability, value);
    }

    public VatEntryMethod VatEntryMethod
    {
        get => _vatEntryMethod;
        set => SetProperty(ref _vatEntryMethod, value);
    }

    public bool IsVatClassificationConfirmed
    {
        get => _isVatClassificationConfirmed;
        set => SetProperty(ref _isVatClassificationConfirmed, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool HasErrors =>
        !string.IsNullOrWhiteSpace(this[nameof(ExpenseDate)])
        || !string.IsNullOrWhiteSpace(this[nameof(BusinessName)])
        || !string.IsNullOrWhiteSpace(this[nameof(Total)]);

    public string Error => "";

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(ExpenseDate) when ExpenseDate == default => "Expense date is required.",

                nameof(BusinessName) when string.IsNullOrWhiteSpace(BusinessName) =>
                    "Business name is required.",

                nameof(Total) when Total < 0 => "Total cannot be negative.",

                _ => "",
            };
        }
    }
}
