using System.Globalization;
using System.Windows.Data;

namespace Kapok.View.Wpf;

/// <summary>
/// A converter in the UI for parsing string values to a numeric target type.
/// </summary>
[ValueConversion(typeof(byte), typeof(string))]
[ValueConversion(typeof(byte?), typeof(string))]
[ValueConversion(typeof(sbyte), typeof(string))]
[ValueConversion(typeof(sbyte?), typeof(string))]
[ValueConversion(typeof(short), typeof(string))]
[ValueConversion(typeof(short?), typeof(string))]
[ValueConversion(typeof(ushort), typeof(string))]
[ValueConversion(typeof(ushort?), typeof(string))]
[ValueConversion(typeof(int), typeof(string))]
[ValueConversion(typeof(int?), typeof(string))]
[ValueConversion(typeof(uint), typeof(string))]
[ValueConversion(typeof(uint?), typeof(string))]
[ValueConversion(typeof(long), typeof(string))]
[ValueConversion(typeof(long?), typeof(string))]
[ValueConversion(typeof(ulong), typeof(string))]
[ValueConversion(typeof(ulong?), typeof(string))]
[ValueConversion(typeof(float), typeof(string))]
[ValueConversion(typeof(float?), typeof(string))]
[ValueConversion(typeof(double), typeof(string))]
[ValueConversion(typeof(double?), typeof(string))]
[ValueConversion(typeof(decimal), typeof(string))]
[ValueConversion(typeof(decimal?), typeof(string))]
public class EnterNumericValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType != typeof(string))
            return targetType.GetTypeDefault(); // not supported conversation --> just throw the default value back an optimistic behavior

        if (value == null)
            return null;

        if (typeof(IFormattable).IsAssignableFrom(value.GetType()))
        {
            // returning the value with its default format
            return ((IFormattable)value).ToString(parameter?.ToString(), culture);
        }

        return value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return targetType.GetTypeDefault();
        }

        if (value is string s)
        {
            if (s == string.Empty)
            {
                return targetType.GetTypeDefault();
            }

            if (targetType == typeof(byte))
            {
                return byte.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(byte?))
            {
                return (byte?)byte.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(sbyte))
            {
                return sbyte.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(short))
            {
                return short.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(short?))
            {
                return (short?)short.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(ushort))
            {
                return ushort.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(ushort?))
            {
                return (ushort?)ushort.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(int))
            {
                return int.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(int?))
            {
                return (int?)int.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(uint))
            {
                return uint.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(uint?))
            {
                return (uint?)uint.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(long))
            {
                return long.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(long?))
            {
                return (long?)long.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(ulong))
            {
                return ulong.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(ulong?))
            {
                return (ulong?)ulong.Parse(s, NumberStyles.Integer, culture);
            }

            if (targetType == typeof(float))
            {
                return float.Parse(s, NumberStyles.Float, culture);
            }

            if (targetType == typeof(float?))
            {
                return (float?)float.Parse(s, NumberStyles.Float, culture);
            }

            if (targetType == typeof(double))
            {
                return double.Parse(s, NumberStyles.Float, culture);
            }

            if (targetType == typeof(double?))
            {
                return (double?)double.Parse(s, NumberStyles.Float, culture);
            }

            if (targetType == typeof(decimal))
            {
                return decimal.Parse(s, NumberStyles.Float, culture);
            }

            if (targetType == typeof(decimal?))
            {
                return (decimal?)decimal.Parse(s, NumberStyles.Float, culture);
            }
        }

        return null; // unexpected input type
    }
}