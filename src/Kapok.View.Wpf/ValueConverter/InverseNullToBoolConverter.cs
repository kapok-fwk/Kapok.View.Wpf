using System.Globalization;
using System.Windows.Data;

namespace Kapok.View.Wpf;

/// <summary>
/// Converts null to boolean (inversed). 
/// </summary>
/// <remarks>
/// If parameter is not specified or specified with null:
/// if passed value is null then false is returned, otherwise true.
/// If parameter is specified then inversion is done:
/// if passed value is null then true is returned, otherwise false.
/// </remarks>
[ValueConversion(typeof(object), typeof(bool))]
public class InverseNullToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool result = value == null;
        if (parameter != null)
            return result;
        return !result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}