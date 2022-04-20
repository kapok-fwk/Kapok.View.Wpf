// ReSharper disable once CheckNamespace
namespace System.Windows;

// ReSharper disable once InconsistentNaming
public static class UIElementExtension
{
    public static T? FindFromPoint<T>(this UIElement reference, Point point)
        where T : DependencyObject
    {
        DependencyObject? element = reference.InputHitTest(point) as DependencyObject;

        if (element == null) return null;
        if (element is T t) return t;
        return element.FindVisualParent<T>();
    }
}