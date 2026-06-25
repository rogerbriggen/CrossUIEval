using System.Windows.Input;
using BackendUiEval.Grpc;
using MauiUiEval.Services;

namespace MauiUiEval.ViewModels;

public class SettingsViewModel : ViewModelBase
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

    // bound to controls
    private bool _notificationsEnabled;
    public bool NotificationsEnabled { get => _notificationsEnabled; set => SetProperty(ref _notificationsEnabled, value); }

    private bool _darkMode;
    public bool DarkMode { get => _darkMode; set => SetProperty(ref _darkMode, value); }

    private bool _showCompletedTodos;
    public bool ShowCompletedTodos { get => _showCompletedTodos; set => SetProperty(ref _showCompletedTodos, value); }

    private string _displayName = "";
    public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

    private string _email = "";
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _signature = "";
    public string Signature { get => _signature; set => SetProperty(ref _signature, value); }

    private string _defaultDetailTemplate = "";
    public string DefaultDetailTemplate { get => _defaultDetailTemplate; set => SetProperty(ref _defaultDetailTemplate, value); }

    private TodoType _defaultTodoType;
    public TodoType DefaultTodoType { get => _defaultTodoType; set => SetProperty(ref _defaultTodoType, value); }

    private TodoState _defaultState;
    public TodoState DefaultState { get => _defaultState; set => SetProperty(ref _defaultState, value); }

    private Language _language;
    public Language Language { get => _language; set => SetProperty(ref _language, value); }

    private int _pageSize;
    public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }

    private AccentColor _accentColor;
    public AccentColor AccentColor { get => _accentColor; set => SetProperty(ref _accentColor, value); }

    // read-only
    private string _userId = "";
    public string UserId { get => _userId; set => SetProperty(ref _userId, value); }

    private string _accountCreated = "";
    public string AccountCreated { get => _accountCreated; set => SetProperty(ref _accountCreated, value); }

    private int _todoCount;
    public int TodoCount { get => _todoCount; set => SetProperty(ref _todoCount, value); }

    private string _appVersion = "";
    public string AppVersion { get => _appVersion; set => SetProperty(ref _appVersion, value); }

    private string? _status;
    public string? Status { get => _status; set => SetProperty(ref _status, value); }

    public IReadOnlyList<Person> Persons { get; } = new[] { Person.Person1, Person.Person2 };
    public IReadOnlyList<TodoType> TodoTypes { get; } = new[] { TodoType.Shopping, TodoType.Work, TodoType.SpareTime };
    public IReadOnlyList<TodoState> TodoStates { get; } = new[] { TodoState.Ready, TodoState.InProgress, TodoState.Done };
    public IReadOnlyList<Language> Languages { get; } = new[] { Language.En, Language.De, Language.Fr, Language.It };
    public IReadOnlyList<int> PageSizes { get; } = new[] { 10, 30, 50, 100 };
    public IReadOnlyList<AccentColor> AccentColors { get; } = new[]
    {
        AccentColor.Blue, AccentColor.Green, AccentColor.Red, AccentColor.Orange,
        AccentColor.Purple, AccentColor.Teal, AccentColor.Pink,
    };

    public ICommand SaveCommand { get; }
    public ICommand ReloadCommand { get; }
    public ICommand SelectAccentColorCommand { get; }

    public SettingsViewModel(BackendClient backend, PersonContext personContext)
    {
        _backend = backend;
        _personContext = personContext;
        SaveCommand = new Command(async () => await SaveAsync());
        ReloadCommand = new Command(async () => await LoadAsync());
        SelectAccentColorCommand = new Command<AccentColor>(c => AccentColor = c);
    }

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
            Status = $"Loaded settings for {ViewingPerson}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    private void ClearFields()
    {
        // Resets every bound field so the page goes empty before the next fetch.
        // ViewingPerson is intentionally NOT cleared — it drives which person we're loading.
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
            TodoCount = updated.TodoCount; // read-only fields come back from server
            Status = $"Saved settings for {ViewingPerson}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }
}
