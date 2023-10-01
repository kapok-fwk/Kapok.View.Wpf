using System.ComponentModel.DataAnnotations;
using Kapok.Data;
using Kapok.View;
using Kapok.View.Wpf.AvalonDock;

namespace ToDoWpfApp.View;

public class MainPage : DocumentPageCollectionPage
{
    private IDataDomain _dataDomain;

    public MainPage(IViewDomain? viewDomain, IDataDomain dataDomain)
        : base(viewDomain)
    {
        _dataDomain = dataDomain;

        Title = "Simple ToDo Application";

        // this is the menu for the navigation bar
        AddMenu("Main");

        // init commands
        OpenToDoListAction = new UIOpenPageAction("OpenToDoList", typeof(Tasks), ViewDomain);
        OpenTestPageAction = new UIOpenPageAction("OpenTestPage", typeof(TestPage), ViewDomain);
    }

    protected override void OnLoading()
    {
        base.OnLoading();

        // Set name of ribbon menu which is visible when no current page is selected.
        Menu[UIMenu.BaseMenuName].MenuItems[0] = new UIMenuItemTab("App")
        {
            Label = "App",
            Description = "Anwendung",
            IsVisible = true,
            RibbonKeyTip = "A"
        };
    }

    protected override void OnSelectedDocumentPageChanged(IPage? oldPage, IPage? page)
    {
        base.OnSelectedDocumentPageChanged(oldPage, page);

        // hide/make visible the ribbon menu which is dependent on the current page
        Menu[UIMenu.BaseMenuName].MenuItems[0].IsVisible = page == null;
        if (page == null && Menu[UIMenu.BaseMenuName].MenuItems[0] is UIMenuItemTab tabItem)
        {
            tabItem.IsSelected = true;
        }

        if (page != null && ViewDomain is WpfAvalonDockViewDomain wpfAvalonDockViewDomain)
        {
            wpfAvalonDockViewDomain.FocusDocumentPage(this, page);
        }
    }

    [MenuItem(MenuName = "Main"), Display(Name = "Open ToDo List")]
    public IAction OpenToDoListAction { get; }

    [MenuItem(MenuName = "Main"), Display(Name = "Test page")]
    public IAction OpenTestPageAction { get; }

    protected override void OnClosed()
    {
        base.OnClosed();

        // close all open windows and end the application with exit code 1 (exit code OK)
        ViewDomain.ShutdownApplication?.Invoke(1);
    }
}