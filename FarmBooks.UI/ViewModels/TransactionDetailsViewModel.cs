using System.ComponentModel;
using System.Windows.Input;
using FarmBooks.Core;
using FarmBooks.Core.Models;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionDetailsViewModel : ViewModelBase, IDataErrorInfo
{
    private string _transactionId = "";
    private DateTime? _receiptDate;
    private DateTime? _paymentDate;
    private string _sourceType = "Receipt";
    private string _documentNumber = "";
    private string _businessName = "";
    private string _description = "";
    private decimal _total;

    private VatApplicability _vatApplicability = VatApplicability.NotSure;

    private VatEntryMethod _vatEntryMethod = VatEntryMethod.None;

    private decimal? _vatc;
    private decimal? _vats;
    private bool _isVatClassificationConfirmed;

    private string _notes = "";
    private string _status = "";

    private bool _suppressVatConfirmationReset;

    public string VatCalculationToolTip =>
        $"""
Calculate VAT

Formula:
(Total ÷ (1 + VAT Rate)) × VAT Rate

Current VAT Rate: {TaxConstants.VatRate:P0}
""";

    public ICommand CalculateVatCCommand { get; }
    public ICommand CalculateVatSCommand { get; }

    public TransactionDetailsViewModel()
    {
        CalculateVatCCommand = new RelayCommand(CalculateVatC);
        CalculateVatSCommand = new RelayCommand(CalculateVatS);
    }

    public bool IsVatEntered
    {
        get => VatEntryMethod == VatEntryMethod.Entered;
        set
        {
            if (!value)
            {
                if (VatEntryMethod == VatEntryMethod.Entered)
                {
                    VatEntryMethod = VatEntryMethod.None;
                }

                return;
            }

            VatEntryMethod = VatEntryMethod.Entered;
        }
    }

    public bool IsVatCalculated
    {
        get => VatEntryMethod == VatEntryMethod.Calculated;
        set
        {
            if (!value)
            {
                if (VatEntryMethod == VatEntryMethod.Calculated)
                {
                    VatEntryMethod = VatEntryMethod.None;
                }

                return;
            }

            VatEntryMethod = VatEntryMethod.Calculated;
        }
    }

    public string TransactionId
    {
        get => _transactionId;
        set => SetProperty(ref _transactionId, value);
    }

    public DateTime? ReceiptDate
    {
        get => _receiptDate;
        set => SetProperty(ref _receiptDate, value);
    }

    public DateTime? PaymentDate
    {
        get => _paymentDate;
        set => SetProperty(ref _paymentDate, value);
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

    public VatApplicability VatApplicability
    {
        get => _vatApplicability;
        set
        {
            if (!SetProperty(ref _vatApplicability, value))
                return;

            if (value != VatApplicability.Yes)
            {
                VatEntryMethod = VatEntryMethod.None;
                VATC = null;
                VATS = null;
                IsVatClassificationConfirmed = false;
            }

            OnPropertyChanged(nameof(HasVat));
        }
    }

    public bool HasVat => VatApplicability == VatApplicability.Yes;

    public VatEntryMethod VatEntryMethod
    {
        get => _vatEntryMethod;
        set
        {
            if (!SetProperty(ref _vatEntryMethod, value))
                return;

            OnPropertyChanged(nameof(IsVatEntered));
            OnPropertyChanged(nameof(IsVatCalculated));
        }
    }

    public decimal? VATC
    {
        get => _vatc;
        set
        {
            if (!SetProperty(ref _vatc, value))
                return;

            if (!_suppressVatConfirmationReset)
            {
                IsVatClassificationConfirmed = false;
            }
        }
    }

    public decimal? VATS
    {
        get => _vats;
        set
        {
            if (!SetProperty(ref _vats, value))
                return;

            if (!_suppressVatConfirmationReset)
            {
                IsVatClassificationConfirmed = false;
            }
        }
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
        !string.IsNullOrWhiteSpace(this[nameof(PaymentDate)])
        || !string.IsNullOrWhiteSpace(this[nameof(BusinessName)])
        || !string.IsNullOrWhiteSpace(this[nameof(Total)]);

    public string Error => "";

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(PaymentDate) when PaymentDate == default => "Payment date is required.",

                nameof(BusinessName) when string.IsNullOrWhiteSpace(BusinessName) =>
                    "Business name is required.",

                nameof(Total) when Total == 0m => "Total cannot be zero.",

                _ => "",
            };
        }
    }

    private void CalculateVatC()
    {
        VATC = CalculateVatFromTotal();
        VATS = null;
        VatEntryMethod = VatEntryMethod.Calculated;
    }

    private void CalculateVatS()
    {
        VATS = CalculateVatFromTotal();
        VATC = null;
        VatEntryMethod = VatEntryMethod.Calculated;
    }

    private decimal CalculateVatFromTotal()
    {
        if (Total == 0m)
            return 0m;

        var vat = (Total / TaxConstants.VatMultiplier) * TaxConstants.VatRate;
        return decimal.Round(vat, 2, MidpointRounding.AwayFromZero);
    }

    public void LoadVatValues(
        VatApplicability vatApplicability,
        VatEntryMethod vatEntryMethod,
        decimal? vatC,
        decimal? vatS,
        bool isConfirmed
    )
    {
        _suppressVatConfirmationReset = true;

        try
        {
            VatApplicability = vatApplicability;
            VatEntryMethod = vatEntryMethod;
            VATC = vatC;
            VATS = vatS;
            IsVatClassificationConfirmed = isConfirmed;
        }
        finally
        {
            _suppressVatConfirmationReset = false;
        }
    }
}
