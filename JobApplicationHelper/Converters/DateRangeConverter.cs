using System.Globalization;
using System.Windows.Data;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Converters;

public sealed class DateRangeConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is not DateRange dateRange)
            return string.Empty;

        var text = dateRange.ToString();

        return string.IsNullOrEmpty(text)
            ? string.Empty
            : $"({text})";
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
