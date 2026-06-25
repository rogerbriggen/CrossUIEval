using System.Collections.ObjectModel;
using System.Windows.Input;
using BackendUiEval.Grpc;
using MauiUiEval.Services;
using MauiUiEval.Views;

namespace MauiUiEval.ViewModels;

public class PagedTodosViewModel : ViewModelBase
{
    private const int PageSize = 30;

    private readonly BackendClient _backend;
    private readonly object _loadLock = new();
    private bool _loadingMore;
    private int _total;

    public ObservableCollection<Todo> Todos { get; } = new();

    private bool _isRefreshing;
    public bool IsRefreshing { get => _isRefreshing; set => SetProperty(ref _isRefreshing, value); }

    private string? _status;
    public string? Status { get => _status; set => SetProperty(ref _status, value); }

    public ICommand RefreshCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand SelectCommand { get; }

    public PagedTodosViewModel(BackendClient backend)
    {
        _backend = backend;
        RefreshCommand = new Command(async () => await RefreshAsync());
        LoadMoreCommand = new Command(async () => await LoadMoreAsync());
        SelectCommand = new Command<Todo>(async t => await OpenDetailAsync(t));
    }

    public async Task RefreshAsync()
    {
        try
        {
            Todos.Clear();
            _total = 0;
            IsRefreshing = true;
            Status = "Loading…";
            var resp = await _backend.Todos.ListPagedAsync(new ListPagedRequest { Offset = 0, Limit = PageSize });
            foreach (var t in resp.Todos) Todos.Add(t);
            _total = resp.Total;
            Status = $"Refreshed {DateTime.Now:HH:mm:ss} — showing {Todos.Count} of {_total}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task LoadMoreAsync()
    {
        lock (_loadLock)
        {
            if (_loadingMore) return;
            if (Todos.Count >= _total && _total > 0) return;
            _loadingMore = true;
        }
        try
        {
            var resp = await _backend.Todos.ListPagedAsync(new ListPagedRequest { Offset = Todos.Count, Limit = PageSize });
            foreach (var t in resp.Todos) Todos.Add(t);
            _total = resp.Total;
            Status = $"Showing {Todos.Count} of {_total}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            lock (_loadLock) _loadingMore = false;
        }
    }

    private static async Task OpenDetailAsync(Todo? todo)
    {
        if (todo is null) return;
        await Shell.Current.GoToAsync(nameof(TodoDetailPage), new Dictionary<string, object> { ["Todo"] = todo });
    }
}
