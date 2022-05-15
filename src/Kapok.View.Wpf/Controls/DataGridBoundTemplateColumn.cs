using System.Windows.Data;

namespace System.Windows.Controls;

public class DataGridBoundTemplateColumn : DataGridBoundColumn
{
    /// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellTemplate" /> dependency property.</summary>
    /// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellTemplate" /> dependency property.</returns>
    public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(
        nameof (CellTemplate), typeof (DataTemplate), typeof (DataGridBoundTemplateColumn),
        new FrameworkPropertyMetadata((object?)null,
            null/*new PropertyChangedCallback(DataGridColumn.NotifyPropertyChangeForRefreshContent)*/));
    /// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</summary>
    /// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</returns>
    public static readonly DependencyProperty CellTemplateSelectorProperty = DependencyProperty.Register(
        nameof (CellTemplateSelector), typeof (DataTemplateSelector), typeof (DataGridBoundTemplateColumn),
        new FrameworkPropertyMetadata((object?)null,
            null/*new PropertyChangedCallback(DataGridColumn.NotifyPropertyChangeForRefreshContent)*/));
    /// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate" /> dependency property.</summary>
    /// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate" /> dependency property.</returns>
    public static readonly DependencyProperty CellEditingTemplateProperty = DependencyProperty.Register(
        nameof (CellEditingTemplate), typeof (DataTemplate), typeof (DataGridBoundTemplateColumn),
        new FrameworkPropertyMetadata((object?)null,
            null/*new PropertyChangedCallback(DataGridColumn.NotifyPropertyChangeForRefreshContent)*/));
    /// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</summary>
    /// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</returns>
    public static readonly DependencyProperty CellEditingTemplateSelectorProperty = DependencyProperty.Register(
        nameof (CellEditingTemplateSelector), typeof (DataTemplateSelector), typeof (DataGridBoundTemplateColumn),
        new FrameworkPropertyMetadata((object?) null,
            null/*new PropertyChangedCallback(DataGridColumn.NotifyPropertyChangeForRefreshContent)*/));

    static DataGridBoundTemplateColumn()
    {
        //DataGridColumn.CanUserSortProperty.OverrideMetadata(typeof (DataGridBoundTemplateColumn), (PropertyMetadata) new FrameworkPropertyMetadata((PropertyChangedCallback) null, new CoerceValueCallback(DataGridBoundTemplateColumn.OnCoerceTemplateColumnCanUserSort)));
        SortMemberPathProperty.OverrideMetadata(typeof (DataGridBoundTemplateColumn), new FrameworkPropertyMetadata(OnTemplateColumnSortMemberPathChanged));
    }

