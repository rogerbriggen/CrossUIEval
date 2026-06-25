using BackendUiEval.Grpc;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public sealed partial class AllTodosPage : Page
{
    private readonly AllTodosViewModel _vm;

    public AllTodosPage()
    {
        this.InitializeComponent();
        _vm = new AllTodosViewModel();
        DataContext = _vm;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await _vm.LoadAsync();
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Todo todo) return;
        App.Services.GetRequiredService<TodoSelectionService>().Selected = todo;
        Frame.Navigate(typeof(TodoDetailPage));
    }
}
