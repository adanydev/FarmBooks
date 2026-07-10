using System.Windows;
using System.Windows.Input;
using FarmBooks.UI.ViewModels;
using FarmBooks.UI.Views;

namespace FarmBooks.UI;

public partial class MainWindow : Window
{
    private readonly ExpensesView _expensesView;
    private readonly AccountingCodesView _accountingCodesView;

    public MainWindow(ExpensesView expensesView, AccountingCodesView accountingCodesView)
    {
        InitializeComponent();

        _expensesView = expensesView;
        _accountingCodesView = accountingCodesView;

        MainContent.Content = _expensesView;

        CommandBindings.Add(
            new CommandBinding(ApplicationCommands.New, (_, _) => OpenNewExpense())
        );

        InputBindings.Add(
            new KeyBinding(ApplicationCommands.New, new KeyGesture(Key.N, ModifierKeys.Control))
        );
    }

    private void ExpensesMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowExpenses();
    }

    private void AccountingCodesMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _accountingCodesView;
    }

    private void NewExpenseMenuItem_Click(object sender, RoutedEventArgs e)
    {
        OpenNewExpense();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowExpenses()
    {
        MainContent.Content = _expensesView;
    }

    private void OpenNewExpense()
    {
        ShowExpenses();

        if (
            _expensesView.DataContext is ExpensesViewModel viewModel
            && viewModel.NewExpenseCommand.CanExecute(null)
        )
        {
            viewModel.NewExpenseCommand.Execute(null);
        }
    }
}
