using System.Windows;
using AvalonDock;
using AvalonDock.Layout;

namespace Kapok.View.Wpf.AvalonDock;

public class WpfAvalonDockViewDomain : WpfViewDomain
{
    public WpfAvalonDockViewDomain(Action<int> shutdownApplicationAction, IServiceProvider? serviceProvider = null)
        : base(shutdownApplicationAction, serviceProvider)
    {
        DefaultCardPageWindow = typeof(CardPageWindow);
        DefaultListPageWindow = typeof(ListPageWindow);
        DefaultPopupListPageWindow = typeof(PopupListPageWindow);
    }

    public virtual void FocusDocumentPage(DocumentPageCollectionPage hostPage, IPage documentPage)
    {
        var window = GetOwnerWindow(hostPage);

        var dockingManager = window.FindVisualChildren<DockingManager>()?.FirstOrDefault() ?? throw new NotSupportedException("Could not get the avalon dock DockingManager");

        var firstDocumentPane = dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
        if (firstDocumentPane != null)
        {
            var documentPageLayoutDocument = firstDocumentPane.Children.FirstOrDefault(layoutDocument => layoutDocument.Content == documentPage);

            // We cannot set the view model but must set the LayoutDocument object
            dockingManager.ActiveContent = documentPageLayoutDocument;
        }
    }
}