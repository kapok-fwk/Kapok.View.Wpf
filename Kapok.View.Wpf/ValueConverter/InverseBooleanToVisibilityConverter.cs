using System.Windows;
using System.Windows.Data;

namespace Kapok.View.Wpf;

//[ValueConversion(typeof(bool), typeof(Visibility))]
//[ValueConversion(typeof(bool?), typeof(Visibility))]
[Localizability(LocalizationCategory.NeverLocalize)]
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        bool flag = false;
        if (value is bool)
            flag = (bool) value;
        else if (value is bool?)
        {
            bool? nullable = (bool?) value;
            flag = nullable.HasValue && nullable.Value;
        }
        return (Visibility) (flag ? 2 : 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility != Visibility.Visible;
        return true;
    }

    #endregion
}