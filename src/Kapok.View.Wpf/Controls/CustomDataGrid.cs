using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Kapok.Core;
using Kapok.Entity;

namespace Kapok.View.Wpf;

// TODO: this class name is ugly
/// <summary>
/// Extends the default DataGrid with additional functionalities:
///  - add an optional header filter
///  - adds bindable 'SelectedItems' property
///  - adds bindable 'ColumnsSource' property
///  - improve data interaction so the table feels like an excel sheet
///  - implement copy/past from Excel or CSV data
/// </summary>
public class CustomDataGrid : DataGrid
{
    static CustomDataGrid()
    {
        CommandManager.RegisterClassCommandBinding(
            typeof(CustomDataGrid),
            new CommandBinding(ApplicationCommands.Paste,
                OnExecutedPaste,
                OnCanExecutePaste));

        EventManager.RegisterClassHandler(
            typeof(CustomDataGrid),
            PreviewKeyDownEvent,
            new RoutedEventHandler(OnPreviewKeyDown)
        );
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        try
        {
            base.OnSelectionChanged(e);
        }
        catch (ArgumentNullException exception)
        {
            if (exception.Message == "Value cannot be null. (Parameter 'item')")
            {
                // do nothing; this is a known bug and ignoring the exception makes it work
                //Debugger.Break();
                return;
            }

            throw;
        }

        OnSelectionChanged_MoveToSelectedEntry(e);
        OnSelectionChanged_UpdateSelectedItemsProperty();
    }

    protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
    {
        base.OnSelectedCellsChanged(e);

        OnSelectedCellsChanged_DataSourceInteraction();
        OnSelectedCellsChanged_ExcelNavigation(e);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        OnMouseLeftButtonDown_DragDropRow(e);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        OnMouseLeftButtonUp_DragDropRow(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        OnMouseMove_DragDropRow(e);
    }

    #region Improve UI behaviour

    private void OnSelectionChanged_MoveToSelectedEntry(SelectionChangedEventArgs e)
    {
        // make sure that the current entry is visible
        if (e.AddedItems.Count == 1)
            ScrollIntoView(e.AddedItems[0]);
    }

    #endregion

    #region Bindable multiselect 'SelectedItems'
    // source: https://stackoverflow.com/questions/9880589/bind-to-selecteditems-from-datagrid-or-listbox-in-mvvm

    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(CustomDataGrid), new PropertyMetadata(default(IList), OnSelectedItemsPropertyChanged));

