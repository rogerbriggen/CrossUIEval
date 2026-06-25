using MauiUiEval.ViewModels;

namespace MauiUiEval.Views;

public partial class AllTodosPage : ContentPage
{
    private readonly AllTodosViewModel _vm;

    public AllTodosPage(AllTodosViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
