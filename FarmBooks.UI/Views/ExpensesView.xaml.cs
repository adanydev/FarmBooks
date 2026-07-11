using System.Windows;
using System.Windows.Controls;
using FarmBooks.UI.ViewModels;

namespace FarmBooks.UI.Views;

public partial class ExpensesView : UserControl
{
    private readonly ExpensesViewModel _viewModel;

    public ExpensesView(ExpensesViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += ExpensesView_Loaded;
    }

    private async void ExpensesView_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}
