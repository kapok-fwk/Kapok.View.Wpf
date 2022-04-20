using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf;

public static class DataGridColumnExtensions
{
    public static readonly DependencyProperty CanUserFilterProperty =
        DependencyProperty.RegisterAttached("CanUserFilter",
            typeof(bool), typeof(DataGridColumn),
            new PropertyMetadata(true, null, null));

    public static bool GetCanUserFilter(DependencyObject target)
    {
        return (bool)target.GetValue(CanUserFilterProperty);
    }

    public static void SetCanUserFilter(DependencyObject target, bool value)
    {
        target.SetValue(CanUserFilterProperty, value);
    }
        
    public static readonly DependencyProperty HeaderTooltipProperty =
        DependencyProperty.Register("HeaderTooltip",
            typeof(object), typeof(DataGridColumn),
            new PropertyMetadata(null, null, null));

    public static object GetHeaderTooltip(DependencyObject target)
    {
        return target.GetValue(HeaderTooltipProperty);
    }

    public static void SetHeaderTooltip(DependencyObject target, object value)
    {
        target.SetValue(HeaderTooltipProperty, value);
    }

    public static readonly DependencyProperty ColumnViewModelProperty =
        DependencyProperty.Register("ColumnViewModel",
            typeof(object), typeof(DataGridColumn),
            new PropertyMetadata(null, null, null));

    public static object GetColumnViewModel(DependencyObject target)
    {
        return target.GetValue(ColumnViewModelProperty);
    }

    public static void SetColumnViewModel(DependencyObject target, object value)
    {
        target.SetValue(ColumnViewModelProperty, value);
    }
}