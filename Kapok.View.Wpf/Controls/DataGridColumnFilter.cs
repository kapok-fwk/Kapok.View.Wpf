using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Kapok.View.Wpf;

public class DataGridColumnFilter : Control
{
    static DataGridColumnFilter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnFilter), new FrameworkPropertyMetadata(typeof(DataGridColumnFilter)));
    }

    #region Overrides

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (!IsControlInitialized &&
            e.Property == DataGridProperty &&
            DataGridColumnHeader?.Column != null && (DataGrid as CustomDataGrid)?.Filter != null )
        {
            // TODO: this initialization here is an ugly solution.

            var itemSourceElementType = GetItemSourceElementType();

            if (itemSourceElementType == null)
                throw new NotSupportedException("Could not determine type from ItemSource of DataGrid.");

            ColumnFilter = new DataGridColumnFilterViewModel(
                // ReSharper disable once AssignNullToNotNullAttribute
                dataGrid: DataGrid as CustomDataGrid,
                elementType: itemSourceElementType,
                propertyPath: GetPropertyBindingPath(DataGridColumnHeader?.Column));

            IsControlInitialized = true;
        }

        base.OnPropertyChanged(e);
    }

    #endregion

    #region Properties

    public DataGridColumnFilterViewModel ColumnFilter
    {
        get => (DataGridColumnFilterViewModel)GetValue(ColumnFilterProperty);
        set => SetValue(ColumnFilterProperty, value);
    }

    public static readonly DependencyProperty ColumnFilterProperty =
        DependencyProperty.Register(nameof(ColumnFilter), typeof(DataGridColumnFilterViewModel), typeof(DataGridColumnFilter));

    public DataGridColumnHeader DataGridColumnHeader
    {
        get => (DataGridColumnHeader)GetValue(DataGridColumnHeaderProperty);
        set => SetValue(DataGridColumnHeaderProperty, value);
    }

    public static readonly DependencyProperty DataGridColumnHeaderProperty =
        DependencyProperty.Register(nameof(DataGridColumnHeader), typeof(DataGridColumnHeader), typeof(DataGridColumnFilter));

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    public static readonly DependencyProperty DataGridProperty =
        DependencyProperty.Register(nameof(DataGrid), typeof(DataGrid), typeof(DataGridColumnFilter));

    public bool IsControlInitialized
    {
        get => (bool)GetValue(IsControlInitializedProperty);
        set => SetValue(IsControlInitializedProperty, value);
    }
    public static readonly DependencyProperty IsControlInitializedProperty =
        DependencyProperty.Register(nameof(IsControlInitialized), typeof(bool), typeof(DataGridColumnFilter),
            new PropertyMetadata(false, IsControlInitializedChanged, null));

    private static void IsControlInitializedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion

    private static Type GetElementType(IEnumerable source)
    {
        var enumerableType = source.GetType();
        if (enumerableType.IsArray)
        {
            return enumerableType.GetElementType();
        }
        if (enumerableType.IsGenericType)
        {
            return enumerableType.GetGenericArguments().First();
        }
        return null;
    }

    private Type GetItemSourceElementType()
    {
        var dataGridItemsSource = DataGrid.ItemsSource;

        IEnumerable e;

        if (dataGridItemsSource is ListCollectionView lcv)
        {
            e = lcv.SourceCollection;
        }
        else
        {
            e = dataGridItemsSource;
        }

        if (e != null)
        {
            return GetElementType(e);
        }

        return null;
    }

    private static string GetPropertyBindingPath(DataGridColumn column)
    {
        string path = string.Empty;

        if (column is DataGridBoundColumn bc)
        {
            path = (bc.Binding as Binding)?.Path.Path;
        }
        else if (column is DataGridLookupComboBoxColumn lookupComboBoxColumn)
        {
            path = null;

            if (lookupComboBoxColumn.SelectedValueBinding is Binding binding)
            {
                path = binding.Path.Path;
            }
        }
        else if (column is DataGridTreeTextColumn ttc)
        {
            if (ttc.Binding is Binding binding)
            {
                path = binding.Path.Path;
            }
        }
        else if (column is DataGridTemplateColumn tc)
        {
            object templateContent = tc.CellTemplate.LoadContent();

            if (templateContent != null && templateContent is TextBlock)
            {
                TextBlock block = templateContent as TextBlock;

                BindingExpression binding = block.GetBindingExpression(TextBlock.TextProperty);

                path = binding?.ParentBinding.Path.Path;
            }
        }
        else if (column is DataGridComboBoxColumn comboColumn)
        {
            path = null;

            if (comboColumn.SelectedValueBinding is Binding valueBinding)
            {
                path = valueBinding.Path.Path;
            }
            else if (comboColumn.SelectedItemBinding is Binding itemBinding)
            {
                path = itemBinding.Path.Path;

                if (DataGridComboBoxExtensions.GetIsTextFilter(comboColumn))
                {
                    if (!string.IsNullOrEmpty(comboColumn.DisplayMemberPath))
                    {
                        if (string.IsNullOrEmpty(path))
                            path = comboColumn.DisplayMemberPath;
                        else
                            path += "." + comboColumn.DisplayMemberPath;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(comboColumn.SelectedValuePath))
                    {
                        if (string.IsNullOrEmpty(path))
                            path = comboColumn.SelectedValuePath;
                        else
                            path += "." + comboColumn.SelectedValuePath;
                    }
                }
            }
        }

        return path;
    }
}