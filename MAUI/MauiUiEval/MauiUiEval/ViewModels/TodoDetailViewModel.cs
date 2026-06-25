using BackendUiEval.Grpc;

namespace MauiUiEval.ViewModels;

public class TodoDetailViewModel : ViewModelBase
{
    private Todo? _todo;
    public Todo? Todo { get => _todo; set => SetProperty(ref _todo, value); }
}
