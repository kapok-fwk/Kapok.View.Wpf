using System.Collections;
using System.Windows;

namespace Kapok.View.Wpf;

public class InteractiveMenu
{
    public static readonly DependencyProperty SelectedItemsBindingProperty =
        DependencyProperty.RegisterAttached("SelectedItemsBinding",
            typeof(IList),
            typeof(InteractiveMenu),
            new PropertyMetadata(default(IList), null, null));

    public static object GetSelectedItemsBinding(DependencyObject target)
    {
        return target.GetValue(SelectedItemsBindingProperty);
    }

    public static void SetSelectedItemsBinding(DependencyObject target, object value)
    {
        target.SetValue(SelectedItemsBindingProperty, value);
    }
}