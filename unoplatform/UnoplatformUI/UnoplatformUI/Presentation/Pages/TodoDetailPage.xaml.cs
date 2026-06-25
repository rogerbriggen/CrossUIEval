using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public sealed partial class TodoDetailPage : Page
{
    public TodoDetailPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var todo = App.Services.GetRequiredService<TodoSelectionService>().Selected;
        if (todo is null)
        {
            DetailPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            NoSelectionText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            return;
        }

        IdText.Text = todo.Id.ToString();
        DateText.Text = todo.Date.ToDateTime().ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        TypeText.Text = todo.Type.ToString();
        PersonText.Text = todo.Person.ToString();
        StateText.Text = todo.State.ToString();
        DescriptionText.Text = todo.Description;
        DetailText.Text = todo.Detail;
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }
}
