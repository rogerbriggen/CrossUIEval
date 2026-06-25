using System.Windows.Input;
using BackendUiEval.Grpc;
using Google.Protobuf.WellKnownTypes;
using MauiUiEval.Services;

namespace MauiUiEval.ViewModels;

public class NewTodoViewModel : ViewModelBase
{
    private readonly BackendClient _backend;

    private string _description = "";
    public string Description { get => _description; set { SetProperty(ref _description, value); CanSubmitChanged(); } }

    private string _detail = "";
    public string Detail { get => _detail; set => SetProperty(ref _detail, value); }

    private TodoType _type = TodoType.Work;
    public TodoType Type { get => _type; set => SetProperty(ref _type, value); }

    private Person _person = Person.Person1;
    public Person Person { get => _person; set => SetProperty(ref _person, value); }

    private TodoState _state = TodoState.Ready;
    public TodoState State { get => _state; set => SetProperty(ref _state, value); }

    private string? _status;
    public string? Status { get => _status; set => SetProperty(ref _status, value); }

    public IReadOnlyList<TodoType> TodoTypes { get; } = new[] { TodoType.Shopping, TodoType.Work, TodoType.SpareTime };
    public IReadOnlyList<Person> Persons { get; } = new[] { Person.Person1, Person.Person2 };
    public IReadOnlyList<TodoState> TodoStates { get; } = new[] { TodoState.Ready, TodoState.InProgress, TodoState.Done };

    public ICommand SubmitCommand { get; }

    public NewTodoViewModel(BackendClient backend)
    {
        _backend = backend;
        SubmitCommand = new Command(async () => await SubmitAsync(), CanSubmit);
    }

    private bool CanSubmit() => !string.IsNullOrWhiteSpace(Description);
    private void CanSubmitChanged() => ((Command)SubmitCommand).ChangeCanExecute();

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
