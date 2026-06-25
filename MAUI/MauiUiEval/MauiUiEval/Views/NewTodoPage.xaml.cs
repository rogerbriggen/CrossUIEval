using MauiUiEval.ViewModels;

namespace MauiUiEval.Views;

public partial class NewTodoPage : ContentPage
{
    public NewTodoPage(NewTodoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