    private static void OnTemplateColumnSortMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(CanUserSortProperty);
    }

    /*private static object OnCoerceTemplateColumnCanUserSort(DependencyObject d, object baseValue)
    {
      if (string.IsNullOrEmpty(((DataGridColumn) d).SortMemberPath))
        return (object) false;
      return DataGridColumn.OnCoerceCanUserSort(d, baseValue);
    }*/

    /// <summary>Gets or sets the template to use to display the contents of a cell that is not in editing mode.</summary>
    /// <returns>The template to use to display the contents of a cell that is not in editing mode. The registered default is <see langword="null" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
    public DataTemplate CellTemplate
    {
        get => (DataTemplate) GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>Gets or sets the object that determines which template to use to display the contents of a cell that is not in editing mode. </summary>
    /// <returns>The object that determines which template to use. The registered default is <see langword="null" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
    public DataTemplateSelector CellTemplateSelector
    {
        get => (DataTemplateSelector) GetValue(CellTemplateSelectorProperty);
        set => SetValue(CellTemplateSelectorProperty, value);
    }

    /// <summary>Gets or sets the template to use to display the contents of a cell that is in editing mode.</summary>
    /// <returns>The template that is used to display the contents of a cell that is in editing mode. The registered default is <see langword="null" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
    public DataTemplate CellEditingTemplate
    {
        get => (DataTemplate) GetValue(CellEditingTemplateProperty);
        set => SetValue(CellEditingTemplateProperty, value);
    }

    /// <summary>Gets or sets the object that determines which template to use to display the contents of a cell that is in editing mode.</summary>
    /// <returns>The object that determines which template to use to display the contents of a cell that is in editing mode. The registered default is <see langword="null" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
    public DataTemplateSelector CellEditingTemplateSelector
    {
        get => (DataTemplateSelector) GetValue(CellEditingTemplateSelectorProperty);
        set => SetValue(CellEditingTemplateSelectorProperty, value);
    }

    private void ChooseCellTemplateAndSelector(bool isEditing, out DataTemplate template, out DataTemplateSelector templateSelector)
    {
        template = null;
        templateSelector = null;
        if (isEditing)
        {
            template = CellEditingTemplate;
            templateSelector = CellEditingTemplateSelector;
        }
        if (template != null || templateSelector != null)
            return;
        template = CellTemplate;
        templateSelector = CellTemplateSelector;
    }

    private FrameworkElement LoadTemplateContent(bool isEditing, object dataItem)
    {
        ChooseCellTemplateAndSelector(isEditing, out var template, out DataTemplateSelector templateSelector);
        if (template == null && templateSelector == null)
            return null;
        /*ContentPresenter contentPresenter = new ContentPresenter();
        BindingOperations.SetBinding((DependencyObject) contentPresenter, ContentPresenter.ContentProperty, Binding);
        contentPresenter.ContentTemplate = template;
        contentPresenter.ContentTemplateSelector = templateSelector;

        return contentPresenter;
        */
        var contentControl = new ContentControl { ContentTemplate = template, Content = dataItem };
        BindingOperations.SetBinding(contentControl, ContentControl.ContentProperty, Binding);
        return contentControl;
    }

    /// <summary>Gets an element defined by the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellTemplate" /> that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
    /// <param name="cell">The cell that will contain the generated element.</param>
    /// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
    /// <returns>A new, read-only element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        return LoadTemplateContent(false, dataItem);
    }

    /// <summary>Gets an element defined by the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate" /> that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
    /// <param name="cell">The cell that will contain the generated element.</param>
    /// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
    /// <returns>A new editing element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        return LoadTemplateContent(true, dataItem);
    }

    /// <summary>Gets or sets the binding object to use when getting or setting cell content for the clipboard.</summary>
    /// <returns>An object that represents the binding.</returns>
    public override BindingBase ClipboardContentBinding
    {
        get => base.ClipboardContentBinding ?? Binding;
        set => base.ClipboardContentBinding = value;
    }

    /*/// <summary>Refreshes the contents of a cell in the column in response to a template property value change.</summary>
    /// <param name="element">The cell to update.</param>
    /// <param name="propertyName">The name of the column property that has changed.</param>
    protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
    {
        DataGridCell dataGridCell = element as DataGridCell;
        if (dataGridCell != null)
        {
            bool isEditing = dataGridCell.IsEditing;
            if (!isEditing && (string.Compare(propertyName, "CellTemplate", StringComparison.Ordinal) == 0 ||
                               string.Compare(propertyName, "CellTemplateSelector", StringComparison.Ordinal) == 0) ||
                isEditing && (string.Compare(propertyName, "CellEditingTemplate", StringComparison.Ordinal) == 0 ||
                              string.Compare(propertyName, "CellEditingTemplateSelector", StringComparison.Ordinal) == 0))
            {
                dataGridCell.BuildVisualTree();
                return;
            }
        }
        base.RefreshCellContent(element, propertyName);
    }*/


    /*public DataTemplate CellTemplate { get; set; }
    public DataTemplate CellEditingTemplate { get; set; }

    protected override System.Windows.FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        return Generate(dataItem, CellEditingTemplate);
    }

    private FrameworkElement Generate(object dataItem, DataTemplate template)
    {
        var contentControl = new ContentControl { ContentTemplate = template, Content = dataItem };
        BindingOperations.SetBinding(contentControl, ContentControl.ContentProperty, Binding);
        return contentControl;
    }

    protected override System.Windows.FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        return Generate(dataItem, CellTemplate);
    }*/
}