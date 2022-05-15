using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kapok.View.Wpf;

public class DataGridTreeTextColumn : DataGridTemplateColumn
{
    private const string ToggleButtonStyleResourceName = "DataGridTreeTextColumn_ToggleButton";

    public DataGridTreeTextColumn()
    {
        CellTemplate = TemplateGenerator.CreateDataTemplate(() => BuildCellTemplate(false), sealTemplate: false);
        CellEditingTemplate = TemplateGenerator.CreateDataTemplate(() => BuildCellTemplate(true), sealTemplate: false);
    }

    object BuildCellTemplate(bool editingTemplate)
    {
        /* Builds a template which is (kinda) equal to:
<DataTemplate x:Key="DataGridTemplateColumn_HierarchyTree">
    <DataTemplate.Resources>
        <vmwpf:DataGridHierarchyColumnLevelToMarginConverter x:Key="HierarchyMarginConverter" />
        <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
    </DataTemplate.Resources>

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition MinWidth="19" Width="Auto" />
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition/>
        </Grid.RowDefinitions>
        <!-- ReSharper disable Xaml.BindingWithContextNotResolved -->

        <!-- Horizontal line -->
        <Rectangle x:Name="HorLn"
                   Margin="{Binding Path=Level, Converter={StaticResource HierarchyMarginConverter}, ConverterParameter=HLine}"
                   Height="1" Stroke="#DCDCDC" SnapsToDevicePixels="True" />
        <!-- Vertical line -->
        <Rectangle x:Name="VerLn" Width="1" Stroke="#DCDCDC"
                   Margin="{Binding Path=Level, Converter={StaticResource HierarchyMarginConverter}, ConverterParameter=VLine}"
                   Grid.Row="0" Grid.RowSpan="2" Grid.Column="0"
                   SnapsToDevicePixels="true" Fill="White" />

        <ToggleButton Grid.Row="0" Grid.Column="0"
                      IsChecked="{Binding IsExpanded, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                      Visibility="{Binding HasChildren, Converter={StaticResource BooleanToVisibilityConverter}}"
                      Margin="{Binding Path=Level, Converter={StaticResource HierarchyMarginConverter}, ConverterParameter=ToggleButton}"
                      Style="{StaticResource DataGridTreeTextColumn_ToggleButton}"
                      >
        </ToggleButton>

        <TextBlock Grid.Column="1" Grid.Row="0" Grid.ColumnSpan="2" Text="{Binding Path=Description}" Margin="1" />
        <!-- ReSharper restore Xaml.BindingWithContextNotResolved -->
    </Grid>
</DataTemplate>
         */

        if (!Application.Current.Resources.Contains(ToggleButtonStyleResourceName))
        {
            throw new NotSupportedException($"Could not find style: {ToggleButtonStyleResourceName}");
        }

        var toggleButtonStyle = (Style) Application.Current.Resources[ToggleButtonStyleResourceName];

