using Kapok.Entity;
using Kapok.Report.Model;

namespace Kapok.View.Wpf.Report;

public class ReportParameterViewModel : BindableObjectBase
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
        set => SetProperty(ref _value, value);
    }

    public List<object> ProposalValues { get; }
}