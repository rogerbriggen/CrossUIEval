using System.Collections.ObjectModel;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public partial class PagedTodosViewModel : ObservableObject
{
    private const int PageSize = 30;

    private readonly BackendClient _backend;
    private int _total;

    public ObservableCollection<Todo> Todos { get; } = new();

    [ObservableProperty] private string? status;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoadMoreVisibility), nameof(LoadMoreLabel))]
    private bool canLoadMore;

    public Visibility LoadMoreVisibility => CanLoadMore ? Visibility.Visible : Visibility.Collapsed;
    public string LoadMoreLabel => $"Load more ({Todos.Count}/{_total})";

    public PagedTodosViewModel()
    {
        _backend = App.Services.GetRequiredService<BackendClient>();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        try
        {
            Todos.Clear();
            _total = 0;
            CanLoadMore = false;
            IsBusy = true;
            Status = "Loading…";
            var resp = await _backend.Todos.ListPagedAsync(new ListPagedRequest { Offset = 0, Limit = PageSize });
            foreach (var t in resp.Todos) Todos.Add(t);
            _total = resp.Total;
            CanLoadMore = Todos.Count < _total;
            Status = $"Refreshed {DateTime.Now:HH:mm:ss} — showing {Todos.Count} of {_total}.";
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

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var resp = await _backend.Todos.ListPagedAsync(new ListPagedRequest { Offset = Todos.Count, Limit = PageSize });
            foreach (var t in resp.Todos) Todos.Add(t);
            _total = resp.Total;
            CanLoadMore = Todos.Count < _total;
            OnPropertyChanged(nameof(LoadMoreLabel));
            Status = $"Showing {Todos.Count} of {_total}.";
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
}
