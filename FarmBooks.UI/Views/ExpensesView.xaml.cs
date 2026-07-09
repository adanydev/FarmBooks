using System.Windows.Controls;
using FarmBooks.UI.ViewModels;

namespace FarmBooks.UI.Views;

public partial class ExpensesView : UserControl
{
    public ExpensesView()
    {
        InitializeComponent();
        DataContext = new ExpensesViewModel();
    }
}