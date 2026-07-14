using System.Windows;

namespace FarmBooks.UI.Services;

public sealed class ConfirmationService : IConfirmationService
{
    public bool Confirm(string message, string title)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No
        );
        return result == MessageBoxResult.Yes;
    }
}
