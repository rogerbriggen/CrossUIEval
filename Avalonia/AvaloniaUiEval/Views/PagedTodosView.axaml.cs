using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaUiEval.ViewModels;

namespace AvaloniaUiEval.Views;

public partial class PagedTodosView : UserControl
{
    private ScrollViewer? _scroll;

    public PagedTodosView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetached;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var list = this.FindControl<ListBox>("TodosListBox");
        _scroll = list?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (_scroll is not null)
            _scroll.ScrollChanged += OnScrollChanged;
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_scroll is not null)
            _scroll.ScrollChanged -= OnScrollChanged;
        _scroll = null;
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scroll is null) return;
        if (DataContext is not PagedTodosViewModel vm) return;

        var nearBottom = _scroll.Offset.Y + _scroll.Viewport.Height >= _scroll.Extent.Height - 200;
        if (nearBottom && vm.LoadMoreCommand.CanExecute(null))
            vm.LoadMoreCommand.Execute(null);
    }
}
