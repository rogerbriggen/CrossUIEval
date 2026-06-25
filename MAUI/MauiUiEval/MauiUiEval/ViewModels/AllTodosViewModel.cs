using System.Collections.ObjectModel;
using System.Windows.Input;
using BackendUiEval.Grpc;
using MauiUiEval.Services;
using MauiUiEval.Views;

namespace MauiUiEval.ViewModels;

public class AllTodosViewModel : ViewModelBase
{
    private readonly BackendClient _backend;

    public ObservableCollection<Todo> Todos { get; } = new();

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    private string? _status;
    public string? Status { get => _status; set => SetProperty(ref _status, value); }

    public ICommand RefreshCommand { get; }
    public ICommand SelectCommand { get; }

    public AllTodosViewModel(BackendClient backend)
    {
        _backend = backend;
        RefreshCommand = new Command(async () => await LoadAsync());
        SelectCommand = new Command<Todo>(async t => await OpenDetailAsync(t));
    }

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

    private static async Task OpenDetailAsync(Todo? todo)
    {
        if (todo is null) return;
        await Shell.Current.GoToAsync(nameof(TodoDetailPage), new Dictionary<string, object> { ["Todo"] = todo });
    }
}
