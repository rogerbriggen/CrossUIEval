using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaUiEval.Services;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUiEval.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly BackendClient _backend;
    private readonly PersonContext _personContext;

    private Person _viewingPerson = Person.Person1;
    public Person ViewingPerson
    {
        get => _viewingPerson;
        set
        {
            if (SetProperty(ref _viewingPerson, value))
            {
                _personContext.Current = value;
                _ = LoadAsync();
            }
        }
    }

    [ObservableProperty] private bool _notificationsEnabled;
    [ObservableProperty] private bool _darkMode;
    [ObservableProperty] private bool _showCompletedTodos;
    [ObservableProperty] private string _displayName = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _signature = "";
    [ObservableProperty] private string _defaultDetailTemplate = "";
    [ObservableProperty] private TodoType _defaultTodoType;
    [ObservableProperty] private TodoState _defaultState;
    [ObservableProperty] private Language _language;
    [ObservableProperty] private int _pageSize;

    private AccentColor _accentColor;
    public AccentColor AccentColor
    {
        get => _accentColor;
        set
        {
            if (SetProperty(ref _accentColor, value))
            {
                foreach (var chip in AccentChips) chip.RefreshSelected(value);
            }
        }
    }

    [ObservableProperty] private string _userId = "";
    [ObservableProperty] private string _accountCreated = "";
    [ObservableProperty] private int _todoCount;
    [ObservableProperty] private string _appVersion = "";
    [ObservableProperty] private string? _status;

    public IReadOnlyList<Person> Persons { get; } = new[] { Person.Person1, Person.Person2 };
    public IReadOnlyList<TodoType> TodoTypes { get; } = new[] { TodoType.Shopping, TodoType.Work, TodoType.SpareTime };
    public IReadOnlyList<TodoState> TodoStates { get; } = new[] { TodoState.Ready, TodoState.InProgress, TodoState.Done };
    public IReadOnlyList<Language> Languages { get; } = new[] { Language.En, Language.De, Language.Fr, Language.It };
    public IReadOnlyList<int> PageSizes { get; } = new[] { 10, 30, 50, 100 };

    public ObservableCollection<AccentChipVm> AccentChips { get; } = new();

    public SettingsViewModel(BackendClient backend, PersonContext personContext)
    {
        _backend = backend;
        _personContext = personContext;
        foreach (var c in new[] {
            AccentColor.Blue, AccentColor.Green, AccentColor.Red, AccentColor.Orange,
            AccentColor.Purple, AccentColor.Teal, AccentColor.Pink })
        {
            AccentChips.Add(new AccentChipVm(c));
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            ClearFields();
            Status = "Loading…";
            var s = await _backend.Settings.GetSettingsAsync(new GetSettingsRequest { Person = ViewingPerson });
            NotificationsEnabled = s.NotificationsEnabled;
            DarkMode = s.DarkMode;
            ShowCompletedTodos = s.ShowCompletedTodos;
            DisplayName = s.DisplayName;
            Email = s.Email;
            Signature = s.Signature;
            DefaultDetailTemplate = s.DefaultDetailTemplate;
            DefaultTodoType = s.DefaultTodoType;
            DefaultState = s.DefaultState;
            Language = s.Language;
            PageSize = s.PageSize;
            AccentColor = s.AccentColor;
            UserId = s.UserId;
            AccountCreated = s.AccountCreated?.ToDateTime().ToString("yyyy-MM-dd") ?? "";
            TodoCount = s.TodoCount;
            AppVersion = s.AppVersion;
            Status = $"Refreshed {DateTime.Now:HH:mm:ss} — settings for {ViewingPerson}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    private void ClearFields()
    {
        NotificationsEnabled = false;
        DarkMode = false;
        ShowCompletedTodos = false;
        DisplayName = "";
        Email = "";
        Signature = "";
        DefaultDetailTemplate = "";
        DefaultTodoType = default;
        DefaultState = default;
        Language = default;
        PageSize = 0;
        AccentColor = default;
        UserId = "";
        AccountCreated = "";
        TodoCount = 0;
        AppVersion = "";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            Status = "Saving…";
            var s = new Settings
            {
                Person = ViewingPerson,
                NotificationsEnabled = NotificationsEnabled,
                DarkMode = DarkMode,
                ShowCompletedTodos = ShowCompletedTodos,
                DisplayName = DisplayName,
                Email = Email,
                Signature = Signature,
                DefaultDetailTemplate = DefaultDetailTemplate,
                DefaultTodoType = DefaultTodoType,
                DefaultState = DefaultState,
                Language = Language,
                PageSize = PageSize,
                AccentColor = AccentColor,
            };
            var updated = await _backend.Settings.UpdateSettingsAsync(new UpdateSettingsRequest { Person = ViewingPerson, Settings = s });
            TodoCount = updated.TodoCount;
            Status = $"Saved {DateTime.Now:HH:mm:ss} — settings for {ViewingPerson}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SelectAccentColor(AccentColor c) => AccentColor = c;

    public Task OnActivatedAsync() => LoadAsync();
}
