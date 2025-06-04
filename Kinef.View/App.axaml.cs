using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Kinef.Core.Extensions;
using Kinef.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kinef.View;

public partial class App : Application
{
    public T? GetService<T>() where T : notnull
    {
        T service = _host!.Services.GetRequiredService<T>();
        return service;
    }
    private IHost? _host;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _host = Host.CreateDefaultBuilder().ConfigureServices((services) =>
        {
            services.AddCoreServices();
        }).Build();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext = GetService<MainViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
        _host.StartAsync();
    }

    
}