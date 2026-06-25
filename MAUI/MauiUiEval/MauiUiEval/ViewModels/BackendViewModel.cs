using System.Windows.Input;
using MauiUiEval.Services;

namespace MauiUiEval.ViewModels;

public class BackendViewModel : ViewModelBase
{
    private readonly BackendClient _backend;

    private string _host = "";
    public string Host { get => _host; set => SetProperty(ref _host, value); }

    private int _port;
    public int Port { get => _port; set => SetProperty(ref _port, value); }

    private string _currentAddress = "";
    public string CurrentAddress { get => _currentAddress; set => SetProperty(ref _currentAddress, value); }

    private string? _status;
    public string? Status { get => _status; set => SetProperty(ref _status, value); }

    public ICommand SaveCommand { get; }
    public ICommand ReloadCommand { get; }

    public BackendViewModel(BackendClient backend)
    {
        _backend = backend;
        SaveCommand = new Command(Save);
        ReloadCommand = new Command(LoadFromClient);
        LoadFromClient();
    }

    public void LoadFromClient()
    {
        Host = _backend.Host;
        Port = _backend.Port;
        CurrentAddress = _backend.Address;
        Status = null;
    }

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
}
