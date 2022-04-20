using System.Windows.Controls;
using System.Windows.Data;

namespace Kapok.View.Wpf;

public class DataGridImageColumn : DataGridBoundTemplateColumn
{
    // TODO: this image column needs somehow an way to edit it, e.g. when the column is not sed to IsReadOnly=true, with an context menu the user should be able to select an image from e.g. an local path or from the application image library

    public DataGridImageColumn()
    {
        CellTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            var image = new Image();
            image.SetBinding(Image.SourceProperty, new Binding
            {
                Converter = new BinaryImageConverter(),
                ElementName = (Binding as Binding)?.ElementName,
                FallbackValue = (Binding as Binding)?.FallbackValue,
                TargetNullValue = (Binding as Binding)?.TargetNullValue,
                Mode = (Binding as Binding)?.Mode ?? BindingMode.Default,
                UpdateSourceTrigger = (Binding as Binding)?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default
            });

            return image;
        });

        IsReadOnly = true;
    }
}