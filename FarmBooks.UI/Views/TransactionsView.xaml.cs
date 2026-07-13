using System.Windows;
using System.Windows.Controls;
using FarmBooks.UI.ViewModels;

namespace FarmBooks.UI.Views;

public partial class TransactionsView : UserControl
{
    private readonly TransactionsViewModel _viewModel;

    public TransactionsView(TransactionsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += TransactionsView_Loaded;
    }

    private async void TransactionsView_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}
