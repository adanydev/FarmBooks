using System.Globalization;
using System.Windows.Data;

namespace FarmBooks.UI.Infrastructure;

public sealed class NullableDateConverter : IValueConverter
{
    private const string DateFormat = "dd/MM/yyyy";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DateTime date
            ? date.ToString(DateFormat, CultureInfo.InvariantCulture)
            : "";
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        var text = value?.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Support 04032026
        if (text.Length == 8 && text.All(char.IsDigit))
        {
            text = $"{text[..2]}/{text.Substring(2, 2)}/{text.Substring(4, 4)}";
        }

        // Support 432026 -> 04/03/2026 isn't realistic,
        // so only handle properly delimited dates beyond this point.

        string[] formats =
        [
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd/M/yyyy",
            "d/MM/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "dd.MM.yyyy",
            "d.M.yyyy",
            "dd/MM/yy",
            "d/M/yy",
        ];

        if (
            DateTime.TryParseExact(
                text,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date
            )
        )
        {
            return date;
        }

        return Binding.DoNothing;
    }
}
