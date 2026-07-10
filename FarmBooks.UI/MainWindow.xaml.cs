using System.Windows;
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
    }

    private void ExpensesButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _expensesView;
    }

    private void AccountingCodesButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _accountingCodesView;
    }
}
