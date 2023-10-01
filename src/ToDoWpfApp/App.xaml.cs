using System.Windows;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Data.EntityFrameworkCore;
using Kapok.Module;
using Kapok.View;
using Kapok.View.Wpf.AvalonDock;
using Microsoft.EntityFrameworkCore;
using ToDoWpfApp.View;

namespace ToDoWpfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IViewDomain? _viewDomain;
    private IDataDomain? _dataDomain;

    public App()
    {
        // initialize app
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Startup += App_Startup;
        Exit += App_Exit;
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        //Logger.Info("Start application");
        // initializes the view domain
        _viewDomain = InitializeViewDomain();

        // load internal modules
        ModuleEngine.InitiateModule(typeof(ToDoModule));

        // initializes the data domain(s)
        _dataDomain = InitializeDataDomain();

        // start main window:
        var mainPage = new MainPage(_viewDomain, _dataDomain);

        //Logger.Info("Open main window");
        mainPage.Show();
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        Startup -= App_Startup;
        Exit -= App_Exit;
    }

    private static void ShutdownApplication(int exitCode)
    {
        //Logger.Info("Shutdown application with exit code {=exitCode}", exitCode);
        Application.Current.Shutdown(exitCode);
    }

    private static IViewDomain InitializeViewDomain()
    {
        //Logger.Info("Initiate view domain");
        DataDomain.DefaultDaoType = typeof(DeferredDao<>);

        return new WpfAvalonDockViewDomain(ShutdownApplication);
    }

    private static IDataDomain InitializeDataDomain()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseInMemoryDatabase("ToDos");

        var dataDomain = new EFCoreDataDomain(optionsBuilder.Options);

        return dataDomain;
    }
}