        var levelToMarginConverter = new DataGridHierarchyColumnLevelToMarginConverter();
        var booleanToVisibilityConverter = new BooleanToVisibilityConverter();

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition() {MinWidth = 19, Width = GridLength.Auto});
        grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
        grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(0, GridUnitType.Star)});
        grid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
        grid.RowDefinitions.Add(new RowDefinition());

        var horizontalLine = new Rectangle
        {
            Name = "HorLn",
            Height = 1,
            Stroke = new SolidColorBrush(Color.FromRgb(0xDC, 0xDC, 0xDC)),
            SnapsToDevicePixels = true
        };
        horizontalLine.SetBinding(FrameworkElement.MarginProperty, new Binding
        {
            Converter = levelToMarginConverter,
            ConverterParameter = "HLine",
            Path = (LevelBinding as Binding)?.Path,
            FallbackValue = new Thickness(9, 1, 0, 0)
        });
        grid.Children.Add(horizontalLine);

        var verticalLine = new Rectangle
        {
            Name = "VerLn",
            Width = 1,
            Stroke = new SolidColorBrush(Color.FromRgb(0xDC, 0xDC, 0xDC)),
            SnapsToDevicePixels = true,
            Fill = new SolidColorBrush(Colors.White)
        };
        verticalLine.SetBinding(FrameworkElement.MarginProperty, new Binding
        {
            Converter = levelToMarginConverter,
            ConverterParameter = "VLine",
            Path = (LevelBinding as Binding)?.Path,
            FallbackValue = new Thickness(0, 0, 0, 0)
        });
        Grid.SetRow(verticalLine, 0);
        Grid.SetRowSpan(verticalLine, 2);
        Grid.SetColumn(verticalLine, 0);
        grid.Children.Add(verticalLine);

        var treeToggleButton = new ToggleButton
        {
            Style = toggleButtonStyle
        };
        Grid.SetRow(treeToggleButton, 0);
        Grid.SetColumn(treeToggleButton, 0);
        treeToggleButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding
        {
            Path = (IsExpandedBinding as Binding)?.Path,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        treeToggleButton.SetBinding(UIElement.VisibilityProperty, new Binding
        {
            Converter = booleanToVisibilityConverter,
            Path = (HasChildrenBinding as Binding)?.Path,
            FallbackValue = Visibility.Collapsed
        });
        treeToggleButton.SetBinding(FrameworkElement.MarginProperty, new Binding
        {
            Converter = levelToMarginConverter,
            ConverterParameter = "ToggleButton",
            Path = (LevelBinding as Binding)?.Path,
            FallbackValue = new Thickness(0, 1, 0, 0)
        });
        grid.Children.Add(treeToggleButton);

        if (editingTemplate)
        {
                
            var focusStyle = new Style();
            var setter = new Setter
            {
                Property = FocusManager.FocusedElementProperty,
                Value = new Binding {RelativeSource = new RelativeSource(RelativeSourceMode.Self)}
            };
            focusStyle.Setters.Add(setter);

            var textBox = new TextBox
            {
                BorderThickness = new Thickness(0),
                Style = focusStyle
            };
            textBox.SetBinding(TextBox.TextProperty, new Binding
            {
                StringFormat = Binding.StringFormat,
                Converter = (Binding as Binding)?.Converter,
                ConverterCulture = (Binding as Binding)?.ConverterCulture,
                ConverterParameter = (Binding as Binding)?.ConverterParameter,
                ElementName = (Binding as Binding)?.ElementName,
                FallbackValue = (Binding as Binding)?.FallbackValue,
                TargetNullValue = (Binding as Binding)?.TargetNullValue,
                Mode = (Binding as Binding)?.Mode ?? BindingMode.Default,
                UpdateSourceTrigger = (Binding as Binding)?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default,
                Path = (Binding as Binding)?.Path
            });
            Grid.SetColumn(textBox, 1);
            Grid.SetRow(textBox, 0);
            Grid.SetColumnSpan(textBox, 2);
            grid.Children.Add(textBox);
        }
        else
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(1)
            };
            textBlock.SetBinding(TextBlock.TextProperty, new Binding
            {
                StringFormat = Binding.StringFormat,
                Converter = (Binding as Binding)?.Converter,
                ConverterCulture = (Binding as Binding)?.ConverterCulture,
                ConverterParameter = (Binding as Binding)?.ConverterParameter,
                ElementName = (Binding as Binding)?.ElementName,
                FallbackValue = (Binding as Binding)?.FallbackValue,
                TargetNullValue = (Binding as Binding)?.TargetNullValue,
                Mode = (Binding as Binding)?.Mode ?? BindingMode.Default,
                UpdateSourceTrigger = (Binding as Binding)?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default,
                Path = (Binding as Binding)?.Path
            });
            Grid.SetColumn(textBlock, 1);
            Grid.SetRow(textBlock, 0);
            Grid.SetColumnSpan(textBlock, 2);
            grid.Children.Add(textBlock);
        }

        return grid;
    }

    private BindingBase _binding;
    private BindingBase _levelBinding;
    private BindingBase _isExpandedBinding;
    private BindingBase _hasChildrenBinding;

    public BindingBase Binding
    {
        get => _binding;
        set
        {
            if (_binding == value)
                return;
            _binding = value;
            NotifyPropertyChanged(nameof(Binding));
        }
    }

    public BindingBase LevelBinding
    {
        get => _levelBinding;
        set
        {
            if (_levelBinding == value)
                return;
            _levelBinding = value;
            NotifyPropertyChanged(nameof(LevelBinding));
        }
    }

    public BindingBase IsExpandedBinding
    {
        get => _isExpandedBinding;
        set
        {
            if (_isExpandedBinding == value)
                return;
            _isExpandedBinding = value;
            NotifyPropertyChanged(nameof(IsExpandedBinding));
        }
    }

    public BindingBase HasChildrenBinding
    {
        get => _hasChildrenBinding;
        set
        {
            if (_hasChildrenBinding == value)
                return;
            _hasChildrenBinding = value;
            NotifyPropertyChanged(nameof(HasChildrenBinding));
        }
    }
}