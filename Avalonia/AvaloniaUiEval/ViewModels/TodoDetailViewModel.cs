using AvaloniaUiEval.Services;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUiEval.ViewModels;

public partial class TodoDetailViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;

    [ObservableProperty] private Todo? _todo;

    public TodoDetailViewModel(NavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void Back() => _navigation.GoBack();
}
