using System.Collections.ObjectModel;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public partial class SettingsViewModel : ObservableObject
{
    private readonly BackendClient _backend;
    private readonly PersonContext _personContext;

    public ObservableCollection<AccentChipVm> AccentColors { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPerson1), nameof(IsPerson2))]
    private Person viewingPerson = Person.Person1;

    public bool IsPerson1 { get => ViewingPerson == Person.Person1; set { if (value) ViewingPerson = Person.Person1; } }
    public bool IsPerson2 { get => ViewingPerson == Person.Person2; set { if (value) ViewingPerson = Person.Person2; } }

    [ObservableProperty] private bool notificationsEnabled;
    [ObservableProperty] private bool darkMode;
    [ObservableProperty] private bool showCompletedTodos;
    [ObservableProperty] private string displayName = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string signature = "";
    [ObservableProperty] private string defaultDetailTemplate = "";
    [ObservableProperty] private TodoType defaultTodoType;
    [ObservableProperty] private TodoState defaultState;
    [ObservableProperty] private BackendUiEval.Grpc.Language language;
    [ObservableProperty] private int pageSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccentColorLabel))]
    private AccentColor accentColor;

    [ObservableProperty] private string userId = "";
    [ObservableProperty] private string accountCreated = "";
    [ObservableProperty] private int todoCount;
    [ObservableProperty] private string appVersion = "";
    [ObservableProperty] private string? status;
    [ObservableProperty] private bool isBusy;

    public string AccentColorLabel => $"Selected: {AccentColor}";

    public IReadOnlyList<TodoType> TodoTypes { get; } = new[] { TodoType.Shopping, TodoType.Work, TodoType.SpareTime };
    public IReadOnlyList<TodoState> TodoStates { get; } = new[] { TodoState.Ready, TodoState.InProgress, TodoState.Done };
    public IReadOnlyList<BackendUiEval.Grpc.Language> Languages { get; } = new[] { BackendUiEval.Grpc.Language.En, BackendUiEval.Grpc.Language.De, BackendUiEval.Grpc.Language.Fr, BackendUiEval.Grpc.Language.It };
    public IReadOnlyList<int> PageSizes { get; } = new[] { 10, 30, 50, 100 };

    public SettingsViewModel()
    {
        _backend = App.Services.GetRequiredService<BackendClient>();
        _personContext = App.Services.GetRequiredService<PersonContext>();
        ViewingPerson = _personContext.Current;

        foreach (var c in (AccentColor[])new[]
        {
            AccentColor.Blue, AccentColor.Green, AccentColor.Red, AccentColor.Orange,
            AccentColor.Purple, AccentColor.Teal, AccentColor.Pink,
        })
        {
            AccentColors.Add(new AccentChipVm(c, this));
        }
    }

    public void SelectAccent(AccentColor c)
    {
        AccentColor = c;
        _personContext.Current = ViewingPerson;
        foreach (var chip in AccentColors) chip.RefreshSelected();
    }

    partial void OnAccentColorChanged(AccentColor value)
    {
        foreach (var chip in AccentColors) chip.RefreshSelected();
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
    public async Task LoadAsync()
    {
        try
        {
            ClearFields();
            IsBusy = true;
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
        finally
        {
            IsBusy = false;
        }
    }

    public IAsyncRelayCommand ReloadCommand => LoadCommand;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            Status = "Saving…";
            var s = new BackendUiEval.Grpc.Settings
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
            Status = $"Saved settings for {ViewingPerson}.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public partial class AccentChipVm : ObservableObject
{
    private readonly SettingsViewModel _owner;

    public AccentColor Value { get; }
    public string Name => Value.ToString();
    public Brush Brush { get; }

    [ObservableProperty] private Brush strokeBrush = new SolidColorBrush(Colors.LightGray);
    [ObservableProperty] private double strokeThickness = 1.0;

    public AccentChipVm(AccentColor value, SettingsViewModel owner)
    {
        Value = value;
        _owner = owner;
        Brush = new SolidColorBrush(HexToColor(ColorHex(value)));
    }

    public void RefreshSelected()
    {
        var selected = _owner.AccentColor == Value;
        StrokeBrush = new SolidColorBrush(selected ? Colors.Black : Colors.LightGray);
        StrokeThickness = selected ? 4.0 : 1.0;
    }

    private static string ColorHex(AccentColor c) => c switch
    {
        AccentColor.Blue => "#3F6BD8",
        AccentColor.Green => "#3CAE5C",
        AccentColor.Red => "#D84A4A",
        AccentColor.Orange => "#E8843C",
        AccentColor.Purple => "#8B5CD8",
        AccentColor.Teal => "#2EA39A",
        AccentColor.Pink => "#D85C9A",
        _ => "#D1D5DB",
    };

    private static Windows.UI.Color HexToColor(string hex)
    {
        var h = hex.TrimStart('#');
        byte r = Convert.ToByte(h.Substring(0, 2), 16);
        byte g = Convert.ToByte(h.Substring(2, 2), 16);
        byte b = Convert.ToByte(h.Substring(4, 2), 16);
        return Windows.UI.Color.FromArgb(255, r, g, b);
    }
}
