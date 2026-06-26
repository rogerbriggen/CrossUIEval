using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaUiEval.Services;
using AvaloniaUiEval.ViewModels;
using AvaloniaUiEval.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaUiEval;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = BuildServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<ShellViewModel>(),
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = () => new ShellView
            {
                DataContext = Services.GetRequiredService<ShellViewModel>(),
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new ShellView
            {
                DataContext = Services.GetRequiredService<ShellViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<BackendClient>();
        services.AddSingleton<PersonContext>();
        services.AddSingleton<NavigationService>();

        services.AddTransient<NewTodoViewModel>();
        services.AddTransient<AllTodosViewModel>();
        services.AddTransient<PagedTodosViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<TodoDetailViewModel>();
        services.AddTransient<BackendViewModel>();
        services.AddTransient<ShellViewModel>();

        return services.BuildServiceProvider();
    }
}
