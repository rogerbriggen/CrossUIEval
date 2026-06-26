using MauiUiEval.Services;
using MauiUiEval.ViewModels;
using MauiUiEval.Views;
using Microsoft.Extensions.Logging;

namespace MauiUiEval;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<BackendClient>();
        builder.Services.AddSingleton<PersonContext>();

        // ViewModels (transient — fresh state per navigation)
        builder.Services.AddTransient<NewTodoViewModel>();
        builder.Services.AddTransient<AllTodosViewModel>();
        builder.Services.AddTransient<PagedTodosViewModel>();
        builder.Services.AddTransient<TodoDetailViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<BackendViewModel>();

        // Pages
        builder.Services.AddTransient<NewTodoPage>();
        builder.Services.AddTransient<AllTodosPage>();
        builder.Services.AddTransient<PagedTodosPage>();
        builder.Services.AddTransient<TodoDetailPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<BackendPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
