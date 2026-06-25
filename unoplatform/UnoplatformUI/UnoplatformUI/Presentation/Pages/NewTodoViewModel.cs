using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using UnoplatformUI.Services;

namespace UnoplatformUI.Presentation.Pages;

public partial class NewTodoViewModel : ObservableObject
{
    private readonly BackendClient _backend;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string description = "";

    [ObservableProperty] private string detail = "";
    [ObservableProperty] private TodoType type = TodoType.Work;
    [ObservableProperty] private Person person = Person.Person1;
    [ObservableProperty] private TodoState state = TodoState.Ready;
    [ObservableProperty] private string? status;

    public IReadOnlyList<TodoType> TodoTypes { get; } = new[] { TodoType.Shopping, TodoType.Work, TodoType.SpareTime };
    public IReadOnlyList<Person> Persons { get; } = new[] { Person.Person1, Person.Person2 };
    public IReadOnlyList<TodoState> States { get; } = new[] { TodoState.Ready, TodoState.InProgress, TodoState.Done };

    public NewTodoViewModel()
    {
        _backend = App.Services.GetRequiredService<BackendClient>();
    }

    private bool CanSubmit() => !string.IsNullOrWhiteSpace(Description);

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        try
        {
            Status = "Submitting…";
            var created = await _backend.Todos.CreateTodoAsync(new Todo
            {
                Description = Description,
                Detail = Detail,
                Type = Type,
                Person = Person,
                State = State,
                Date = Timestamp.FromDateTime(DateTime.UtcNow),
            });
            Status = $"Created todo #{created.Id}.";
            Description = "";
            Detail = "";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }
}
