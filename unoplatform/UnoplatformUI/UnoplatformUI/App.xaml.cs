using Uno.Resizetizer;
using UnoplatformUI.Presentation;
using UnoplatformUI.Services;

namespace UnoplatformUI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Services = new ServiceCollection()
            .AddSingleton<BackendClient>()
            .AddSingleton<PersonContext>()
            .AddSingleton<TodoSelectionService>()
            .BuildServiceProvider();

        MainWindow = new Window();

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        MainWindow.Content = new Shell();
        MainWindow.Activate();
    }
}
