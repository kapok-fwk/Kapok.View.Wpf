using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kapok.View.Wpf;

/// <summary>
/// Returns the specific color for a data set entity.
///
/// object[0] must be the WPF DataSet
/// object[1] must be the entity
/// object[2] (optional) is the name of the property of the entity to be colored. If not given, the general color is taken.
///
/// Parameter:
///     Foreground - the foreground color
///     Background - the background color
///     BackgroundAlternationIndex1 - the background with alternation index 1
///     ForegroundSelected - the foreground color when entity is selected (e.g. in a DataGrid)
///     BackgroundSelected - the background color when entity is selected
///     BorderBrushSelected - the border brush when the cell is selected
/// </summary>
public class DataSetEntityColorConverter : IMultiValueConverter
{
    // fallback values
    public Brush FallbackForeground { get; set; }
    public Brush FallbackBackground { get; set; }
    public Brush FallbackBackgroundAlternationIndex1 { get; set; }
    public Brush FallbackForegroundSelected { get; set; }
    public Brush FallbackBackgroundSelected { get; set; }
    public Brush FallbackBorderBrushSelected { get; set; }

    #region IMultiValueConverter

    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return Binding.DoNothing;

        var dataSetView = values[0] as IWpfDataSetView;
        if (dataSetView == null)
            return Binding.DoNothing;

        System.Drawing.Color? color;

        string? propertyName = values.Length >= 3 ? values[2] as string : null;

        switch (parameter)
        {
            case "Foreground":
                color = dataSetView.GetForegroundColorOfEntity(values[1], propertyName);
                break;
            case "Background":
            case "BackgroundAlternationIndex1":
                color = dataSetView.GetBackgroundColorOfEntity(values[1], propertyName);
                break;
            case "ForegroundSelected":
                color = dataSetView.GetForegroundSelectedColorOfEntity(values[1], propertyName);
                break;
            case "BorderBrushSelected":
            case "BackgroundSelected":
                color = dataSetView.GetBackgroundSelectedColorOfEntity(values[1], propertyName);
                break;
            default:
                color = null;
                break;
        }

        if (color.HasValue)
            return new SolidColorBrush(color.Value.ToMediaColor());

        // fallback values
        switch (parameter)
        {
            case "Foreground":
                return FallbackForeground ?? Binding.DoNothing;
            case "Background":
                return FallbackBackground ?? Binding.DoNothing;
            case "BackgroundAlternationIndex1":
                return FallbackBackgroundAlternationIndex1 ?? FallbackBackground ?? Binding.DoNothing;
            case "ForegroundSelected":
                return FallbackForegroundSelected ?? Binding.DoNothing;
            case "BackgroundSelected":
                return FallbackBackgroundSelected ?? Binding.DoNothing;
            case "BorderBrushSelected":
                return FallbackBorderBrushSelected ?? Binding.DoNothing;
            default:
                return Binding.DoNothing;
        }
    }

    public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException($"{nameof(DataSetEntityColorConverter)} is a OneWay converter.");
    }        

    #endregion
}