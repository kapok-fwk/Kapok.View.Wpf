using System.Globalization;
using System.Windows.Data;

namespace Kapok.View.Wpf;

[ValueConversion(typeof(Caption), typeof(string))]
public class CaptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;
        if (!(value is Caption captionValue))
            throw new NotSupportedException($"The object value must be of type Caption to be able to use the {typeof(CaptionConverter).FullName} class");

        return captionValue.LanguageOrDefault(culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}