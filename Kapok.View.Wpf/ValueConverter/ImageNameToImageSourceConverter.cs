using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kapok.View.Wpf;

public class ImageNameToImageSourceConverter : IValueConverter, IMultiValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType != typeof(ImageSource))
            throw new InvalidOperationException("Target type must be System.Windows.Media.ImageSource.");

        if (value == null)
            return null;

        if (parameter == null)
            return null;

        string resourceString;

        switch (parameter.ToString())
        {
            case "Large":
                resourceString = ImageManager.GetImageResource(value.ToString(), ImageManager.ImageSize.Large);
                break;
            case "Small":
                resourceString = ImageManager.GetImageResource(value.ToString(), ImageManager.ImageSize.Small);
                break;
            default:
                Debug.WriteLine("ImageNameToImageSourceConverter: Parameter was not specified, no image source was created");
                return null;
        }

        return CreateImageSource(resourceString);
    }

    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType != typeof(ImageSource))
            throw new InvalidOperationException("Target type must be System.Windows.Media.ImageSource.");

        if (values == null || values[0] == null)
            return null;

        if (parameter == null)
            return null;

        string resourceString;

        if (values.Length > 1 && values[1] != null)
        {
            if (values[1].GetType() == typeof(bool) && (bool)values[1] == false ||
                values[1].GetType() == typeof(bool?) && (bool?)values[1] == false)
            {
                if (parameter.ToString() == "Large")
                    return null;
            }
        }

        switch (parameter.ToString())
        {
            case "Large":
                resourceString = ImageManager.GetImageResource(values[0].ToString(), ImageManager.ImageSize.Large);
                break;
            case "Small":
                resourceString = ImageManager.GetImageResource(values[0].ToString(), ImageManager.ImageSize.Small);
                break;
            default:
                Debug.WriteLine("ImageNameToImageSourceConverter: Parameter was not specified, no image source was created");
                return null;
        }

        return CreateImageSource(resourceString);
    }

    private ImageSource CreateImageSource(string resourceString)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(resourceString);
        image.EndInit();

        return image;
    }

    #region ConvertBack methods (not supported)

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}