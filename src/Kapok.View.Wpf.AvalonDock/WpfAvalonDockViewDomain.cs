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
}