    public new IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => throw new Exception("This property is read-only. To bind to it you must use 'Mode=OneWayToSource'.");
    }

    private static bool _skipOnSelectedItemsPropertyChanged;

    private void OnSelectionChanged_UpdateSelectedItemsProperty()
    {
        _skipOnSelectedItemsPropertyChanged = true;
        SetValue(SelectedItemsProperty, base.SelectedItems);
        _skipOnSelectedItemsPropertyChanged = false;
    }

    private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (_skipOnSelectedItemsPropertyChanged) return;

        ((CustomDataGrid)d).OnSelectedItemsChanged((IList)e.OldValue, (IList)e.NewValue);
    }

    protected virtual void OnSelectedItemsChanged(IList oldSelectedItems, IList newSelectedItems)
    {
        SelectAll();
    }

    #endregion

    #region Bindable columns 'ColumnsSource'
    //source: https://stackoverflow.com/questions/320089/how-do-i-bind-a-wpf-datagrid-to-a-variable-number-of-columns

    public static readonly DependencyProperty ColumnsSourceProperty = DependencyProperty.Register(
        nameof(ColumnsSource),
        typeof(IList),
        typeof(CustomDataGrid),
        new FrameworkPropertyMetadata(null, ColumnsSourcePropertyChanged, null));

    private static void ColumnsSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
        if (source is CustomDataGrid dataGrid)
        {
            dataGrid.OnColumnsSourcePropertyChanged((IList)e.OldValue, (IList)e.NewValue);
        }
    }

    public ObservableCollection<ColumnPropertyView> ColumnsSource
    {
        get => (ObservableCollection<ColumnPropertyView>) GetValue(ColumnsSourceProperty);
        set => SetValue(ColumnsSourceProperty, value);
    }

    protected virtual void OnColumnsSourcePropertyChanged(IList oldValue, IList newValue)
    {
        if (oldValue is INotifyCollectionChanged oldCollection)
            // With the weak event manager it is required that we register the handler on the the original
            // object which throws the exception. With this trick we work ourselves around the issue that
            // PropertyViewCollection<T> is a sub-class with an internal ObservableCollection<T> which
            // actually throws the event. <c>collection.SyncRoot</c> will return the internal
            // ObservableCollection<T> instance.
            // See as well: https://stackoverflow.com/questions/29270048/collectionchangedeventmanager-not-forwarding-event-for-custom-collection
            // And as well: https://stackoverflow.com/questions/25640010/why-weakeventmanager-does-not-fire-an-event-when-the-sender-is-not-the-nominal/29292233#29292233
            if (oldCollection is ICollection collection && collection.SyncRoot is INotifyCollectionChanged syncRootCollection)
                CollectionChangedEventManager.RemoveHandler(syncRootCollection, ColumnsSourceObject_CollectionChanged);
            else
                CollectionChangedEventManager.RemoveHandler(oldCollection, ColumnsSourceObject_CollectionChanged);

        // set auto generate columns to false when 'ColumnsSource' is used
        this.SetValue(DataGrid.AutoGenerateColumnsProperty, false);

        ResetColumnsFromColumnsSource(newValue);

        if (newValue is INotifyCollectionChanged newCollection)
            if (newCollection is ICollection collection && collection.SyncRoot is INotifyCollectionChanged syncRootCollection)
                CollectionChangedEventManager.AddHandler(syncRootCollection, ColumnsSourceObject_CollectionChanged);
            else
                CollectionChangedEventManager.AddHandler(newCollection, ColumnsSourceObject_CollectionChanged);
    }

    private void ColumnsSourceObject_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (this.IsEditing())
        {
            // At this point we have to make sure to exit the edit mode. Otherwise we will get into
            // trouble with the DataGrid 'CellAutomationValueHolder':
            //   The DataGrid caches the cells with 'CellAutomationValueHolder' in
            //   DataGrid._editingCellAutomationValueHolders. This class holds a reference to the
            //   DataGridColumn.
            //
            //   When we would not make sure to end the editing on row level (which clears
            //   the cache through method 'DataGrid.ReleaseCellAutomationValueHolders'),
            //   we would cause an inconsistency in the edit cache.
            //
            //   This might then lead to a crash because the 'CellAutomationValueHolder' class
            //   requires 'DataGridColumn.DataGridOwner' to be set.
            //
            //   See as well: https://github.com/dotnet/wpf/pull/6553

            if (CurrentCell.IsValid)
            {
                this.CommitEdit(DataGridEditingUnit.Row, true);
            }
            else
            {
                this.CancelEdit(DataGridEditingUnit.Row);
            }
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var column in e.NewItems)
                    AddColumnFromColumnPropertyView((ColumnPropertyView)column);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var column in e.OldItems)
                    RemoveColumnByColumnPropertyView((ColumnPropertyView)column);
                break;
            case NotifyCollectionChangedAction.Replace:
                Columns[e.NewStartingIndex] = e.NewItems[0] as DataGridColumn;
                break;
            case NotifyCollectionChangedAction.Move:
                Columns.Move(e.OldStartingIndex, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                ResetColumnsFromColumnsSource(e.NewItems);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetColumnsFromColumnsSource(IList newItems)
    {
        Columns.Clear();
        if (newItems == null)
            return;
        foreach (var column in newItems)
            AddColumnFromColumnPropertyView((ColumnPropertyView)column);
    }

    public static readonly DependencyProperty DrillDownActionDictionaryProperty = DependencyProperty.Register(
        nameof(DrillDownActionDictionary),
        typeof(IDictionary), typeof(CustomDataGrid),
        new FrameworkPropertyMetadata(null, null, null));

    public BindingBase LookupItemsSource { get; set; }

    public IDictionary DrillDownActionDictionary
    {
        get => (IDictionary) GetValue(DrillDownActionDictionaryProperty);
        set => SetValue(DrillDownActionDictionaryProperty, value);
    }

    private void AddColumnFromColumnPropertyView(ColumnPropertyView columnPropertyView)
    {
        if (columnPropertyView.IsHidden)
            return;
        var dataGridColumn = GenerateColumnFromColumnPropertyView(columnPropertyView);
        DataGridColumnExtensions.SetColumnViewModel(dataGridColumn, columnPropertyView);
        Columns.Add(dataGridColumn);
    }

    private void RemoveColumnByColumnPropertyView(ColumnPropertyView columnPropertyView)
    {
        var dataGridColumn = GetColumnFromColumnViewModel(columnPropertyView);
        if (dataGridColumn != null)
        {
            Columns.Remove(dataGridColumn);
        }
    }

    private DataGridColumn? GetColumnFromColumnViewModel(object columnViewModel)
    {
        return Columns.FirstOrDefault(col => DataGridColumnExtensions.GetColumnViewModel(col) == columnViewModel);
    }

    // ReSharper disable once UnusedParameter.Local
    private DataGridColumn GenerateColumnFromColumnPropertyView(ColumnPropertyView columnPropertyView)
    {
        // START_NOTE: party copied from class DataGridColumn, method: internal static DataGridColumn CreateDefaultColumn(ItemPropertyInfo itemProperty)
        Debug.Assert(columnPropertyView != null && columnPropertyView.PropertyInfo != null, "columnPropertyView and/or its PropertyType member cannot be null");
            
        DataGridColumn? dataGridColumn = null;
        DataGridComboBoxColumn? comboBoxColumn = null;
        Type propertyType = columnPropertyView.PropertyInfo.PropertyType;

        // determine the type of column to be created and create one
        if (propertyType.IsEnum)
        {
            comboBoxColumn = new DataGridComboBoxColumn();
            comboBoxColumn.ItemsSource = Enum.GetValues(propertyType);
            dataGridColumn = comboBoxColumn;
        }
        else if (typeof(string).IsAssignableFrom(propertyType))
        {
            dataGridColumn = new DataGridTextColumn();
        }
        else if (typeof(bool).IsAssignableFrom(propertyType))
        {
            dataGridColumn = new DataGridCheckBoxColumn();
        }
        else if (typeof(Uri).IsAssignableFrom(propertyType))
        {
            dataGridColumn = new DataGridHyperlinkColumn();
        }
        else
        {
            dataGridColumn = new DataGridTextColumn();
        }

        // determine if the datagrid can sort on the column or not
        if (!typeof(IComparable).IsAssignableFrom(propertyType))
        {
            dataGridColumn.CanUserSort = false;
        }

        dataGridColumn.Header = columnPropertyView.PropertyInfo.Name; // NOTE: Will be overriden in GenerateDataGridColumnExtension(..)

        // Set the data field binding for such created columns and
        // choose the BindingMode based on editability of the property.
        DataGridBoundColumn? boundColumn = dataGridColumn as DataGridBoundColumn;
        if (boundColumn != null || comboBoxColumn != null)
        {
            Binding binding = new Binding(columnPropertyView.PropertyInfo.Name);

            if (columnPropertyView.PropertyInfo.PropertyType.IsNumericType())
            {
                binding.Converter = new EnterNumericValueConverter();
                binding.ConverterParameter = columnPropertyView.StringFormat;
                binding.ValidatesOnDataErrors = true;
                binding.NotifyOnValidationError = true;
                binding.ValidationRules.Add(new NumericValueValidationRule(columnPropertyView.PropertyInfo.PropertyType));
            }

            if (comboBoxColumn != null)
            {
                comboBoxColumn.SelectedItemBinding = binding;
            }
            else
            {
                boundColumn.Binding = binding;
            }

            if (columnPropertyView.IsReadOnly ||
                !columnPropertyView.PropertyInfo.CanWrite)
            {
                binding.Mode = BindingMode.OneWay;
                dataGridColumn.IsReadOnly = true;
            }
        }

        // END_NOTE

        bool isReadOnly;
        if (columnPropertyView.IsReadOnly)
        {
            isReadOnly = true;
        }
        else
        {
            isReadOnly = (bool) GetValue(IsReadOnlyProperty);
        }

        GenerateDataGridColumnExtension(ref dataGridColumn, columnPropertyView, isReadOnly);

        return dataGridColumn;
    }

    private void AutoGeneratingColumnApplyControlImprovement(DataGridColumn column, string propertyName, Type propertyType)
    {
        // make the combo box click-able
        if (column is DataGridCheckBoxColumn && !column.IsReadOnly)
        {
            var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkboxFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkboxFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            checkboxFactory.SetValue(IsEnabledProperty, new Binding(nameof(IsReadOnly))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    // TODO: this is not optional because it reads the IsReadOnly from the DataGrid, NOT from the DataGridCheckBoxColumn object. 
                    AncestorType = typeof(CustomDataGrid)
                },
                Converter = new InverseBooleanConverter()
            });
            checkboxFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding(propertyName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            });

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = checkboxFactory;
            dataTemplate.Seal();

            var oldColumn = column;
            column = new DataGridTemplateColumn
            {
                CellTemplate = dataTemplate,

                // fields from origin
                // TODO: this will kill bindings which existed before!
                Header = column.Header,
                SortMemberPath = column.SortMemberPath,
                CanUserReorder = column.CanUserReorder,
                CanUserResize = column.CanUserResize,
                CanUserSort = column.CanUserSort,
                SortDirection = column.SortDirection,
                HeaderStringFormat = column.HeaderStringFormat,
                HeaderStyle = column.HeaderStyle,
                HeaderTemplate = column.HeaderTemplate,
                HeaderTemplateSelector = column.HeaderTemplateSelector,
                IsReadOnly = column.IsReadOnly,
                MaxWidth = column.MaxWidth,
                MinWidth = column.MinWidth,
                Visibility = column.Visibility,
                Width = column.Width
            };

            if (oldColumn.DisplayIndex != -1)
                column.DisplayIndex = oldColumn.DisplayIndex;
        }

        object? errorTemplate = null;
        if (Application.Current.Resources.Contains(ErrorTemplateDefaultResourceName))
        {
            errorTemplate = (ControlTemplate)Application.Current.Resources[ErrorTemplateDefaultResourceName];
        }
        else
        {
#if DEBUG
            Debug.WriteLine($"Could not find the style {ErrorTemplateDefaultResourceName} required for validation.");
#endif
        }

        // show numbers always right
        if (column is DataGridTextColumn textColumn)
        {
            var defaultElementStyle = textColumn.ElementStyle == null
                ? new Style(typeof(TextBlock))
                : new Style(typeof(TextBlock), textColumn.ElementStyle);
            if (errorTemplate != null)
                defaultElementStyle.Setters.Add(new Setter(Validation.ErrorTemplateProperty, errorTemplate));
            var defaultEditingElementStyle = textColumn.EditingElementStyle == null
                ? new Style(typeof(TextBox))
                : new Style(typeof(TextBox), textColumn.EditingElementStyle);
            if (errorTemplate != null)
                defaultEditingElementStyle.Setters.Add(new Setter(Validation.ErrorTemplateProperty, errorTemplate));

            if (propertyType.IsNumericType())
            {
                var elementStyle = new Style(typeof(TextBlock), defaultElementStyle);
                elementStyle.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Right));
                textColumn.ElementStyle = elementStyle;
            }
            else if (propertyType == typeof(Guid))
            {
                var elementStyle = new Style(typeof(TextBlock), defaultElementStyle);
                if (errorTemplate != null)
                    elementStyle.Setters.Add(new Setter(Validation.ErrorTemplateProperty, errorTemplate));
                elementStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
                elementStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(nameof(TextBlock.Text))
                {
                    RelativeSource = RelativeSource.Self
                }));
                textColumn.ElementStyle = elementStyle;
            }
            else if (errorTemplate != null)
            {
                textColumn.ElementStyle = defaultElementStyle;
            }

            textColumn.EditingElementStyle = defaultEditingElementStyle;
        }

        if (column is DataGridHyperlinkCommandColumn hyperlinkCommandColumn)
        {
            if (propertyType.IsNumericType())
            {
                // TODO: this style setting doesn't work, therefore it is statically set in DataGridHyperlinkCommandColumn, see constructor template generation

                var elementStyle = new Style(typeof(TextBlock));
                elementStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                hyperlinkCommandColumn.ElementStyle = elementStyle;
            }
        }
    }

    internal const string ErrorTemplateDefaultResourceName = "ErrorTemplateSilverlightStyle";
    private const string ComboBoxColumnCellStyleResourceName = "SpecialComboBoxCellStyle";

    private void GenerateDataGridColumnExtension(ref DataGridColumn column, ColumnPropertyView propertyView, bool isReadOnly)
    {
        if (propertyView.ShowHierarchicalTree)
        {
            var newColumn = new DataGridTreeTextColumn();
            newColumn.Header = propertyView.PropertyInfo.Name;
            newColumn.Binding = new Binding(propertyView.PropertyInfo.Name);
            newColumn.LevelBinding = new Binding("Level");
            newColumn.HasChildrenBinding = new Binding("HasChildren");
            newColumn.IsExpandedBinding = new Binding("IsExpanded");
            column = newColumn;
        }
        else if (propertyView.LookupDefinition != null &&
                 (propertyView.PropertyInfo.PropertyType == typeof(string) || propertyView.PropertyInfo.PropertyType == typeof(string[])) &&
                 !isReadOnly)
        {
            if (!(LookupItemsSource is Binding))
                throw new NotSupportedException(
                    $"The property {LookupItemsSource} must be given when using a column with lookup.");

            // use lookup field
            var newLookupComboBoxColumn = new DataGridLookupComboBoxColumn();
            newLookupComboBoxColumn.ItemsSourceBinding = new Binding(
                $"{((Binding) LookupItemsSource).Path?.Path}[{propertyView.PropertyInfo.Name}].View")
            {
                Source = DataContext,
                Mode = BindingMode.OneTime
            };
            newLookupComboBoxColumn.Header = propertyView.PropertyInfo.Name;
            newLookupComboBoxColumn.SelectedValueBinding = new Binding(propertyView.PropertyInfo.Name);

            if (propertyView.LookupDefinition.FieldSelectorFunc == null)
            {
                Debug.Write($"ERROR: FieldSelectorFunc not set in LookupDefinition for column {propertyView.PropertyInfo.Name}");
            }
            else
            {
                newLookupComboBoxColumn.SelectedValuePath =
                    propertyView.LookupDefinition.FieldSelectorFunc.GetMemberName();
            }

            column = newLookupComboBoxColumn;
        }
        else if (propertyView.PropertyInfo.PropertyType == typeof(byte[]) &&
                 propertyView.PropertyInfo.GetCustomAttribute<BinaryImageAttribute>() != null)
        {
            var newColumn = new DataGridImageColumn();
            newColumn.Binding = new Binding(propertyView.PropertyInfo.Name);
            newColumn.Header = propertyView.PropertyInfo.Name;
            column = newColumn;
        }
        else if (propertyView.PropertyInfo.GetCustomAttribute<InfoImagesAttribute>() != null)
        {
            var newColumn = new DataGridInfoImageColumn();
            newColumn.Binding = new Binding(propertyView.PropertyInfo.Name);
            newColumn.Header = propertyView.PropertyInfo.Name;
            column = newColumn;
        }
        else if (propertyView.DrillDownDefinition != null &&
                 column is DataGridBoundColumn bondColumn2 &&
                 isReadOnly)
        {
            /*if (!(DrillDownItemsSource is Binding))
                throw new NotSupportedException(
                    $"The property {DrillDownItemsSource} must be given when using a column drill down in a column.");
            */
            var drillDownColumn = new DataGridHyperlinkCommandColumn();
            drillDownColumn.Binding = bondColumn2.Binding;
            drillDownColumn.Header = propertyView.PropertyInfo.Name;
            drillDownColumn.Binding = new Binding(propertyView.PropertyInfo.Name);
            drillDownColumn.CommandBinding =
                new Binding($"DrillDownActionDictionary[{propertyView.PropertyInfo.Name}]")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(CustomDataGrid)
                    },
                    Converter = new IActionToICommandConverter(),
                    Mode = BindingMode.OneTime
                };
            drillDownColumn.CommandParameterBinding = new Binding("SelectedItems")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(CustomDataGrid)
                }
            };
            column = drillDownColumn;
        }
        else if (propertyView.PropertyInfo.PropertyType.IsEnum)
        {
            var newColumn = new DataGridComboBoxColumn();
            newColumn.SelectedValueBinding = new Binding(propertyView.PropertyInfo.Name);
            newColumn.Header = propertyView.PropertyInfo.Name;
            column = newColumn;
        }
        else if (propertyView.PropertyInfo.PropertyType.IsGenericType &&
                 propertyView.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                 propertyView.PropertyInfo.PropertyType.GenericTypeArguments[0].IsEnum)
        {
            var newColumn = new DataGridComboBoxColumn();
            newColumn.SelectedValueBinding = new Binding(propertyView.PropertyInfo.Name);
            newColumn.Header = propertyView.PropertyInfo.Name;
            column = newColumn;
        }

        // required because it is not set automatically
        column.SetValue(DataGridColumn.IsReadOnlyProperty, isReadOnly);

        AutoGeneratingColumnApplyControlImprovement(column, propertyView.PropertyInfo.Name, propertyView.PropertyInfo.PropertyType);

        if (propertyView.Width.HasValue)
            column.Width = new DataGridLength(propertyView.Width.Value);
        else
            column.Width = new DataGridLength(); // auto field length

        // update style
        if (column is DataGridComboBoxColumn || column is DataGridLookupComboBoxColumn)
        {
            if (Application.Current.Resources.Contains(ComboBoxColumnCellStyleResourceName) &&
                Application.Current.Resources[ComboBoxColumnCellStyleResourceName] is Style comboBoxStyle)
            {
                column.CellStyle = comboBoxStyle;
            }
#if DEBUG
            else
            {
                Debug.WriteLine(
                    $"Could not find the style {ComboBoxColumnCellStyleResourceName} of type Style required for the lookup combo box.");
            }
#endif
        }

        // enum translation
        if (column is DataGridComboBoxColumn enumCombBoxColumn &&
            (propertyView.PropertyInfo.PropertyType.IsEnum ||
             (
                 propertyView.PropertyInfo.PropertyType.IsGenericType &&
                 propertyView.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                 propertyView.PropertyInfo.PropertyType.GenericTypeArguments[0].IsEnum
             )))
        {
            var style = new Style(typeof(ComboBox));

            style.Setters.Add(new Setter(FocusManager.FocusedElementProperty, new Binding
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            }));

            style.Setters.Add(new Setter(ComboBox.SelectedValuePathProperty, nameof(EnumValueViewModel.Value)));
            style.Setters.Add(new Setter(ComboBox.DisplayMemberPathProperty, nameof(EnumValueViewModel.Name)));

            style.Setters.Add(new Setter(ComboBox.IsDropDownOpenProperty, new Binding(nameof(PauseExcelNavigation))
            {
                RelativeSource = new RelativeSource { AncestorType = typeof(CustomDataGrid) }
            }));

            style.Setters.Add(new Setter(ComboBox.ItemsSourceProperty, new Binding
            {
                Converter = new EnumToCollectionConverter(propertyView.PropertyInfo.PropertyType),
                Mode = BindingMode.OneTime,
                Path = new PropertyPath(propertyView.PropertyInfo.Name)
            }));

            enumCombBoxColumn.ElementStyle = style;
            enumCombBoxColumn.EditingElementStyle = style;
        }

        // update binding
        Binding binding = null;
        if (column is DataGridComboBoxColumn combBoxColumn)
        {
            binding = combBoxColumn.SelectedValueBinding as Binding;
        }
        else if (column is DataGridLookupComboBoxColumn lookupComboBoxColumn)
        {
            binding = lookupComboBoxColumn.SelectedValueBinding as Binding;
        }
        else if (column is DataGridHyperlinkCommandColumn hyperlinkCommandColumn)
        {
            binding = hyperlinkCommandColumn.Binding as Binding;
        }
        else if (column is DataGridBoundColumn boundColumn)
        {
            binding = boundColumn.Binding as Binding;
        }

        if (binding != null)
        {
            if (propertyView.ArrayIndex.HasValue)
                binding.Path.Path = $"{binding.Path.Path}[{propertyView.ArrayIndex.Value}]";

            if (!isReadOnly)
                binding.ValidatesOnDataErrors = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
        }

        // apply ColumnPropertyView properties
        if (propertyView.DisplayName != null)
        {
            if (propertyView.DisplayShortName != null)
            {
                // TODO: implement here logic in WPF to show the short name only when the header is not big enough to show propertyView.DisplayName!
                column.Header = propertyView.DisplayShortName.LanguageOrDefault(CultureInfo.CurrentUICulture);
            }
            else
            {
                column.Header = propertyView.DisplayName.LanguageOrDefault(CultureInfo.CurrentUICulture);
            }
        }

        if (propertyView.DisplayShortName != null || propertyView.DisplayDescription != null)
        {
            var description = propertyView.DisplayDescription?.LanguageOrDefault(CultureInfo.CurrentUICulture);

            var tooltipHeaderText = propertyView.DisplayName?.LanguageOrDefault(CultureInfo.CurrentUICulture) ??
                                    column.Header.ToString();

            var tooltip = new StackPanel();
            // header
            var tooltipHeader = new TextBlock();
            tooltipHeader.FontWeight = FontWeights.Bold;
            tooltipHeader.FontSize = 14;
            tooltipHeader.Margin = new Thickness(0, 0, 0, 5);
            tooltipHeader.Inlines.Add(tooltipHeaderText);
            tooltip.Children.Add(tooltipHeader);
            // lines
            if (description != null)
            {
                var tooltipDescription = new TextBlock();
                tooltipDescription.TextWrapping = TextWrapping.Wrap;
                tooltipDescription.MaxWidth = 250;
                tooltipDescription.Inlines.Add(description);
                tooltip.Children.Add(tooltipDescription);
            }

            DataGridColumnExtensions.SetHeaderTooltip(column, tooltip);
        }

        if (binding != null)
        {
            if (propertyView.StringFormat != default)
                // TODO: string format in combination with UpdateSourceTarget=PropertyChanged is not a good idea, see
                //       https://social.msdn.microsoft.com/Forums/vstudio/en-US/0ee0019d-4dcc-4579-8de2-2016a534c317/twoway-binding-doesnt-work-using-stringformat-why-and-how-to-fix-it?forum=wpf
                //       We should use a converter in such cases
                binding.StringFormat = propertyView.StringFormat;
            if (propertyView.NullDisplayText != default)
                binding.TargetNullValue = propertyView.NullDisplayText;
        }

        // set filterable attribute
        if (!propertyView.IsFilterable)
        {
            DataGridColumnExtensions.SetCanUserFilter(column, false);
        }
    }

    #endregion

    #region Clipboard Copy

    protected override void OnCopyingRowClipboardContent(DataGridRowClipboardEventArgs args)
    {
        base.OnCopyingRowClipboardContent(args);

        if (SelectionUnit == DataGridSelectionUnit.FullRow &&
            SelectedItems.Count == 1)
        {
            // when the selection mode is FullRow and only one row is selected we don't want to copy
            // the whole row but just the current cell value.

            if (CurrentCell.IsValid)
            {
                var currentCellRowContent =
                    args.ClipboardRowContent.FirstOrDefault(rc => rc.Column == CurrentCell.Column);

                if (currentCellRowContent.Column != null) // test if column was found
                {
                    args.ClipboardRowContent.Clear();
                    args.ClipboardRowContent.Add(currentCellRowContent);
                }
            }
        }
    }

    #endregion

    #region Clipboard Paste

    private static void OnCanExecutePaste(object target, CanExecuteRoutedEventArgs args)
    {
        ((CustomDataGrid)target).OnCanExecutePaste(args);
    }

    /// <summary>
    /// This virtual method is called when ApplicationCommands.Paste command query its state.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnCanExecutePaste(CanExecuteRoutedEventArgs args)
    {
        args.CanExecute = true;
        args.Handled = true;
    }

    private static void OnExecutedPaste(object target, ExecutedRoutedEventArgs args)
    {
        ((CustomDataGrid)target).OnExecutedPaste(args);
    }

    /// <summary>
    /// This virtual method is called when ApplicationCommands.Paste command is executed.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnExecutedPaste(ExecutedRoutedEventArgs args)
    {
        // With help of source: http://stackoverflow.com/questions/4118617/wpf-datagrid-pasting

        // parse the clipboard data            
        List<object[]> rowData = ClipboardHelper.ParseClipboardData();

        // call OnPastingCellClipboardContent for each cell
        int minRowIndex = Items.IndexOf(CurrentItem);
        int maxRowIndex = Items.Count - 1;
        int minColumnDisplayIndex = (SelectionUnit != DataGridSelectionUnit.FullRow) ? Columns.IndexOf(CurrentColumn) : 0;
        int maxColumnDisplayIndex = Columns.Count - 1;
        int rowDataIndex = 0;

        var displayColumns = new List<DataGridColumn>();
        for (int i = minColumnDisplayIndex; i <= maxColumnDisplayIndex; i++)
        {
            DataGridColumn column = ColumnFromDisplayIndex(i);
            if (column.Visibility == Visibility.Visible)
                displayColumns.Add(column);
        }

        for (int i = minRowIndex; i <= maxRowIndex && rowDataIndex < rowData.Count; i++, rowDataIndex++)
        {
            BeginEditCommand.Execute(null, this);

            int maxIteration = rowData[rowDataIndex].Length < displayColumns.Count
                ? rowData[rowDataIndex].Length
                : displayColumns.Count;
            for (var columnDataIndex = 0; columnDataIndex < maxIteration; columnDataIndex++)
            {
                var column = displayColumns[columnDataIndex];
                if (!column.IsReadOnly)
                {
                    column.OnPastingCellClipboardContent(Items[i], rowData[rowDataIndex][columnDataIndex]);
                }
            }

            CommitEditCommand.Execute(this, this);

            // note: the 'BeginEditCommand' might add an new row via the NewItemPlaceholder. With this we update the max. last row for our loop
            maxRowIndex = Items.Count - 1;
        }
    }

    /// <summary>
    ///     Whether the end-user can add new rows to the ItemsSource.
    /// </summary>
    public bool CanUserPasteToNewRows
    {
        get => (bool)GetValue(CanUserPasteToNewRowsProperty);
        set => SetValue(CanUserPasteToNewRowsProperty, value);
    }

    /// <summary>
    ///     DependencyProperty for CanUserAddRows.
    /// </summary>
    public static readonly DependencyProperty CanUserPasteToNewRowsProperty =
        DependencyProperty.Register( nameof(CanUserPasteToNewRows),
            typeof(bool), typeof(CustomDataGrid),
            new FrameworkPropertyMetadata(true, null, null));

    #endregion Clipboard Paste

    #region Data source interaction

    private bool _newLineFirstCellFocused;

    public static readonly DependencyProperty LastEditCancelledProperty =
        DependencyProperty.Register(nameof(LastEditCancelled),
            typeof(bool), typeof(CustomDataGrid),
            new FrameworkPropertyMetadata(false, null, null));

    // NOTE: this is a ugly solution here...
    public bool LastEditCancelled
    {
        get => (bool) GetValue(LastEditCancelledProperty);
        set => SetValue(LastEditCancelledProperty, value);
    }

    protected override void OnCanExecuteCommitEdit(CanExecuteRoutedEventArgs e)
    {
        LastEditCancelled = false;
        base.OnCanExecuteCommitEdit(e);
    }

    protected override void OnExecutedCancelEdit(ExecutedRoutedEventArgs e)
    {
        LastEditCancelled = true;
        base.OnExecutedCancelEdit(e);
    }

    protected override void OnCurrentCellChanged(EventArgs e)
    {
        base.OnCurrentCellChanged(e);

        // commit editing when is new line via NewItemPlaceholder
        if (ItemsSource is IEditableCollectionView editableCollectionView &&
            editableCollectionView.IsAddingNew)
        {
            if (!_newLineFirstCellFocused)
            {
                _newLineFirstCellFocused = true;
                return;
            }

            // don't create invalid entries
            if (CurrentItem is INotifyDataErrorInfo validatableItem &&
                validatableItem.HasErrors)
            {
                return;
            }

            // add new row (which was created through the NewItemPlaceholder)
            CommitEdit();
        }
    }

    private void OnSelectedCellsChanged_DataSourceInteraction()
    {
        _newLineFirstCellFocused = false;
    }

    #endregion

    #region Excel Navigate

    public static readonly DependencyProperty PauseExcelNavigationProperty =
        DependencyProperty.Register(nameof(PauseExcelNavigation),
            typeof(bool), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(false, null, null));

    public bool PauseExcelNavigation
    {
        get => (bool) GetValue(PauseExcelNavigationProperty);
        set => SetValue(PauseExcelNavigationProperty, value);
    }

    /// <summary>
    /// Implement 'Enter' key as 'next field' or 'next row' action
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private static void OnPreviewKeyDown(object sender, RoutedEventArgs eventArgs)
    {
        CustomDataGrid @this = sender as CustomDataGrid;
        if (@this == null) return;

        if (@this.PauseExcelNavigation)
            return;
            
        KeyEventArgs e = eventArgs as KeyEventArgs;
        if (e == null) return;

        var uiElement = e.OriginalSource as UIElement;
        if (uiElement == null) return;
            
        // local helper function
        DataGridCell GetCurrentDataGridCell(DataGrid dataGrid)
        {
            var currentItem = dataGrid.CurrentItem;
            if (currentItem == null) return null;

            var cellContent = dataGrid.CurrentCell.Column.GetCellContent(currentItem);

            return cellContent?.Parent as DataGridCell;
        }

        bool isShiftPressed = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        switch (e.Key)
        {
            case Key.Down:
            case Key.Up:
            {
                DataGridCell dataGridCell = GetCurrentDataGridCell(@this);
                if (dataGridCell == null) return;

                bool isEditing = @this.IsEditing();

                if (isEditing)
                    @this.CommitEdit();

                if (!isShiftPressed)
                {
                    bool cellChanged;
                    if (e.Key == Key.Down)
                    {
                        try
                        {
                            cellChanged = dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        }
                        catch(ArgumentNullException)
                        {
                            // known error: "Value cannot be null. Arg_ParamName_Name"
#if DEBUG
                            Debug.WriteLine("CustomDataGrid: known error: Value cannot be null.Arg_ParamName_Name");
#else
                                    // do nothing here
#endif
                            return;
                        }
                    }
                    else
                        cellChanged = dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));

                    if (cellChanged)
                    {
                        if (isEditing)
                            @this.BeginEdit();

                        @this.SelectedItem = @this.CurrentCell.Item;
                    }

                    e.Handled = true;
                }

                break;
            }
            case Key.Escape:
            {
                @this.CancelEdit();
            }
                break;
            case Key.Enter:
            {
                bool isEditing = @this.IsEditing();
                if (isEditing)
                {
                    @this.CommitEdit();
                }

                var currentCell = @this.CurrentCell;

                // move to the next cell
                DataGridCell dataGridCell = GetCurrentDataGridCell(@this);
                if (dataGridCell == null) return;

                if (isShiftPressed)
                    dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                else if (isEditing)
                    dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                if ((isEditing && isShiftPressed) || currentCell == @this.CurrentCell)
                {
                    dataGridCell = GetCurrentDataGridCell(@this);
                    if (dataGridCell == null) return;

                    // jump to the next line
                    dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }

                // move next until editable field found
                if (isEditing &&
                    @this.CurrentColumn != null &&
                    @this.CurrentColumn.IsReadOnly &&
                    @this.Columns.Any(
                        column => !column.IsReadOnly && column.Visibility == Visibility.Visible))
                {
                    // this is a self-protection to avoid endless loops
                    int maxTurns = @this.Columns.Count(column => column.Visibility == Visibility.Visible) + 1;
                    int i = 0;

                    do
                    {
                        dataGridCell = GetCurrentDataGridCell(@this);
                        if (dataGridCell == null) return;
                        dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    } while (@this.CurrentColumn.IsReadOnly && i++ < maxTurns );
                }

                // start editing the current cell
                if (@this.CurrentColumn != null && !@this.CurrentColumn.IsReadOnly)
                    @this.BeginEdit();

                e.Handled = true;
                break;
            }
        }
    }

    private void OnSelectedCellsChanged_ExcelNavigation(SelectedCellsChangedEventArgs e)
    {
        // this implementation lets a cell start editing right away
        if (IsReadOnly)
            return;

        if (e.AddedCells != null && e.AddedCells.Count == 1 &&
            SelectedItems != null && SelectedItems.Count == 1 &&
            CurrentCell.IsValid &&
            e.AddedCells.Contains(CurrentCell))
        {
            var cell = CurrentCell;
            if (!cell.Column.IsReadOnly)
            {
                BeginEdit();
            }
        }
    }

    #endregion

    #region Filter functionality
        
    public static readonly DependencyProperty IsFilterVisibleProperty =
        DependencyProperty.RegisterAttached(nameof(IsFilterVisible), typeof(bool), typeof(CustomDataGrid),
            new FrameworkPropertyMetadata(false, null, null));

    public bool IsFilterVisible
    {
        get => (bool) GetValue(IsFilterVisibleProperty);
        set => SetValue(IsFilterVisibleProperty, value);
    }

    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(nameof(Filter), typeof(IPropertyFilterCollection), typeof(CustomDataGrid),
            new PropertyMetadata(null, OnFilterPropertyChanged));

    public IPropertyFilterCollection Filter
    {
        get => (IPropertyFilterCollection) GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
        
    private static void OnFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nothing, just necessary here to support binding.
    }

    #endregion

    #region Drag&Drop for rows functionality

    private bool _temporaryDragDropIsReadOnly;
    private bool _isDragging;
    private object _draggingTarget;

    public static readonly DependencyProperty DraggedItemProperty =
        DependencyProperty.Register(nameof(DraggedItem),
            typeof(object), typeof(CustomDataGrid),
            new FrameworkPropertyMetadata(null, null, null));
        
    public object DraggedItem
    {
        get => GetValue(DraggedItemProperty);
        set => SetValue(DraggedItemProperty, value);
    }

    public static readonly DependencyProperty DragPopupProperty =
        DependencyProperty.Register(nameof(DragPopup),
            typeof(Popup), typeof(CustomDataGrid),
            new FrameworkPropertyMetadata(null, null, null));

    public Popup DragPopup
    {
        get => (Popup) GetValue(DragPopupProperty);
        set => SetValue(DragPopupProperty, value);
    }

    private void OnMouseLeftButtonDown_DragDropRow(MouseButtonEventArgs e)
    {
        Debug.WriteLine("ButtonDown -- Drag");

        if (DragPopup == null) // drag-drop is only supported when the DragPopup is set
            return;

        if (this.IsEditing())
            return;

        // drag-drop is only if ItemsSource implements IList
        if (!(ItemsSource is IList || ItemsSource is CollectionView collectionView && collectionView.SourceCollection is IList))
            return;

        var row = this.FindFromPoint<DataGridRow>(e.GetPosition(this));
        if (row == null || row.IsEditing) return;

        //set flag that indicates we're capturing mouse movements
        _isDragging = true;
        DraggedItem = row.Item;
    }

    private void OnMouseLeftButtonUp_DragDropRow(MouseButtonEventArgs e)
    {
        Debug.WriteLine("ButtonUp - DROP");

        if (!_isDragging || this.IsEditing())
            return;

        //get the target item
        object targetItem = _draggingTarget;

        if ((targetItem == null || !ReferenceEquals(DraggedItem, targetItem)))
        {
            IList list = null;
            if (ItemsSource is IList iList)
            {
                list = iList;
            }
            else if (ItemsSource is CollectionView collectionView &&
                     collectionView.SourceCollection is IList collectionViewList)
            {
                list = collectionViewList;
            }

            if (list == null)
            {
                ResetDragDrop();
                return;
            }

            var targetIndex = list.IndexOf(targetItem);
            if (targetIndex == -1)
            {
                ResetDragDrop();
                return;
            }

            if (list.GetType().GetGenericTypeDefinition() == typeof(ObservableCollection<>))
            {
                var oldIndex = list.IndexOf(DraggedItem);

                if (oldIndex == -1)
                {
                    ResetDragDrop();
                    return;
                }

                //call itemsSourceList.Move(oldIndex, targetIndex);
                list.GetType().GetMethod(nameof(ObservableCollection<object>.Move))
                    .Invoke(list, new object[]
                    {
                        // old index
                        oldIndex,
                        
                        // new index
                        targetIndex
                    });
            }
            else
            {
                list.Insert(targetIndex, DraggedItem);
            }

            //select the dropped item
            SelectedItem = DraggedItem;
        }

        ResetDragDrop();
    }

    private void ResetDragDrop()
    {
        _isDragging = false;
        DragPopup.IsOpen = false;
        if (_temporaryDragDropIsReadOnly)
        {
            IsReadOnly = false;
            _temporaryDragDropIsReadOnly = false;
        }
    }

    private void OnMouseMove_DragDropRow(MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
            return;
            
        Debug.WriteLine("MouseMove");

        //display the popup if it hasn't been opened yet
        if (!DragPopup.IsOpen)
        {
            //switch to read-only mode
            if (!IsReadOnly)
            {
                _temporaryDragDropIsReadOnly = true;
                IsReadOnly = true;
            }

            //make sure the popup is visible
            DragPopup.IsOpen = true;
        }

        // set position and sizing once
        Size popupSize = new Size(DragPopup.ActualWidth, DragPopup.ActualHeight);
        DragPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

        //make sure the row under the grid is being selected
        var row = this.FindFromPoint<DataGridRow>(e.GetPosition(this));
        _draggingTarget = row.Item;

        Debug.WriteLine($"{DateTime.Now.Ticks} Dragged Target: {_draggingTarget}");
    }

    #endregion
}