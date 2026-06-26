using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaUiEval.Services;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUiEval.ViewModels;

public partial class PagedTodosViewModel : ViewModelBase
{
    private const int PageSize = 30;

    private readonly BackendClient _backend;
    private readonly NavigationService _navigation;
    private readonly object _loadLock = new();
    private bool _loadingMore;
    private int _total;

    public ObservableCollection<Todo> Todos { get; } = new();

    [ObservableProperty] private bool _isRefreshing;
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

    public PagedTodosViewModel(BackendClient backend, NavigationService navigation)
    {
        _backend = backend;
        _navigation = navigation;
    }

    [RelayCommand]
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

    [RelayCommand]
    public async Task LoadMoreAsync()
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

    public Task OnActivatedAsync() => RefreshAsync();
}
