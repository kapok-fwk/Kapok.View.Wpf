using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf;

public static class DataPickerExtension
{
    #region ShowNullWhenDateTimeMinValueProperty

    // Source: https://michaelmairegger.wordpress.com/2012/05/31/binding-null-value-of-datetime-to-datepicker-using-an-attached-property/

    public static readonly DependencyProperty ShowNullWhenDateTimeMinValueProperty =
        DependencyProperty.RegisterAttached("ShowNullWhenDateTimeMinValue",
            typeof(bool),
            typeof(DataPickerExtension),
            new PropertyMetadata(default(bool),
                SetShowNullWhenDateTimeMinValueChanged));

    private static void SetShowNullWhenDateTimeMinValueChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        DatePicker control = (DatePicker)d;
        if ((bool)e.NewValue)
        {
            control.SelectedDateChanged += OnSelectedDateChanged;
        }
        else
        {
            control.SelectedDateChanged -= OnSelectedDateChanged;
        }
    }

    private static void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        UIElement control = (UIElement)sender;

        if ((bool)control.GetValue(ShowNullWhenDateTimeMinValueProperty))
        {
            var currentSelectedDate = ((DateTime?)control.GetValue(DatePicker.SelectedDateProperty));
            if (currentSelectedDate.HasValue && currentSelectedDate.Value == DateTime.MinValue)
            {
                //currently set DateTime.Now that the DatePicker opens the Popup with the current month
                control.SetValue(DatePicker.SelectedDateProperty, DateTime.Now);
                control.SetValue(DatePicker.SelectedDateProperty, null);
            }
        }
    }

    public static void SetShowNullWhenDateTimeMinValue(UIElement element, bool value)
    {
        element.SetValue(ShowNullWhenDateTimeMinValueProperty, value);
    }

    public static bool GetShowNullWhenDateTimeMinValue(UIElement element)
    {
        return (bool)element.GetValue(ShowNullWhenDateTimeMinValueProperty);
    }

    #endregion
}