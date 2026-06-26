using System;
using BackendUiEval.Grpc;

namespace AvaloniaUiEval.Services;

public class NavigationService
{
    public event EventHandler<Todo>? DetailRequested;
    public event EventHandler? BackRequested;
    public event EventHandler<int>? TabRequested;

    public void NavigateToDetail(Todo todo) => DetailRequested?.Invoke(this, todo);
    public void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);
    public void SelectTab(int index) => TabRequested?.Invoke(this, index);
}
