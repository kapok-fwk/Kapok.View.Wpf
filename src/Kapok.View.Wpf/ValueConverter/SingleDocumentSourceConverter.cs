using System.Globalization;
using System.Windows.Data;

namespace Kapok.View.Wpf;

[ValueConversion(typeof(IDataPage), typeof(IEnumerable<IDataPage>))]
public class SingleDataPageAsDocumentSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new[] {value as IDataPage};
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}