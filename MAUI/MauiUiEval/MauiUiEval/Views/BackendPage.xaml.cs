using MauiUiEval.ViewModels;

namespace MauiUiEval.Views;

public partial class BackendPage : ContentPage
{
    private readonly BackendViewModel _vm;

    public BackendPage(BackendViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadFromClient();
    }
}
