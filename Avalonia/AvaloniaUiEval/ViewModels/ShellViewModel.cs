using System;
using System.Collections.Generic;
using AvaloniaUiEval.Services;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaUiEval.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly NavigationService _navigation;
    private ViewModelBase? _previousPage;

    public IReadOnlyList<TabItem> Tabs { get; }

    private int _selectedTabIndex = -1;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value) && value >= 0 && value < Tabs.Count)
            {
                CurrentPage = ResolveTab(Tabs[value]);
                _previousPage = CurrentPage;
                _ = TryActivateAsync(CurrentPage);
            }
        }
    }

    [ObservableProperty] private ViewModelBase? _currentPage;

    public ShellViewModel(IServiceProvider services, NavigationService navigation)
    {
        _services = services;
        _navigation = navigation;
        Tabs = new[]
        {
            new TabItem("New", typeof(NewTodoViewModel)),
            new TabItem("All", typeof(AllTodosViewModel)),
            new TabItem("Paged", typeof(PagedTodosViewModel)),
            new TabItem("Settings", typeof(SettingsViewModel)),
            new TabItem("Backend", typeof(BackendViewModel)),
        };
        _navigation.DetailRequested += OnDetailRequested;
        _navigation.BackRequested += OnBackRequested;
        _navigation.TabRequested += (_, i) => SelectedTabIndex = i;
        SelectedTabIndex = 0;
    }

    private ViewModelBase ResolveTab(TabItem tab)
        => (ViewModelBase)_services.GetRequiredService(tab.ViewModelType);

    private void OnDetailRequested(object? sender, Todo todo)
    {
        var detail = _services.GetRequiredService<TodoDetailViewModel>();
        detail.Todo = todo;
        CurrentPage = detail;
    }

    private void OnBackRequested(object? sender, EventArgs e)
    {
        if (_previousPage is not null)
            CurrentPage = _previousPage;
    }

    private static async System.Threading.Tasks.Task TryActivateAsync(ViewModelBase? vm)
    {
        switch (vm)
        {
            case AllTodosViewModel a: await a.OnActivatedAsync(); break;
            case PagedTodosViewModel p: await p.OnActivatedAsync(); break;
            case SettingsViewModel s: await s.OnActivatedAsync(); break;
            case BackendViewModel b: await b.OnActivatedAsync(); break;
            case NewTodoViewModel n: await n.OnActivatedAsync(); break;
        }
    }
}

public record TabItem(string Title, Type ViewModelType);
