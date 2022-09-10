using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf;

public class MenuItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? DefaultButtonTemplate { get; set; }
    public DataTemplate? ToggleButtonTemplate { get; set; }
    public DataTemplate? TableDataButtonTemplate { get; set; }
    public DataTemplate? MenuButtonTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item != null)
        {
            if (item is UIToggleMenuItemAction)
            {
                return ToggleButtonTemplate;
            }

            if (item.GetType().IsGenericType &&
                item.GetType().GetGenericTypeDefinition() == typeof(UIMenuItemDataSetSelectionAction<>))
            {
                return TableDataButtonTemplate;
            }

            if (item is UIMenuItem menuItem && (menuItem.SubMenuItems?.Count ?? 0) > 0)
            {
                return MenuButtonTemplate;
            }
        }

        return DefaultButtonTemplate;
    }
}