using System.Windows.Controls;
using FarmBooks.UI.ViewModels;

namespace FarmBooks.UI.Views;

public partial class AccountingCodesView : UserControl
{
    public AccountingCodesView(AccountingCodesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
