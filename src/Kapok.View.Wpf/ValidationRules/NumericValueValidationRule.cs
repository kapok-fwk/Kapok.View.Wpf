using System.Globalization;
using System.Windows.Controls;
using Res = Kapok.View.Wpf.Resources.ValidationRules.NumericValueValidationRule;

namespace Kapok.View.Wpf;

/// <summary>
/// A validation rule should be used together with <c>EnterNumericValueConverter</c>
/// </summary>
public class NumericValueValidationRule : ValidationRule
{
    protected Type TargetType { get; }

    public NumericValueValidationRule(Type targetType)
    {
        TargetType = targetType;
    }

    public override ValidationResult Validate(object? value, CultureInfo culture)
    {
        if (value == null)
            return new ValidationResult(true, null);

        if (value is string s)
        {
            if (s == string.Empty)
            {
                return new ValidationResult(true, null);
            }

            var targetType = TargetType;

            if (Nullable.GetUnderlyingType(targetType) != null)
                targetType = Nullable.GetUnderlyingType(targetType);

            if (targetType == typeof(byte))
            {
                if (!byte.TryParse(s, NumberStyles.Integer, culture, out byte _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(sbyte))
            {
                if (!sbyte.TryParse(s, NumberStyles.Integer, culture, out sbyte _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(short))
            {
                if (!short.TryParse(s, NumberStyles.Integer, culture, out short _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(ushort))
            {
                if (!ushort.TryParse(s, NumberStyles.Integer, culture, out ushort _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(int))
            {
                if (!int.TryParse(s, NumberStyles.Integer, culture, out int _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(uint))
            {
                if (!uint.TryParse(s, NumberStyles.Integer, culture, out uint _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(long))
            {
                if (!long.TryParse(s, NumberStyles.Integer, culture, out long _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(ulong))
            {
                if (!ulong.TryParse(s, NumberStyles.Integer, culture, out ulong _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(float))
            {
                if (!float.TryParse(s, NumberStyles.Float, culture, out float _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(double))
            {
                if (!double.TryParse(s, NumberStyles.Float, culture, out double _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }

            if (targetType == typeof(decimal))
            {
                if (!decimal.TryParse(s, NumberStyles.Float, culture, out decimal _))
                {
                    return new ValidationResult(false, string.Format(Res.InvalidInput, value));
                }

                return new ValidationResult(true, null);
            }
        }

        return new ValidationResult(false, string.Format(Res.UnexpectedType, value.GetType().FullName));
    }
}