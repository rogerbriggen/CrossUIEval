using System;
using AvaloniaUiEval.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUiEval.ViewModels;

public partial class BackendViewModel : ViewModelBase
{
    private readonly BackendClient _backend;

    [ObservableProperty] private string _host = "";
    [ObservableProperty] private int _port;
    [ObservableProperty] private string _currentAddress = "";
    [ObservableProperty] private string? _status;

    public BackendViewModel(BackendClient backend)
    {
        _backend = backend;
        LoadFromClient();
    }

    [RelayCommand]
    public void Reload() => LoadFromClient();

    private void LoadFromClient()
    {
        Host = _backend.Host;
        Port = _backend.Port;
        CurrentAddress = _backend.Address;
        Status = null;
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            _backend.UpdateEndpoint(Host, Port);
            CurrentAddress = _backend.Address;
            Status = $"Saved. Backend is now {CurrentAddress}. Open any tab to see it in effect.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    public System.Threading.Tasks.Task OnActivatedAsync()
    {
        LoadFromClient();
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
