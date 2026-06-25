using Microsoft.UI.Xaml.Navigation;

namespace UnoplatformUI.Presentation.Pages;

public sealed partial class BackendPage : Page
{
    private readonly BackendViewModel _vm;

    public BackendPage()
    {
        this.InitializeComponent();
        _vm = new BackendViewModel();
        DataContext = _vm;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _vm.LoadFromClient();
    }
}
