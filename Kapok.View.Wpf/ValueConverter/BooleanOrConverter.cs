using System.Windows.Data;

namespace Kapok.View.Wpf;

public class BooleanOrConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (values == null)
            return false;

        foreach (object? value in values)
        {
            if (value is bool boolValue && boolValue)
            {
                return true;
            }
        }
        return false;
    }
    public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}