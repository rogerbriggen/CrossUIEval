using BackendUiEval.Grpc;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly SettingsViewModel _vm;

    public SettingsPage()
    {
        this.InitializeComponent();
        _vm = new SettingsViewModel();
        DataContext = _vm;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await _vm.LoadAsync();
    }

    private async void OnPersonToggle(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn || btn.Tag is not string tag) return;
        var newPerson = tag == "Person1" ? Person.Person1 : Person.Person2;
        if (_vm.ViewingPerson == newPerson)
        {
            btn.IsChecked = true; // keep at least one toggle on
            return;
        }
        _vm.ViewingPerson = newPerson;
        await _vm.LoadAsync();
    }

    private void OnChipTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not AccentChipVm chip) return;
        _vm.SelectAccent(chip.Value);
    }
}
