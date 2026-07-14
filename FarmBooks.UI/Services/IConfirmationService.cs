namespace FarmBooks.UI.Services;

public interface IConfirmationService
{
    bool Confirm(string message, string title);
}
