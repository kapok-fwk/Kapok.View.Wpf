using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf.AvalonDock;

public class WpfAvalonDockViewDomain : WpfViewDomain
{
    public WpfAvalonDockViewDomain(Action<int> shutdownApplicationAction)
        : base(shutdownApplicationAction)
    {
        DefaultCardPageWindow = typeof(CardPageWindow);
        DefaultListPageWindow = typeof(ListPageWindow);
        DefaultPopupListPageWindow = typeof(PopupListPageWindow);
    }

    protected override ContentControl GetPageContentControlFromPageContainerPage(IPage page, IPage pageContainerPage)
    {
        if (!PageWpfWindows.ContainsKey(pageContainerPage))
            throw new ArgumentException("The page container page of page does not have an active, open window", nameof(pageContainerPage));

        var pageControlType = GetPageControlType(page.GetType());
        var avalonDockingManager = PageWpfWindows[pageContainerPage].FindVisualChild<global::AvalonDock.DockingManager>();
        return (ContentControl)avalonDockingManager.FindVisualChild(pageControlType);
    }
}