using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf;

public class PageTemplateSelector : DataTemplateSelector
{
    // TODO [memory leak] here we just use an cache but we never clear up this cache after an DataTemplate is not used anymore
    private readonly Dictionary<Type, DataTemplate> _dataTemplateCache = new();

    public override DataTemplate SelectTemplate(object? item, DependencyObject container)
    {
        if (item is IPage page)
        {
            Type type = item.GetType();

            if (_dataTemplateCache.ContainsKey(type))
                return _dataTemplateCache[type];

            var dataTemplate = BuildDataTemplate(page);
            if (dataTemplate != null)
            {
                _dataTemplateCache.Add(type, dataTemplate);
                return dataTemplate;
            }
        }

        return base.SelectTemplate(item, container);
    }
        
    private DataTemplate BuildDataTemplate(IPage page)
    {
        var controlType = page.ViewDomain.GetPageControlType(page.GetType());

        var dataTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            var constructor = controlType.GetConstructor(new Type[] {});
            var pageControl = constructor?.Invoke(null);
            return pageControl;
        }, false);

        return dataTemplate;
    }
}