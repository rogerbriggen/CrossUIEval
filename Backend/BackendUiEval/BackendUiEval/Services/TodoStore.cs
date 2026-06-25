using BackendUiEval.Grpc;
using Google.Protobuf.WellKnownTypes;

namespace BackendUiEval.Services;

public class TodoStore
{
    private const int SeedCount = 10000;

    private readonly object _lock = new();
    private readonly List<Todo> _todos;
    private int _nextId;

    public TodoStore()
    {
        _todos = Seed(SeedCount);
        _nextId = SeedCount + 1;
    }

    public Todo Add(Todo template)
    {
        var todo = template.Clone();
        lock (_lock)
        {
            todo.Id = _nextId++;
            if (todo.Date is null)
                todo.Date = Timestamp.FromDateTime(DateTime.UtcNow);
            _todos.Insert(0, todo);
            return todo.Clone();
        }
    }

    public (IReadOnlyList<Todo> page, int total) GetPaged(int offset, int limit)
    {
        if (offset < 0) offset = 0;
        if (limit <= 0) limit = 30;
        if (limit > 1000) limit = 1000;

        lock (_lock)
        {
            var total = _todos.Count;
            if (offset >= total) return (Array.Empty<Todo>(), total);
            var end = Math.Min(offset + limit, total);
            var slice = new List<Todo>(end - offset);
            for (var i = offset; i < end; i++) slice.Add(_todos[i].Clone());
            return (slice, total);
        }
    }

    public (IReadOnlyList<Todo> all, int total) GetAll()
    {
        lock (_lock)
        {
            var snapshot = new List<Todo>(_todos.Count);
            foreach (var t in _todos) snapshot.Add(t.Clone());
            return (snapshot, snapshot.Count);
        }
    }

    private static readonly string[] DescriptionsShopping =
    {
        "Buy milk", "Pick up bread", "Order coffee beans", "Grab apples",
        "Restock olive oil", "Get dish soap", "Buy birthday card", "Replace toothbrush",
    };

    private static readonly string[] DescriptionsWork =
    {
        "Review PR", "Write release notes", "Sync with team",
        "Update dashboard", "Prepare slides", "Fix flaky test", "Triage backlog", "Refactor module",
    };

    private static readonly string[] DescriptionsSpareTime =
    {
        "Read a chapter", "Go for a run", "Practice guitar", "Call a friend",
        "Plan weekend trip", "Watch documentary", "Bake bread", "Hike local trail",
    };

    private static readonly string[] DetailTemplates =
    {
        "Don't forget to check the list twice.",
        "Blocked on input from the other person.",
        "Should take about 30 minutes once started.",
        "Low priority but worth tracking.",
        "Follow-up item from earlier this week.",
    };

    private static List<Todo> Seed(int count)
    {
        var rng = new Random(42);
        var list = new List<Todo>(count);
        // Newest first: item 0 is "now", item N is ~N hours older.
        var now = DateTime.UtcNow;
        for (var i = 0; i < count; i++)
        {
            var type = (Grpc.TodoType)(rng.Next(3) + 1);          // SHOPPING/WORK/SPARE_TIME
            var person = (Grpc.Person)(rng.Next(2) + 1);          // PERSON1/PERSON2
            var state = (Grpc.TodoState)(rng.Next(3) + 1);        // READY/IN_PROGRESS/DONE
            var descriptions = type switch
            {
                Grpc.TodoType.Shopping => DescriptionsShopping,
                Grpc.TodoType.Work => DescriptionsWork,
                _ => DescriptionsSpareTime,
            };
            var description = descriptions[rng.Next(descriptions.Length)];
            var detail = DetailTemplates[rng.Next(DetailTemplates.Length)];

            list.Add(new Todo
            {
                Id = i + 1,
                Date = Timestamp.FromDateTime(now.AddHours(-i).ToUniversalTime()),
                Description = description,
                Detail = detail,
                Type = type,
                Person = person,
                State = state,
            });
        }
        return list;
    }
}
