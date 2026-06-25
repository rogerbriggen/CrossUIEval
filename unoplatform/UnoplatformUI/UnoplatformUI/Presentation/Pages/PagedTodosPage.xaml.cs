using BackendUiEval.Grpc;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public sealed partial class PagedTodosPage : Page
{
    private readonly PagedTodosViewModel _vm;

    public PagedTodosPage()
    {
        this.InitializeComponent();
        _vm = new PagedTodosViewModel();
        DataContext = _vm;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await _vm.RefreshAsync();
    }

    private async void OnRefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        await _vm.RefreshAsync();
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Todo todo) return;
        App.Services.GetRequiredService<TodoSelectionService>().Selected = todo;
        Frame.Navigate(typeof(TodoDetailPage));
    }
}
