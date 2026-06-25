# CrossUIEval

Try out different frameworks for cross-platform ui.

## Backend endpoint configuration

Every client app has a **Backend** tab/page next to Settings. It shows the gRPC
host and port currently in use, lets you change them, and applies the new
endpoint immediately when you press **Save** — the gRPC channel is rebuilt
in place, and the next navigation to any other tab refetches from the new
backend. The value is persisted to
`%LOCALAPPDATA%/CrossUIEval/backend.json` (or the platform equivalent) so it
survives app restarts.

Defaults if no override is saved:

| Platform | Host | Port |
|---|---|---|
| Windows / macOS / iOS simulator / WASM / desktop | `localhost` | `5080` |
| Android emulator | `10.0.2.2` | `5080` |

Page locations:

- MAUI: [Views/BackendPage.xaml](MAUI/MauiUiEval/MauiUiEval/Views/BackendPage.xaml) + [ViewModels/BackendViewModel.cs](MAUI/MauiUiEval/MauiUiEval/ViewModels/BackendViewModel.cs)
- MAUI Blazor Hybrid: [Components/Pages/Backend.razor](MauiBlazor/MauiBlazorhybrid/MauiBlazorhybrid/Components/Pages/Backend.razor)
- Uno Platform: [Presentation/Pages/BackendPage.xaml](unoplatform/UnoplatformUI/UnoplatformUI/Presentation/Pages/BackendPage.xaml) + [BackendViewModel.cs](unoplatform/UnoplatformUI/UnoplatformUI/Presentation/Pages/BackendViewModel.cs)

The shared mechanism lives in each app's `BackendClient`:
[MAUI](MAUI/MauiUiEval/MauiUiEval/Services/BackendClient.cs),
[MauiBlazor](MauiBlazor/MauiBlazorhybrid/MauiBlazorhybrid/Services/BackendClient.cs),
[Uno](unoplatform/UnoplatformUI/UnoplatformUI/Services/BackendClient.cs).
`UpdateEndpoint(host, port)` disposes the current `GrpcChannel`, builds a new
one, rebinds the `Settings` / `Todos` service clients, and writes the new
values to `backend.json`. All call sites read `_backend.Settings` /
`_backend.Todos` per-call, so they automatically pick up the new channel.

See [AGENTS.md](AGENTS.md) for the full architecture overview.
