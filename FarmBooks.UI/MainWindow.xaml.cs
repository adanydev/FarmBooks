using System.Windows;
using System.Windows.Input;
using FarmBooks.UI.ViewModels;
using FarmBooks.UI.Views;

namespace FarmBooks.UI;

public partial class MainWindow : Window
{
    private readonly TransactionsView _transactionsView;
    private readonly AccountingCodesView _accountingCodesView;

    public MainWindow(TransactionsView transactionsView, AccountingCodesView accountingCodesView)
    {
        InitializeComponent();

        _transactionsView = transactionsView;
        _accountingCodesView = accountingCodesView;

        MainContent.Content = _transactionsView;

        CommandBindings.Add(
            new CommandBinding(ApplicationCommands.New, (_, _) => OpenNewTransaction())
        );

        InputBindings.Add(
            new KeyBinding(ApplicationCommands.New, new KeyGesture(Key.N, ModifierKeys.Control))
        );
    }

    private void TransactionsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowTransactions();
    }

    private void AccountingCodesMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _accountingCodesView;
    }

    private void NewTransactionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        OpenNewTransaction();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowTransactions()
    {
        MainContent.Content = _transactionsView;
    }

    private void OpenNewTransaction()
    {
        ShowTransactions();

        if (
            _transactionsView.DataContext is TransactionsViewModel viewModel
            && viewModel.NewTransactionCommand.CanExecute(null)
        )
        {
            viewModel.NewTransactionCommand.Execute(null);
        }
    }
}
