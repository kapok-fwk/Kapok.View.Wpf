using System.ComponentModel.DataAnnotations;
using Kapok.Data;
using Kapok.View;

namespace ToDoWpfApp.View;

public class MainPage : InteractivePage
{
    private IDataDomain _dataDomain;

    public MainPage(IViewDomain? viewDomain, IDataDomain dataDomain)
        : base(viewDomain)
    {
        _dataDomain = dataDomain;

        Title = "Simple ToDo Application";

        // init commands
        OpenToDoListAction = new UIOpenPageAction("OpenToDoList", typeof(Tasks), ViewDomain);
    }

    [MenuItem, Display(Name = "Open ToDo List")]
    public IAction OpenToDoListAction { get; }

    protected override void OnClosed()
    {
        base.OnClosed();

        // close all open windows and end the application with exit code 1 (exit code OK)
        ViewDomain.ShutdownApplication?.Invoke(1);
    }
}