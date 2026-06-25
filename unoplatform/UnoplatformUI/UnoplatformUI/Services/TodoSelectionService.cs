using BackendUiEval.Grpc;

namespace UnoplatformUI.Services;

// Holds the Todo selected on All/Paged so the detail page can read it on navigate.
// No GetById endpoint, so we stash the message rather than the id.
public class TodoSelectionService
{
    public Todo? Selected { get; set; }
}
