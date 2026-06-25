using System.Collections.Concurrent;
using BackendUiEval.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace BackendUiEval.Services;

public class SettingsService : Grpc.SettingsService.SettingsServiceBase
{
    private static readonly ConcurrentDictionary<Person, Settings> Store = new(new[]
    {
        new KeyValuePair<Person, Settings>(Person.Person1, SeedPerson1()),
        new KeyValuePair<Person, Settings>(Person.Person2, SeedPerson2()),
    });

    public override Task<Settings> GetSettings(GetSettingsRequest request, ServerCallContext context)
    {
        if (request.Person is Person.Unspecified)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "person must be specified"));

        if (!Store.TryGetValue(request.Person, out var settings))
            throw new RpcException(new Status(StatusCode.NotFound, $"no settings for {request.Person}"));

        return Task.FromResult(settings.Clone());
    }

    public override Task<Settings> UpdateSettings(UpdateSettingsRequest request, ServerCallContext context)
    {
        if (request.Person is Person.Unspecified)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "person must be specified"));
        if (request.Settings is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "settings payload required"));

        var incoming = request.Settings.Clone();
        incoming.Person = request.Person;

        // preserve read-only fields from the seed so clients cannot overwrite them
        var current = Store.GetOrAdd(request.Person, _ => incoming);
        incoming.UserId = current.UserId;
        incoming.AccountCreated = current.AccountCreated;
        incoming.TodoCount = current.TodoCount;
        incoming.AppVersion = current.AppVersion;

        Store[request.Person] = incoming;
        return Task.FromResult(incoming.Clone());
    }

    private static Settings SeedPerson1() => new()
    {
        Person = Person.Person1,
        NotificationsEnabled = true,
        DarkMode = false,
        ShowCompletedTodos = true,
        DisplayName = "Roger",
        Email = "roger@example.com",
        Signature = "Sent from my todo app\n— Roger",
        DefaultDetailTemplate = "Priority:\nContext:\nNotes:",
        DefaultTodoType = TodoType.Work,
        DefaultState = TodoState.Ready,
        Language = Language.De,
        PageSize = 30,
        UserId = "p1-9f3e7c80-1d2a-4e6b-9c11-7b0e2a5f4d12",
        AccountCreated = Timestamp.FromDateTime(new DateTime(2024, 3, 12, 9, 0, 0, DateTimeKind.Utc)),
        TodoCount = 42,
        AppVersion = "1.0.0",
        AccentColor = AccentColor.Blue,
    };

    private static Settings SeedPerson2() => new()
    {
        Person = Person.Person2,
        NotificationsEnabled = false,
        DarkMode = true,
        ShowCompletedTodos = true,
        DisplayName = "Alex",
        Email = "alex@example.com",
        Signature = "",
        DefaultDetailTemplate = "TODO:",
        DefaultTodoType = TodoType.Shopping,
        DefaultState = TodoState.Ready,
        Language = Language.En,
        PageSize = 50,
        UserId = "p2-77ab1f44-8be0-4c33-bd9e-3a1c6b8f0e22",
        AccountCreated = Timestamp.FromDateTime(new DateTime(2025, 11, 1, 14, 30, 0, DateTimeKind.Utc)),
        TodoCount = 17,
        AppVersion = "1.0.0",
        AccentColor = AccentColor.Purple,
    };
}
