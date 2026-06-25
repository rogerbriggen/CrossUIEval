using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public partial class BackendViewModel : ObservableObject
{
    private readonly BackendClient _backend;

    [ObservableProperty] private string host = "";
    [ObservableProperty] private int port;
    [ObservableProperty] private string currentAddress = "";
    [ObservableProperty] private string? status;

    public BackendViewModel()
    {
        _backend = App.Services.GetRequiredService<BackendClient>();
        LoadFromClient();
    }

    public void LoadFromClient()
    {
        Host = _backend.Host;
        Port = _backend.Port;
        CurrentAddress = _backend.Address;
        Status = null;
    }

    [RelayCommand]
    private void Reload() => LoadFromClient();

    [RelayCommand]
    private void Save()
    {
        try
        {
            _backend.UpdateEndpoint(Host, Port);
            CurrentAddress = _backend.Address;
            Status = $"Saved. Backend is now {CurrentAddress}. Open any other tab to see it in effect.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }
}
