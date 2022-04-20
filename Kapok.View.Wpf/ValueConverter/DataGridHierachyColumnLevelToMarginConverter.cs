using System.Windows;
using System.Windows.Data;

namespace Kapok.View.Wpf;

//[ValueConversion(typeof(int), typeof(Thickness), ParameterType = typeof(string))]
public class DataGridHierarchyColumnLevelToMarginConverter : IValueConverter
{
    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value == null) return null;

        int lvl = (int)value;

        if (parameter == null)
            return new Thickness(20 * lvl, 0, 0, 0);

        switch (parameter.ToString())
        {
            case "HLine":
                return new Thickness(20 * lvl + 9, 1, 0, 0);
            case "VLine":
                return new Thickness(20 * lvl, 0, 0, 0);
            // ReSharper disable once RedundantCaseLabel
            case "ToggleButton":
            default:
                return new Thickness(20 * lvl, 1, 0, 0);
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    #endregion
}