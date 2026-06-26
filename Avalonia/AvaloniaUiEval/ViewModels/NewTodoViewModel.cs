using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaUiEval.Services;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;

namespace AvaloniaUiEval.ViewModels;

public partial class NewTodoViewModel : ViewModelBase
{
    private readonly BackendClient _backend;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string _description = "";

    [ObservableProperty] private string _detail = "";
    [ObservableProperty] private TodoType _type = TodoType.Work;
    [ObservableProperty] private Person _person = Person.Person1;
    [ObservableProperty] private TodoState _state = TodoState.Ready;
    [ObservableProperty] private string? _status;

    public IReadOnlyList<TodoType> TodoTypes { get; } = new[] { TodoType.Shopping, TodoType.Work, TodoType.SpareTime };
    public IReadOnlyList<Person> Persons { get; } = new[] { Person.Person1, Person.Person2 };
    public IReadOnlyList<TodoState> TodoStates { get; } = new[] { TodoState.Ready, TodoState.InProgress, TodoState.Done };

    public NewTodoViewModel(BackendClient backend)
    {
        _backend = backend;
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

    public Task OnActivatedAsync() => Task.CompletedTask;
}
