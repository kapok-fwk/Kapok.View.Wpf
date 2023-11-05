using System;
using System.Windows;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Data.EntityFrameworkCore;
using Kapok.Module;
using Kapok.View;
using Kapok.View.Wpf.AvalonDock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ToDoWpfApp.View;

namespace ToDoWpfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IHost Host { get; }

    public T GetService<T>()
        where T : class
    {
        if (Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices.");
        }

        return service;
    }

    public App()
    {
        InitializeComponent();

        ModuleEngine.InitiateModule(typeof(ToDoModule));

        DataDomain.DefaultDaoType = typeof(DeferredDao<>);

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;
            }).ConfigureServices((context, services) =>
            {
                // View logic
                services.AddSingleton<IViewDomain, WpfAvalonDockViewDomain>(serviceProvider => new WpfAvalonDockViewDomain(ShutdownApplication, serviceProvider));

                // Data logic
                services.AddSingleton<IDataDomain>(serviceProvider =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseInMemoryDatabase("ToDos");

                    var dataDomain = new EFCoreDataDomain(optionsBuilder.Options)
                    {
                        ServiceProvider = serviceProvider
                    };

                    return dataDomain;
                });
                services.TryAdd(ServiceDescriptor.Scoped<IDataDomainScope>(p => new EFCoreDataDomainScope(p.GetRequiredService<IDataDomain>(), p)));
                services.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(EFCoreRepository<>)));

                // Views and ViewModels
                services.AddTransient<MainPage>();

            }).Build();

        // initialize app
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Startup += App_Startup;
        Exit += App_Exit;
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        // start main window:
        var mainPage = GetService<MainPage>();

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
}