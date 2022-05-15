using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Kapok.View.Wpf;

public class DataGridInfoImageColumn : DataGridBoundTemplateColumn
{
    public DataGridInfoImageColumn()
    {
        CellTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var itemsPanelTemplate = new ItemsPanelTemplate(stackPanelFactory);

            var itemTemplateFactory = new FrameworkElementFactory(typeof(Image));
            itemTemplateFactory.SetBinding(Image.SourceProperty, new Binding());
            itemTemplateFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 3, 0));

            var itemTemplate = new DataTemplate();
            itemTemplate.VisualTree = itemTemplateFactory;

            var itemsControl = new ItemsControl();
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
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
            itemsControl.ItemsPanel = itemsPanelTemplate;
            itemsControl.ItemTemplate = itemTemplate;

            return itemsControl;
        });
        IsReadOnly = true;
    }
}