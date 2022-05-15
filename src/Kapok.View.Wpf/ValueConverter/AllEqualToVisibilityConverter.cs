using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kapok.View.Wpf;

/// <summary>
/// Returns 'Visible' when all values have the same value (and are not null), if not, 'Collapsed'
/// </summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public class AllEqualToVisibilityConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null)
            return Visibility.Collapsed;

        object? lastValue = null;

        foreach (var value in values)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (lastValue == null)
            {
                lastValue = value;
            }
            else if (!lastValue.Equals(value))
            {
                return Visibility.Collapsed;
            }
        }

        return Visibility.Visible;
    }

    public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}