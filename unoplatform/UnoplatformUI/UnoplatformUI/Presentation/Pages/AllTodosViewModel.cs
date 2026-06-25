using System.Collections.ObjectModel;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public partial class AllTodosViewModel : ObservableObject
{
    private readonly BackendClient _backend;

    public ObservableCollection<Todo> Todos { get; } = new();

    [ObservableProperty] private string? status;
    [ObservableProperty] private bool isBusy;

    public AllTodosViewModel()
    {
        _backend = App.Services.GetRequiredService<BackendClient>();
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
                if (t.Person == Person.Person1) Todos.Add(t);
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

    // Refresh is just LoadAsync — bound from the XAML Refresh button.
    public IAsyncRelayCommand RefreshCommand => LoadCommand;
}
