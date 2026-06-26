using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Uno.Toolkit.UI;
using UnoplatformUI.Presentation.Pages;

namespace UnoplatformUI.Presentation;

public sealed partial class Shell : UserControl
{
    public Shell()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(NewTodoPage));
        MainTabBar.SelectedIndex = 0;
    }

    private void OnTabSelectionChanged(TabBar sender, TabBarSelectionChangedEventArgs args)
    {
        Type pageType = MainTabBar.SelectedIndex switch
        {
            0 => typeof(NewTodoPage),
            1 => typeof(AllTodosPage),
            2 => typeof(PagedTodosPage),
            3 => typeof(SettingsPage),
            4 => typeof(BackendPage),
            _ => typeof(NewTodoPage),
        };

        // Always-fresh navigation: each tap creates a new page instance, so
        // OnNavigatedTo fires and the page re-fetches from the backend.
        ContentFrame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
        ContentFrame.BackStack.Clear();
    }
}
