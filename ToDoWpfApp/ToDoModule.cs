using Kapok.Core;
using Kapok.View;
using Kapok.View.Wpf;
using ToDoWpfApp.DataModel;
using ToDoWpfApp.View;
using ToDoWpfApp.ViewWpf;

namespace ToDoWpfApp;

public class ToDoModule : ModuleBase
{
    public ToDoModule() : base(nameof(ToDoModule))
    {
        // data model registration
        DataDomain.RegisterEntity<Task>();
        DataDomain.RegisterEntity<TaskList>();

        // register default pages for data models
        ViewDomain.RegisterEntityDefaultPage<Task>(typeof(Tasks));

        // register detail page controls
        WpfViewDomain.RegisterPageWpfControlType<MainPage>(typeof(MainPageWindow));

        // override default window constructor
        WpfViewDomain.RegisterPageWpfWindowConstructor<MainPage>(() => new MainPageWindow());
    }
}