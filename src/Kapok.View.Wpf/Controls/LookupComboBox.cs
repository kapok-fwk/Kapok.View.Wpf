using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Kapok.Entity;

namespace Kapok.View.Wpf;

public class LookupComboBox : CustomComboBox
{
    public static readonly DependencyProperty AllowSetToDefaultProperty = DependencyProperty.Register(
        nameof(AllowSetToDefault), typeof(bool), typeof(LookupComboBox),
        new PropertyMetadata(true, null, null), null);

    protected DataGrid DropDownDataGrid { get; private set; }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var scrollViewer = GetTemplateChild("DropDownScrollViewer") as ScrollViewer;
        if (scrollViewer == null)
            return;
        if (scrollViewer.Parent is Decorator parent)
        {
            DropDownDataGrid = new DataGrid
            {
                Name = "DropDownDataGrid",
                IsReadOnly = true,
                CanUserResizeRows = false,
                CanUserSortColumns = false,
                RowHeaderWidth = 0,
                SelectionMode = DataGridSelectionMode.Single
            };

            var snapsToDevicePixelsBinding = GetBindingExpression(SnapsToDevicePixelsProperty);
            if (snapsToDevicePixelsBinding != null)
            {
                DropDownDataGrid.SetBinding(SnapsToDevicePixelsProperty, snapsToDevicePixelsBinding.ParentBinding);
            }
            else if (GetValue(SnapsToDevicePixelsProperty) != null)
            {
                DropDownDataGrid.SetValue(SnapsToDevicePixelsProperty, GetValue(SnapsToDevicePixelsProperty));
            }

            var itemsSourceBinding = GetBindingExpression(ItemsSourceProperty);
            if (itemsSourceBinding != null)
            {
                DropDownDataGrid.SetBinding(ItemsSourceProperty, itemsSourceBinding.ParentBinding);
            }
            else if (GetValue(ItemsSourceProperty) != null)
            {
                DropDownDataGrid.SetValue(ItemsSourceProperty, GetValue(ItemsSourceProperty));
            }

            DropDownDataGrid.SetValue(SelectedItemProperty,
                new Binding("SelectedItem")
                    {RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)});
            DropDownDataGrid.AutoGeneratingColumn += DropDownDataGrid_AutoGeneratingColumn;
            DropDownDataGrid.MouseLeftButtonUp += DropDownDataGrid_MouseLeftButtonUp;

            DropDownDataGrid.SetBinding(FocusManager.FocusedElementProperty,
                new Binding() {RelativeSource = RelativeSource.Self});

            parent.Child = DropDownDataGrid;
        }
    }

    private void DropDownDataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsDropDownOpen)
            return;

        DependencyObject dep = (DependencyObject)e.OriginalSource;
        while ((dep != null) && !(dep is DataGridCell))
        {
            dep = VisualTreeHelper.GetParent(dep);
        }
        if (dep == null) return;

        IsDropDownOpen = false;
    }

    private void DropDownDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var lookupAttr = GetPropertyAttribute<LookupColumnAttribute>(e.PropertyDescriptor);
        if (lookupAttr == null || !lookupAttr.Show)
        {
            e.Cancel = true;
            return;
        }
            
        if (GetPropertyAttribute<BinaryImageAttribute>(e.PropertyDescriptor) != null)
        {
            // TODO: this is duplicated with the code in CustomDataGrid in method CustomDataGrid_AutoGeneratingColumn(o,e)
            var newColumn = new DataGridImageColumn();
            newColumn.Binding = new Binding(e.PropertyName);
            newColumn.Header = e.PropertyName;
            e.Column = newColumn;
        }

        var displayAttribute = GetPropertyAttribute<DisplayAttribute>(e.PropertyDescriptor);
        if (displayAttribute != null)
        {
            System.Resources.ResourceManager resourceManager = (System.Resources.ResourceManager)displayAttribute.ResourceType?
                .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod
                .Invoke(null, null);

            if (!string.IsNullOrEmpty(displayAttribute.Name))
            {
                var name = resourceManager?.GetString(displayAttribute.Name) ?? displayAttribute.Name;
                e.Column.Header = name;
            }
        }

        e.Column.IsReadOnly = true;
    }

    private static T GetPropertyAttribute<T>(object descriptor)
        where T : Attribute
    {
        if (descriptor is PropertyDescriptor pd)
        {               
            return pd.Attributes[typeof(T)] as T;
        }

        PropertyInfo pi = descriptor as PropertyInfo;
        if (pi != null)
        {
            Object[] attrs = pi.GetCustomAttributes(typeof(T), true);
            foreach (var att in attrs)
            {
                return att as T;
            }
        }

        return null;
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        base.OnDropDownOpened(e);

        if (DropDownDataGrid?.SelectedItem != null)
        {
            // scroll to the position of the current selected item
            DropDownDataGrid.ScrollIntoView(DropDownDataGrid.SelectedItem);
        }
    }

    /// <summary>
    /// If set to true it is possible to set the ComboBox to its default value (= empty/null)
    /// e.g. by pressing the 'Delete' key when the ComboBox is not editable.
    /// </summary>
    public bool AllowSetToDefault
    {
        get => (bool) GetValue(AllowSetToDefaultProperty);
        set => SetValue(AllowSetToDefaultProperty, value);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (!IsEditable && e.Key == Key.Delete && AllowSetToDefault)
        {
            SelectedValue = default;
        }

        base.OnKeyUp(e);
    }
}