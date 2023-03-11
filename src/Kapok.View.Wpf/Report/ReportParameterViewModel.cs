using Kapok.Entity;
using Kapok.Report.Model;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Kapok.View.Wpf.Report;

public class ReportParameterViewModel : EditableEntityBase
{
    public ReportParameterViewModel(ReportParameter reportParameter)
    {
        ReportParameter = reportParameter;

        if (reportParameter.DefaultIterativeValues != null)
        {
            ProposalValues = new List<object>(reportParameter.DefaultIterativeValues);
        }
    }

    public ReportParameter ReportParameter { get; }

    public bool HasIterativeValues { get; set; }

    private object? _value;
    public object? Value
    {
        get => _value;
        set
        {
            if (SetValidateProperty(ref _value, value))
            {
                if (_value == null)
                {
                    _value = ReportParameter.DataType.GetTypeDefault();
                }
                else if (ReportParameter.DataType == typeof(string))
                {
                    return;
                }
                else if (_value is IConvertible convertible)
                {
                    _value = convertible.ToType(ReportParameter.DataType, CultureInfo.CurrentUICulture);
                }
            }
        }
    }

    public List<object>? ProposalValues { get; }

    protected override void ValidateProperty(object? value, [CallerMemberName] string? propertyName = null)
    {
        base.ValidateProperty(value, propertyName);

        if (propertyName == nameof(Value))
        {
            if (Equals(value, null))
            {
                return;
            }
            else if (ReportParameter.DataType == typeof(string))
            {
                return;
            }
            else if (_value is IConvertible convertible)
            {
                //convertible.ToType(ReportParameter.DataType, CultureInfo.CurrentUICulture);
            }
        }
    }
}