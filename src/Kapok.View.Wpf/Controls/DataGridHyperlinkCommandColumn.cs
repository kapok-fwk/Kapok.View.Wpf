using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Kapok.View.Wpf;

public class DataGridHyperlinkCommandColumn : DataGridBoundTemplateColumn
{

    public DataGridHyperlinkCommandColumn()
    {
        CellTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            var innerTextBlock = new TextBlock();
            innerTextBlock.SetBinding(TextBlock.TextProperty, new Binding
            {
                StringFormat = Binding.StringFormat,
                Converter = (Binding as Binding)?.Converter,
                ConverterCulture =  (Binding as Binding)?.ConverterCulture,
                ConverterParameter = (Binding as Binding)?.ConverterParameter,
                ElementName = (Binding as Binding)?.ElementName,
                FallbackValue = (Binding as Binding)?.FallbackValue,
                TargetNullValue = (Binding as Binding)?.TargetNullValue,
                Mode = (Binding as Binding)?.Mode ?? BindingMode.Default,
                UpdateSourceTrigger = (Binding as Binding)?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default
            });

            var hyperlink = new Hyperlink();
            hyperlink.SetBinding(Hyperlink.CommandProperty, CommandBinding);
            hyperlink.SetBinding(Hyperlink.CommandParameterProperty, CommandParameterBinding);
            hyperlink.Inlines.Add(innerTextBlock);

            var textBlock = new TextBlock();
            textBlock.Inlines.Add(hyperlink);

            // TODO: this should be left when the content is not a number, but I have difficulties to pass this information from CustomDataGrid.AutoGeneratingColumnApplyControlImprovement(..)
            textBlock.TextAlignment = TextAlignment.Right;

            return textBlock;
        }, sealTemplate: false);
        IsReadOnly = true;
    }

    private BindingBase _commandBinding;
    private BindingBase _commandParameterBinding;

    public BindingBase CommandBinding
    {
        get => _commandBinding;
        set
        {
            if (_commandBinding == value)
                return;
            _commandBinding = value;
            NotifyPropertyChanged(nameof(CommandBinding));
        }
    }

    public BindingBase CommandParameterBinding
    {
        get => _commandParameterBinding;
        set
        {
            if (_commandParameterBinding == value)
                return;
            _commandParameterBinding = value;
            NotifyPropertyChanged(nameof(CommandParameterBinding));
        }
    }
}