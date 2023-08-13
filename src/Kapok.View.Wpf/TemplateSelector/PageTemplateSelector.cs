using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf;

public class PageTemplateSelector : DataTemplateSelector
{
    private readonly ConditionalWeakTable<Type, DataTemplate> _dataTemplateCache = new();

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is IPage page)
        {
            Type type = item.GetType();

            if (_dataTemplateCache.TryGetValue(type, out var dataTemplate))
                return dataTemplate;

            dataTemplate = BuildDataTemplate(page);
            _dataTemplateCache.Add(type, dataTemplate);
            return dataTemplate;
        }

        return base.SelectTemplate(item, container);
    }

    private DataTemplate BuildDataTemplate(IPage page)
    {
        var controlType = page.ViewDomain.GetPageControlType(page.GetType());

        if (page.ViewDomain is not WpfViewDomain wpfViewDomain)
            throw new NotSupportedException($"The page must use a view domain assignable to {typeof(WpfViewDomain).FullName} when used with {nameof(PageTemplateSelector)}");

        var viewDomainRef = new WeakReference<WpfViewDomain>(wpfViewDomain);

        var dataTemplate = TemplateGenerator.CreateDataTemplate(() =>
        {
            var constructor = controlType.GetConstructor(new Type[] {});
            Debug.Assert(constructor != null);
            var pageControl = constructor.Invoke(null);

            if (viewDomainRef.TryGetTarget(out var viewDomain) &&
                pageControl is ContentControl contentControl)
            {
                LoadedWeakEventManager.AddHandler(contentControl, ContentControl_Loaded);
                UnloadedWeakEventManager.AddHandler(contentControl, ContentControl_Unloaded);
            }

            return pageControl;
        }, false);

        return dataTemplate;
    }

    private void ContentControl_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ContentControl contentControl)
        {
            if (contentControl.DataContext is IPage page &&
                page.ViewDomain is WpfViewDomain wpfViewDomain)
            {
                wpfViewDomain.RegisterPageContentControl(page, contentControl);
            }
        }
    }

    private void ContentControl_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ContentControl contentControl)
        {
            if (contentControl.DataContext is IPage page &&
                page.ViewDomain is WpfViewDomain wpfViewDomain)
            {
                wpfViewDomain.RemovePageContentControl(page);
            }
        }
    }
}