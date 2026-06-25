namespace UnoplatformUI.Presentation.Pages;

public sealed partial class NewTodoPage : Page
{
    public NewTodoPage()
    {
        this.InitializeComponent();
        DataContext = new NewTodoViewModel();
    }
}
