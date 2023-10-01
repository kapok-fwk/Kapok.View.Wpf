using System.Globalization;
using System.Windows.Data;
using AvalonDock.Layout;

namespace Kapok.View.Wpf;

public class ActiveDocumentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IPage)
            return value;

        if (value is LayoutDocument layoutDocument)
        {
            if (layoutDocument.Content is IPage)
                return layoutDocument.Content;
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IPage)
            return value;

        if (value is LayoutDocument layoutDocument)
        {
            if (layoutDocument.Content is IPage)
                return layoutDocument.Content;
        }

        return Binding.DoNothing;
    }
}