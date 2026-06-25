using BackendUiEval.Grpc;

namespace MauiBlazorhybrid.Services;

// Singleton handoff for the detail page — the All/Paged page sets Selected before
// navigating to /todo/detail; the detail page reads it on init.
// Blazor doesn't have MAUI's IQueryAttributable navigation params, and the backend
// has no GetById endpoint, so the cleanest mirror is to stash the message itself.
public class TodoSelectionService
{
    public Todo? Selected { get; set; }
}
