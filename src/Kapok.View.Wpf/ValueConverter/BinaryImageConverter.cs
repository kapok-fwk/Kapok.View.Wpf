using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Kapok.View.Wpf;
// source: http://cromwellhaus.com/2007/07/binding-to-the-byte-of-an-image-in-wpf/

[ValueConversion(typeof(byte[]), typeof(BitmapImage))]
public class BinaryImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value != null && value is byte[] bytes)
        {
            MemoryStream stream = new MemoryStream( bytes );

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            return image;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}