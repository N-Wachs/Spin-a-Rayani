using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SpinARayani.Core.Interfaces;
using SpinARayani.Core.Services;
using SpinARayani.UI.WPF.ViewModels;
using SpinARayani.UI.WPF.Views;

namespace SpinARayani.UI.WPF;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IRollService, RollService>();
        services.AddSingleton<ISaveService, SaveService>();
        services.AddSingleton<IGameService, GameService>();
        // ViewModels
        services.AddSingleton<MainViewModel>();
        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
        base.OnStartup(e);
    }
}
