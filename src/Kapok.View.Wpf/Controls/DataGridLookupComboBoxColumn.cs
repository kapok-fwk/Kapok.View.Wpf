using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Kapok.View.Wpf;

public class DataGridLookupComboBoxColumn : DataGridTemplateColumn
{
    public static readonly DependencyProperty DisplayMemberPathProperty =
        ItemsControl.DisplayMemberPathProperty.AddOwner(typeof(DataGridLookupComboBoxColumn),
            new FrameworkPropertyMetadata(null, null, null));

    public static readonly DependencyProperty SelectedValuePathProperty =
        Selector.SelectedValuePathProperty.AddOwner(typeof(DataGridLookupComboBoxColumn),
            new FrameworkPropertyMetadata(string.Empty, null, null));
        
    public DataGridLookupComboBoxColumn()
    {
        object errorTemplate = null;
        if (Application.Current.Resources.Contains(CustomDataGrid.ErrorTemplateDefaultResourceName))
        {
            errorTemplate = (ControlTemplate)Application.Current.Resources[CustomDataGrid.ErrorTemplateDefaultResourceName];
        }
        else
        {
#if DEBUG
            Debug.WriteLine($"Could not find the style {CustomDataGrid.ErrorTemplateDefaultResourceName} required for validation.");
#endif
        }

        CellTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            // TODO find here a better solution - it doesn't work with the ComboBox text box styled as used by microsoft, but with textBlock we aren't able to use the DisplayMemberPath object

            /*var comboBox = new ComboBox();
            comboBox.SetResourceReference(FrameworkElement.StyleProperty, DataGridComboBoxColumn.TextBlockComboBoxStyleKey);
            comboBox.IsTabStop = false;

            ApplyColumnProperties(comboBox);
            if (DisplayMemberPath != null && SelectedValueBinding is Binding selectedValueBinding2)
                comboBox.SetValue(DisplayMemberPathProperty, selectedValueBinding2.Path.Path + "." + DisplayMemberPath);
            else
                comboBox.SetValue(DisplayMemberPathProperty, SelectedValuePath);
            return comboBox;*/

            var textBlock = new TextBlock();

            BindingBase textBinding;
            if (!string.IsNullOrEmpty(DisplayMemberPath) &&
                SelectedValueBinding is Binding selectedValueBinding)
                textBinding = new Binding(selectedValueBinding.Path.Path + "." + DisplayMemberPath);
            else
                textBinding = SelectedValueBinding;

            textBlock.SetBinding(TextBlock.TextProperty, textBinding);
            if (errorTemplate == null)
                textBlock.SetValue(Validation.ErrorTemplateProperty, errorTemplate);

            return textBlock;
        });
        CellEditingTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            var lookupComboBox = new LookupComboBox();

            var focusStyle = new Style();
            var setter = new Setter
            {
                Property = FocusManager.FocusedElementProperty,
                Value = new Binding {RelativeSource = new RelativeSource(RelativeSourceMode.Self)}
            };
            focusStyle.Setters.Add(setter);

            lookupComboBox.Style = focusStyle;
            lookupComboBox.IsEditable = true;
            lookupComboBox.IsTextSearchEnabled = true;
            lookupComboBox.IsTextSearchCaseSensitive = false;
            lookupComboBox.StaysOpenOnEdit = true;
            lookupComboBox.SetBinding(ComboBox.IsDropDownOpenProperty, new Binding(nameof(CustomDataGrid.PauseExcelNavigation))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(CustomDataGrid), 1)
            });
            if (errorTemplate == null)
                lookupComboBox.SetValue(Validation.ErrorTemplateProperty, errorTemplate);

            ApplyColumnProperties(lookupComboBox);

            return lookupComboBox;
        });
    }

    private void ApplyColumnProperties(ComboBox comboBox)
    {
        comboBox.SetBinding(ItemsControl.ItemsSourceProperty, ItemsSourceBinding);
        comboBox.SetBinding(Selector.SelectedValueProperty, SelectedValueBinding);
        comboBox.SetValue(SelectedValuePathProperty, SelectedValuePath);
        if (string.IsNullOrEmpty(DisplayMemberPath))
            comboBox.SetValue(DisplayMemberPathProperty, SelectedValuePath);
        else
            comboBox.SetValue(DisplayMemberPathProperty, DisplayMemberPath);
        TextSearch.SetTextPath(comboBox, SelectedValuePath);
    }

    protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
    {
        // set focus on ComboBox
        var contentPresenter = editingElement as ContentPresenter;
        var comboBox = contentPresenter?.FindVisualChild<ComboBox>();
        var textBox = comboBox?.FindVisualChild<TextBox>();
        if (textBox != null)
            textBox.Focus();
        else
            comboBox?.Focus();

        return base.PrepareCellForEdit(editingElement, editingEventArgs);
    }

    private BindingBase _itemsSourceBinding;
    private BindingBase _selectedValueBinding;
        
    public BindingBase ItemsSourceBinding
    {
        get => _itemsSourceBinding;
        set
        {
            if (_itemsSourceBinding == value)
                return;
            _itemsSourceBinding = value;
            NotifyPropertyChanged(nameof(ItemsSourceBinding));
        }
    }

    public BindingBase SelectedValueBinding
    {
        get => _selectedValueBinding;
        set
        {
            if (_selectedValueBinding == value)
                return;
            _selectedValueBinding = value;
            NotifyPropertyChanged(nameof(SelectedValueBinding));
        }
    }

    public string DisplayMemberPath
    {
        get => (string) GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string SelectedValuePath
    {
        get => (string) GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    /// <summary>Gets or sets the binding object to use when getting or setting cell content for the clipboard.</summary>
    /// <returns>An object that represents the binding.</returns>
    public override BindingBase ClipboardContentBinding
    {
        get => base.ClipboardContentBinding ?? SelectedValueBinding;
        set => base.ClipboardContentBinding = value;
    }
}