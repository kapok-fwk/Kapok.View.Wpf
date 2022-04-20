using System.Windows.Data;
using System.Globalization;

namespace Kapok.View.Wpf.DataGridFilterSupport;

public class FontSizeToHeightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;

        if(double.TryParse(value.ToString(), out var height))
        {
            return height * 2;
        }

        return double.NaN;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}