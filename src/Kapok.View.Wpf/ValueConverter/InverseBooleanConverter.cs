using System.Windows;
using System.Windows.Data;

namespace Kapok.View.Wpf;

[ValueConversion(typeof(bool), typeof(bool))]
[Localizability(LocalizationCategory.NeverLocalize)]
public class InverseBooleanConverter : IValueConverter
{
    private static InverseBooleanConverter? _defaultConverter;

    public static InverseBooleanConverter DefaultConverter =>
        _defaultConverter ??= new InverseBooleanConverter();

    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        // ReSharper disable once PossibleNullReferenceException
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        // ReSharper disable once PossibleNullReferenceException
        return !(bool)value;
    }

    #endregion
}