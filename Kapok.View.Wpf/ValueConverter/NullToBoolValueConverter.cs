using System.Globalization;
using System.Windows.Data;

namespace Kapok.View.Wpf;

// Source: http://vladimir-kurka.blogspot.com/2010/07/nulltoboolvalueconverter-for-wpf.html
/// <summary>
/// Converts null to boolean. 
/// </summary>
/// <remarks>
/// If parameter is not specified or specified with null:
/// if passed value is null then true is returned, otherwise false.
/// If parameter is specified then inversion is done:
/// if passed value is null then false is returned, otherwise true.
/// </remarks>
[ValueConversion(typeof(object), typeof(bool))]
public class NullToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool result = value == null;
        if (parameter != null)
            return !result;
        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}