using MauiUiEval.ViewModels;

namespace MauiUiEval.Views;

public partial class PagedTodosPage : ContentPage
{
    private readonly PagedTodosViewModel _vm;

    public PagedTodosPage(PagedTodosViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshAsync();
    }
}
