using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Kapok.View.Wpf;

public class EnumValueViewModel : INotifyPropertyChanged
{
    public EnumValueViewModel(Enum? enumValue)
    {
        Value = enumValue;

        if (enumValue == null)
        {
            Name = "";
        }
        else
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false).SingleOrDefault() is DisplayAttribute
                displayAttribute)
            {
                System.Resources.ResourceManager resourceManager =
                    (System.Resources.ResourceManager) displayAttribute.ResourceType?
                        .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod
                        .Invoke(null, null);

                if (!string.IsNullOrEmpty(displayAttribute.Name))
                {
                    Name = resourceManager?.GetString(displayAttribute.Name) ?? displayAttribute.Name;
                }
                else
                {
                    Name = enumValue.ToString();
                }

                if (!string.IsNullOrEmpty(displayAttribute.Description))
                {
                    Description = resourceManager?.GetString(displayAttribute.Description) ??
                                  displayAttribute.Description;
                }
            }
            else if (fieldInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).SingleOrDefault() is
                     DisplayNameAttribute displayNameAttribute)
            {
                Name = displayNameAttribute.DisplayName;
            }
            else if (fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() is
                     DescriptionAttribute descriptionAttribute)
            {
                Name = descriptionAttribute.Description;
            }
            else
            {
                Name = enumValue.ToString();
            }
        }
    }

    /// <summary>
    /// Value of the enum value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Display name of the enum value.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Additional description of the enum value.
    /// </summary>
    public string Description { get; }

    #region INotifyPropertyChanged

    /// <summary>
    /// this is implemented to avoid a memory leak when binding to the properties
    /// </summary>
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add
        {
        }
        remove
        {
        }
    }

    #endregion

    public override string ToString()
    {
        return Name;
    }
}

[ValueConversion(typeof(Enum), typeof(IEnumerable<EnumValueViewModel>))]
public class EnumToCollectionConverter : MarkupExtension, IValueConverter
{
    private readonly List<EnumValueViewModel>? _cachedEnumValueList;

    public EnumToCollectionConverter()
    {
        // No caching, use a generic version which can support whatever enum type (does not support nullable values!)
    }

    public EnumToCollectionConverter(Type baseType)
    {
        bool withNullable = false;

        if (baseType.IsGenericType &&
            baseType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            withNullable = true;
            baseType = baseType.GenericTypeArguments[0];
        }

        _cachedEnumValueList = GetListFromType(baseType, withNullable);
    }

    public static List<EnumValueViewModel> GetListFromType(Type type, bool withNullable)
    {
        if (!type.IsEnum)
            throw new ArgumentException($"The parameter {nameof(type)} must be an enum type.");

        var list = Enum.GetValues(type)
            .Cast<Enum>()
            .Select(e => new EnumValueViewModel(e))
            .ToList();

        if (withNullable)
        {
            list.Insert(0, new EnumValueViewModel(null));
        }

        return list;
    }

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (_cachedEnumValueList != null)
        {
            return _cachedEnumValueList;
        }
        else
        {
            // enum detection
            if (value == null)
                return null;

            Type type;
            if (value is Enum)
                type = value.GetType();
            else if (value is Type)
                type = (Type) value;
            else
                return null;

            // return list with view model
            return GetListFromType(type, false);
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}