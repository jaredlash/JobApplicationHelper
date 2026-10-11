using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JobApplicationHelper.Converters;

public sealed class BooleanToGridLengthConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return value is true
            ? GridLength.Auto
            : new GridLength(0);
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        Binding.DoNothing;
}