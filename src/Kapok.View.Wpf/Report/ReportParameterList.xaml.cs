using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Kapok.Report.Model;

namespace Kapok.View.Wpf.Report;

/// <summary>
/// Interaktionslogik für ReportParameterList.xaml
/// </summary>
public partial class ReportParameterList : UserControl
{
    public ReportParameterList()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ParameterListProperty = 
        DependencyProperty.Register("ParameterList", typeof(Collection<ReportParameterViewModel>),
            typeof(ReportParameterList));

    public Collection<ReportParameterViewModel> ParameterList
    {
        get => (Collection<ReportParameterViewModel>) GetValue(ParameterListProperty);
        set => SetValue(ParameterListProperty, value);
    }
}

public class ReportParameterTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (!(container is FrameworkElement element) || !(item is ReportParameterViewModel vm))
            return null;

        if (vm.ProposalValues != null && vm.ProposalValues.Count > 0)
        {
            if (vm.ReportParameter.AllowedParameterValues == AllowedParameterValueType.All)
            {
                return (DataTemplate)element.FindResource("ComboBoxEditableTemplate");
            }

            return (DataTemplate)element.FindResource("ComboBoxTemplate");
        }

        if (vm.ReportParameter.DataType == typeof(bool))
            return (DataTemplate)element.FindResource("CheckBoxTemplate");
        if (vm.ReportParameter.DataType == typeof(bool?))
            return (DataTemplate)element.FindResource("BooleanNullableTemplate");
        if (vm.ReportParameter.DataType == typeof(DateTime))
        {
            // TODO: we just use here a date picker, but it might be that the user has to set the time as well which is not possible today.
            return (DataTemplate) element.FindResource("DatePickerTemplate");
        }
        if (vm.ReportParameter.DataType == typeof(string) ||

            // special types
            vm.ReportParameter.DataType == typeof(Guid) ||
            vm.ReportParameter.DataType == typeof(TimeSpan) ||

            // int data types
            vm.ReportParameter.DataType == typeof(byte) ||
            vm.ReportParameter.DataType == typeof(short) ||
            vm.ReportParameter.DataType == typeof(int) ||
            vm.ReportParameter.DataType == typeof(long) ||

            // float data types
            vm.ReportParameter.DataType == typeof(float) ||
            vm.ReportParameter.DataType == typeof(double) ||
            vm.ReportParameter.DataType == typeof(decimal)
           )
        {
            return (DataTemplate)element.FindResource("TextBoxTemplate");
        }

        Debug.WriteLine($"ReportParameterList UserControl: Could not find template for type {vm.ReportParameter.DataType}. Use TextBoxTemplate instead.");

        return (DataTemplate)element.FindResource("TextBoxTemplate");
    }
}