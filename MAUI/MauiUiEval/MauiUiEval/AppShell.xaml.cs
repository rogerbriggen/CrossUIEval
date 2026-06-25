using MauiUiEval.Views;

namespace MauiUiEval;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TodoDetailPage), typeof(TodoDetailPage));
    }
}
