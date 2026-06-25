using BackendUiEval.Grpc;

namespace UnoplatformUI.Services;

public class PersonContext
{
    private Person _current = Person.Person1;

    public Person Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            _current = value;
            Changed?.Invoke(this, value);
        }
    }

    public event EventHandler<Person>? Changed;
}
