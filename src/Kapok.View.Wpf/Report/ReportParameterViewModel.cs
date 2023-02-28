using Kapok.Entity;
using Kapok.Report.Model;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Kapok.View.Wpf.Report;

public class ReportParameterViewModel : EditableEntityBase
{
    public ReportParameterViewModel(ReportParameter reportParameter)
    {
        ReportParameter = reportParameter;

        if (reportParameter.DefaultValueList != null)
        {
            var type = reportParameter.DefaultValueList.GetType();

            if (type == typeof(FixedValueList))
            {
                FixedValueList valueList = (FixedValueList) reportParameter.DefaultValueList;
                ProposalValues = new List<object>(valueList.Values);
            }
            else
            {
                throw new NotSupportedException($"The report parameter has a reference to a default value list from the type {type} which is not supported in this class.");
            }
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

    public List<object> ProposalValues { get; }

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