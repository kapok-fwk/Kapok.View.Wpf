using System.Globalization;
using System.Windows.Data;

namespace Kapok.View.Wpf;

public class BuildListControlEntryMouseDoubleClickCommand : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var type = value.GetType();

        var detailListPageType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDetailListPage<,>));
        if (detailListPageType != null)
        {
            var entryType = detailListPageType.GenericTypeArguments[0];
            var subEntryType = detailListPageType.GenericTypeArguments[1];

            var buildMethodBase = typeof(ListControlEntryMouseDoubleClickCommand).GetMethodExt(
                nameof(ListControlEntryMouseDoubleClickCommand.BuildNew), typeof(IDetailListPage<,>));

            var buildMethod = buildMethodBase.MakeGenericMethod(entryType, subEntryType);

            return buildMethod.Invoke(null, new[] { value });
        }

        var listPageType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IListPage<>));
        if (listPageType != null)
        {
            var entryType = listPageType.GenericTypeArguments[0];

            var buildMethodBase = typeof(ListControlEntryMouseDoubleClickCommand).GetMethodExt(
                nameof(ListControlEntryMouseDoubleClickCommand.BuildNew), typeof(IListPage<>));

            var buildMethod = buildMethodBase.MakeGenericMethod(entryType);

            return buildMethod.Invoke(null, new[] {value});
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}