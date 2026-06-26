using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaUiEval.Services;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUiEval.ViewModels;

public partial class AllTodosViewModel : ViewModelBase
{
    private readonly BackendClient _backend;
    private readonly NavigationService _navigation;

    public ObservableCollection<Todo> Todos { get; } = new();

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _status;

    private Todo? _selectedTodo;
    public Todo? SelectedTodo
    {
        get => _selectedTodo;
        set
        {
            if (SetProperty(ref _selectedTodo, value) && value is not null)
            {
                _navigation.NavigateToDetail(value);
                SetProperty(ref _selectedTodo, null, nameof(SelectedTodo));
            }
        }
    }

    public AllTodosViewModel(BackendClient backend, NavigationService navigation)
    {
        _backend = backend;
        _navigation = navigation;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            Todos.Clear();
            IsBusy = true;
            Status = "Loading…";
            var resp = await _backend.Todos.ListAllAsync(new ListAllRequest());
            foreach (var t in resp.Todos)
                if (t.Person == Person.Person1)
                    Todos.Add(t);
            Status = $"Refreshed {DateTime.Now:HH:mm:ss} — {Todos.Count} todos for Person1 (of {resp.Total} total).";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task OnActivatedAsync() => LoadAsync();
}
