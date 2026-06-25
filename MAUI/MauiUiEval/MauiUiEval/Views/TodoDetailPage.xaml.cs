using BackendUiEval.Grpc;
using MauiUiEval.ViewModels;

namespace MauiUiEval.Views;

public partial class TodoDetailPage : ContentPage, IQueryAttributable
{
    private readonly TodoDetailViewModel _vm;

    public TodoDetailPage(TodoDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Todo", out var t) && t is Todo todo)
            _vm.Todo = todo;
    }
}
