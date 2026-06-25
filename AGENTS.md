# AGENTS.md

This file provides instructions for Claude Code, Github Copilot, Codex (and other AI assistants) working with this repository.

The goal of this solution is to create several apps to test out cross-platform UI.

We create a todo app.

## Backend

The backend is a dotnet 10 ASP.NET Core host (project type `Microsoft.NET.Sdk.Web`) that exposes gRPC services. It is started as a command line app and listens on `http://localhost:5080` over HTTP/2 cleartext (h2c). State is in-memory only — restarting the host resets everything to the seed.

Project: [Backend/BackendUiEval/BackendUiEval/BackendUiEval.csproj](Backend/BackendUiEval/BackendUiEval/BackendUiEval.csproj).

Run it with:

```powershell
dotnet run --project Backend/BackendUiEval/BackendUiEval/BackendUiEval.csproj
```

### Proto layout

All `.proto` files live under `Backend/BackendUiEval/BackendUiEval/Protos/` and share the C# namespace `BackendUiEval.Grpc` so client code sees a single namespace. The csproj sets `ProtoRoot="Protos"` so cross-file imports use bare filenames.

| File | Package | Contents |
|---|---|---|
| `common.proto` | `common` | Shared enums: `Person` (PERSON1/PERSON2), `TodoType` (SHOPPING/WORK/SPARE_TIME), `TodoState` (READY/IN_PROGRESS/DONE) |
| `settings.proto` | `settings` | `SettingsService`, `Settings`, `Language`, `AccentColor` |
| `todos.proto` | `todos` | `TodoService`, `Todo`, `ListAllRequest`, `ListPagedRequest`, `ListTodosResponse` |

Proto enum naming note: each enum value is prefixed with the enum name (e.g. `PERSON_PERSON1`, not `PERSON1`) so the C# generator's prefix-stripping rule yields the expected names (`Person.Person1`, not `Person._1`).

### Endpoints

A todo consists of:

* `id` — int32, server-assigned (seed: 1..10000; ticker continues from 10001). Client-supplied ids on `CreateTodo` are ignored.
* `date` — `google.protobuf.Timestamp`, server-assigned to "now" if the client doesn't set it
* `description` — string
* `detail` — string (longer free text)
* `type` — `common.TodoType`: shopping, work, sparetime
* `person` — `common.Person`: person1, person2
* `state` — `common.TodoState`: ready, inProgress, done (defaults to `READY` if unset on create)

All todos share a single list. Newest first.

The store is seeded with 10000 deterministically generated todos (`Random(42)`) so each restart produces the same sequence. Server gRPC max message size is bumped to 16MB so `ListAll` of 10k todos fits comfortably.

#### New todo

`rpc CreateTodo (Todo) returns (Todo)` — the returned `Todo` carries the server-assigned `id` and `date`. Validation: `description`, `person`, and `type` are required; unspecified `state` becomes `READY`.

##### Todos all in one

`rpc ListAll (ListAllRequest) returns (ListTodosResponse)` — returns every todo, newest first. The UI filters by person client-side.

##### Todos paged

`rpc ListPaged (ListPagedRequest{ offset, limit }) returns (ListTodosResponse)` — slice `[offset, offset+limit)`, newest first. `limit` is clamped to `[1, 1000]`; a `limit` of 0 or negative is treated as 30 (the default page size). `ListTodosResponse.total` is the current total — useful for end-of-list detection while paging.

A `BackgroundService` (`TodoTickerService`) inserts 5 new todos at the top of the list every 10 seconds. The descriptions are prefixed with `"Auto: "` so they're easy to spot in the UI while testing pull-to-refresh.

##### Settings

Settings are stored per person (Person1 / Person2). Each field maps to a specific UI control on the Settings page so every widget has live data to bind to. Sample values below are the seed defaults used by the backend.

| UI control | Setting key | Type | Sample (Person1) | Sample (Person2) |
|---|---|---|---|---|
| Switch | `notificationsEnabled` | bool | true | false |
| Switch | `darkMode` | bool | false | true |
| Switch | `showCompletedTodos` | bool | true | true |
| Single-line text | `displayName` | string | "Roger" | "Alex" |
| Single-line text | `email` | string | "roger@example.com" | "alex@example.com" |
| Multi-line text | `signature` | string | "Sent from my todo app\n— Roger" | "" (empty — test empty state) |
| Multi-line text | `defaultDetailTemplate` | string | "Priority:\nContext:\nNotes:" | "TODO:" |
| Dropdown | `defaultTodoType` | enum {shopping, work, sparetime} | work | shopping |
| Dropdown | `defaultState` | enum {ready, inProgress, done} | ready | ready |
| Dropdown | `language` | enum {en, de, fr, it} | de | en |
| Dropdown | `pageSize` | int {10, 30, 50, 100} | 30 | 50 |
| Read-only | `userId` | string (guid) | "p1-9f3e…" | "p2-77ab…" |
| Read-only | `accountCreated` | timestamp | 2024-03-12 | 2025-11-01 |
| Read-only | `todoCount` | int | computed | computed |
| Read-only | `appVersion` | string | "1.0.0" | "1.0.0" |
| Horizontal scroll picker | `accentColor` | enum {blue, green, red, orange, purple, teal, pink} | blue | purple |

Design notes:
- The two persons differ on every control so swapping persons visibly changes the page.
- Person2's `signature` is intentionally empty to exercise empty-state rendering.
- Read-only fields are server-owned: the gRPC `UpdateSettings` call preserves them from the seed even if the client sends new values.

## Apps

We create several apps. All have the same functions.

Every page has a title.

We have a main navigation with 3 entries:
New
All
Paged
Settings

### New

You can enter a new todo with all fields

### All

You see all todos for Person1, all todos are fetched immediatly.
You see
first line: left: date - right: type
second line: description

You can click on a entry, then you go to a sub-page where you see all info.

### Paged

You only see 30 todos. If you scroll down, again 30 todos are fetched.
If you pull to refresh, also new todos are fetched.

You see
first line: left: date - right: type
second line: description

You can click on a entry, then you go to a sub-page where you see all info.

### Settings

To test different UI things.

We have a View, which you can scroll from left to right.
We have switch on and off
We can enter single line text
We can enter multi line text
We can select some items from a dropdown
We have some text just read-only.

### Frameworks

In the folder MAUI, we have a Maui App with dotnet 10. Project: [MAUI/MauiUiEval/MauiUiEval/MauiUiEval.csproj](MAUI/MauiUiEval/MauiUiEval/MauiUiEval.csproj). It targets net10.0-windows / -android / -ios / -maccatalyst.

#### Project layout

| Folder | Contents |
|---|---|
| `Services/` | [BackendClient](MAUI/MauiUiEval/MauiUiEval/Services/BackendClient.cs) (gRPC channel + service clients), [PersonContext](MAUI/MauiUiEval/MauiUiEval/Services/PersonContext.cs) (currently-selected person with change event) |
| `ViewModels/` | [ViewModelBase](MAUI/MauiUiEval/MauiUiEval/ViewModels/ViewModelBase.cs) (manual `INotifyPropertyChanged`), one VM per page |
| `Views/` | One `.xaml` + code-behind per page |
| `Converters/` | [AccentColorToBrushConverter](MAUI/MauiUiEval/MauiUiEval/Converters/AccentColorToBrushConverter.cs), [SelectedAccentConverter](MAUI/MauiUiEval/MauiUiEval/Converters/SelectedAccentConverter.cs) |
| `AppShell.xaml` | `TabBar` with the four nav entries; `TodoDetailPage` route registered in code-behind |
| `MauiProgram.cs` | DI wiring — `BackendClient`/`PersonContext` as singletons, every VM and page as transient |

#### Backend connection

- `http://localhost:5080` on Windows / macOS / iOS-simulator.
- `http://10.0.2.2:5080` on Android emulator (host loopback).
- The switch is compile-time `#if ANDROID` in [Services/BackendClient.cs](MAUI/MauiUiEval/MauiUiEval/Services/BackendClient.cs).
- That same file flips `System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport = true` so h2c works, and bumps `GrpcChannelOptions.MaxReceiveMessageSize` to 16 MB so `ListAll` of 10k todos fits.

#### Pages

| Tab | View | VM | What it does |
|---|---|---|---|
| New | [NewTodoPage](MAUI/MauiUiEval/MauiUiEval/Views/NewTodoPage.xaml) | [NewTodoViewModel](MAUI/MauiUiEval/MauiUiEval/ViewModels/NewTodoViewModel.cs) | Form: description, detail (multi-line), type / person / state pickers. `Create` is disabled until `Description` is non-empty. On success: status shows `Created todo #N.` and the description/detail fields are cleared. |
| All | [AllTodosPage](MAUI/MauiUiEval/MauiUiEval/Views/AllTodosPage.xaml) | [AllTodosViewModel](MAUI/MauiUiEval/MauiUiEval/ViewModels/AllTodosViewModel.cs) | `ListAll`, then client-side filter to Person1 only. Explicit Refresh button at the top (`RefreshView` swipe doesn't work on desktop). |
| Paged | [PagedTodosPage](MAUI/MauiUiEval/MauiUiEval/Views/PagedTodosPage.xaml) | [PagedTodosViewModel](MAUI/MauiUiEval/MauiUiEval/ViewModels/PagedTodosViewModel.cs) | `ListPaged(offset, 30)`; `RefreshView` for pull-to-refresh + explicit button; `CollectionView.RemainingItemsThreshold` drives `LoadMore` for infinite scroll. A `_loadingMore` flag under a `lock` prevents overlapping fetches when the threshold fires repeatedly. |
| Settings | [SettingsPage](MAUI/MauiUiEval/MauiUiEval/Views/SettingsPage.xaml) | [SettingsViewModel](MAUI/MauiUiEval/MauiUiEval/ViewModels/SettingsViewModel.cs) | Person carousel (see below), color chips, switches, Entry/Editor, Pickers, read-only block. Save / Reload buttons. |
| (sub-page) | [TodoDetailPage](MAUI/MauiUiEval/MauiUiEval/Views/TodoDetailPage.xaml) | [TodoDetailViewModel](MAUI/MauiUiEval/MauiUiEval/ViewModels/TodoDetailViewModel.cs) | Read-only view of every field. |

Tapping a row on All or Paged calls `Shell.Current.GoToAsync(nameof(TodoDetailPage), new Dictionary<string, object> { ["Todo"] = todo })`. The detail page implements `IQueryAttributable` and assigns `vm.Todo` from the dictionary — there's no `GetById` endpoint, so we pass the message itself.

#### Always-fresh data

Every page that fetches from the backend clears its bound state at the top of its load method and re-fetches on each `OnAppearing`. There is no "load once" cache:

- `AllTodosViewModel.LoadAsync` calls `Todos.Clear()` first.
- `PagedTodosViewModel.RefreshAsync` calls `Todos.Clear()` and resets `_total = 0` first.
- `SettingsViewModel.LoadAsync` calls a private `ClearFields()` that resets every bound field (switches → false, strings → "", enums → default, numbers → 0). `ViewingPerson` is intentionally **not** cleared — it's the *input* to the next fetch.
- The status label on each page is prefixed with `Refreshed HH:mm:ss — …` so it's visually obvious the call ran (otherwise a "10042 → 10047" total change after the ticker fires is easy to miss).

`NewTodoPage` and `TodoDetailPage` never fetch — they're write-only and read-from-navigation-parameter respectively.

#### Settings page specifics

**Person1 / Person2 carousel.** `Persons` is bound to a `CarouselView` with `CurrentItem` two-way-bound to `ViewingPerson`. The `ViewingPerson` setter calls `LoadAsync()` whenever it changes, so swiping the carousel triggers a fresh fetch for the other person. A small `IndicatorView` below shows which card is active.

**Accent color chips.** Original attempt used `CollectionView` with `SelectionMode="Single"` and a `VisualStateManager` — the click registered but the `Selected` visual state didn't apply on Windows. Replaced with `HorizontalStackLayout` + `BindableLayout`. Each chip is a `Border`:

- Its fill is the real accent color via [AccentColorToBrushConverter](MAUI/MauiUiEval/MauiUiEval/Converters/AccentColorToBrushConverter.cs) (`Blue → #3F6BD8`, etc.).
- Its `Stroke` and `StrokeThickness` come from a `MultiBinding` against `[chip value, VM.AccentColor]` through [SelectedAccentConverter](MAUI/MauiUiEval/MauiUiEval/Converters/SelectedAccentConverter.cs). When the two match → black 4px border. Otherwise → light-gray 1px.
- A `TapGestureRecognizer` calls `SettingsViewModel.SelectAccentColorCommand` with the chip's value. The VM command sets `AccentColor`; the binding round-trip updates every chip's border.

The page is named `x:Name="settingsPage"` because the chips reach the VM via `{Binding Source={x:Reference settingsPage}, Path=BindingContext.SelectAccentColorCommand}`. If you rename it, fix the chip refs.

#### XAML notes

- **`xmlns:grpc` assembly is `Shared.Grpc`**, not `MauiUiEval`. Every XAML that types-out a proto enum or message needs `xmlns:grpc="clr-namespace:BackendUiEval.Grpc;assembly=Shared.Grpc"`. Used in `AllTodosPage`, `PagedTodosPage`, `SettingsPage`.
- **`TodoDetailPage` is forced to runtime XAML inflation** via `<MauiXaml Update="Views\TodoDetailPage.xaml" Inflator="Runtime" />` in the csproj. The source generator emits `enum?`-to-`enum` assignments when walking bindings through a nullable `Todo?` root, which fails to compile. The other pages stay on SourceGen.
- **MultiBinding inputs are positional.** `SelectedAccentConverter.Convert(values, …)` reads `values[0]` as the chip's value and `values[1]` as the VM's selected `AccentColor`. The XAML order of `<Binding>` elements matches; don't reorder them.

## Shared library

Protos and generated client/server stubs are factored into a standalone class library so neither Backend nor MAUI has to run `Grpc.Tools` itself: [Shared/Shared.Grpc/Shared.Grpc.csproj](Shared/Shared.Grpc/Shared.Grpc.csproj).

- It owns `Protos/*.proto` and emits both client and server stubs (`GrpcServices="Both"`).
- Backend references it for the server stubs.
- MAUI references it for the client stubs.

This avoids a real problem: `Grpc.Tools` does generate `.cs` files into the MAUI Windows TFM's `obj/.../win-x64/` folder, but its `_Protobuf_Compile_BeforeCsCompile` hook fails to add them to `@Compile` in the multi-TFM MAUI build pipeline, so CSC reports the `BackendUiEval.Grpc` namespace as missing even though the code is on disk. Splitting the protos out into a normal class library sidesteps the issue entirely.

## MAUI BlazorHybrid client

Project: [MauiBlazor/MauiBlazorhybrid/MauiBlazorhybrid/MauiBlazorhybrid.csproj](MauiBlazor/MauiBlazorhybrid/MauiBlazorhybrid/MauiBlazorhybrid.csproj).

The MAUI shell is intentionally minimal — `MainPage.xaml` is just a `BlazorWebView` hosting the `Routes` root component. All UI lives in Razor pages under `Components/Pages/`.

| Path | Purpose |
|---|---|
| `Components/Routes.razor` | Root `<Router>` |
| `Components/Layout/MainLayout.razor` | Top nav bar with the four `NavLink`s |
| `Components/Pages/Home.razor` | `/` — New todo form |
| `Components/Pages/All.razor` | `/all` — Person1 list with Refresh button |
| `Components/Pages/Paged.razor` | `/paged` — Paged list with Refresh + "Load more" button |
| `Components/Pages/TodoDetail.razor` | `/todo/detail` — read-only view; reads from `TodoSelectionService` |
| `Components/Pages/Settings.razor` | `/settings` — person tabs, color chips, switches, etc. |
| `Services/BackendClient.cs` | Same shape as the MAUI app — h2c switch + `Grpc.Net.Client` |
| `Services/PersonContext.cs` | Same as MAUI |
| `Services/TodoSelectionService.cs` | Singleton handoff for the detail page — Blazor has no `IQueryAttributable` |

**Always-fresh data**: each Razor component re-runs `OnInitializedAsync` on every route navigation (the Router builds a new instance), so `All`/`Paged`/`Settings` clear their lists/fields and re-fetch every time. Mirrors the MAUI behavior.

**Pull-to-refresh**: replaced with explicit **Refresh** buttons — web has no native swipe gesture. Status labels are prefixed with `Refreshed HH:mm:ss — …` for parity with MAUI.

**Person selector**: a pair of tab buttons rather than a `CarouselView` (carousel is a touch metaphor; tabs are cleaner on the web).

**Color chips**: real-color background via an inline `style="background:#hex"`, `.selected` CSS class for the thick black border. Click sets `accentColor` and re-renders.

**Csproj quirk**: `<MauiXaml Update="MainPage.xaml" Inflator="Runtime" />` — XAML SourceGen and Razor SourceGen can't see each other's generated types, so resolving the `Routes` type at XAML compile time fails. Forcing runtime inflation for that one page works around it.

## Uno Platform client

Project: [unoplatform/UnoplatformUI/UnoplatformUI/UnoplatformUI.csproj](unoplatform/UnoplatformUI/UnoplatformUI/UnoplatformUI.csproj). Built for `net10.0-desktop` (Skia) as the primary dev target; the csproj also lists Android / iOS / WASM.

The original Uno.Extensions.Hosting/Navigation scaffolding is bypassed — `App.xaml.cs` uses a plain `Window` + `Frame` navigation pattern with a static `App.Services` `IServiceProvider`. The `Shell` `UserControl` hosts a `Uno.Toolkit.UI.TabBar` at the bottom and a `Frame` above; tab selection calls `Frame.Navigate(typeof(SomePage))` and clears the back stack, so every tab tap produces a fresh page instance.

| Path | Purpose |
|---|---|
| `App.xaml.cs` | DI setup + window startup |
| `Presentation/Shell.xaml(.cs)` | TabBar + content `Frame`, always-fresh tab navigation |
| `Presentation/Pages/NewTodoPage` | Form, `[RelayCommand]` Create |
| `Presentation/Pages/AllTodosPage` | `ListView` + Refresh button + click-through |
| `Presentation/Pages/PagedTodosPage` | `RefreshContainer` (real pull-to-refresh) + Refresh button + "Load more" |
| `Presentation/Pages/TodoDetailPage` | Reads `TodoSelectionService.Selected` on `OnNavigatedTo` |
| `Presentation/Pages/SettingsPage` | Person `ToggleButton` pair, color chips via `Border` + brush bindings |
| `Presentation/Pages/TimestampDisplayConverter.cs` | Formats `Timestamp` for list rows (x:Bind chains with method args don't work) |
| `Services/*.cs` | Same `BackendClient` / `PersonContext` / `TodoSelectionService` shape |

**Always-fresh data**: `Page.OnNavigatedTo` calls `vm.LoadAsync` / `RefreshAsync` every time. `Frame.Navigate` always builds a new page instance because `NavigationCacheMode="Disabled"` and the back stack is cleared on tab switch.

**Pull-to-refresh**: real, via `Microsoft.UI.Xaml.Controls.RefreshContainer` on the Paged page. The explicit Refresh button is kept as a desktop fallback.

**Person selector**: a pair of `ToggleButton`s. Functionally equivalent to MAUI's CarouselView — pick one of two — but cleaner in WinUI.

**Color chips**: each chip is a `Border` with `Background` from a per-chip `AccentChipVm.Brush`. The chip VM exposes `StrokeBrush` + `StrokeThickness` that re-evaluate when the selected `AccentColor` changes; `RefreshSelected()` is called on the whole collection on each selection so the thick black border moves to the active chip.

**Central Package Management**: this project uses CPM. Adding new packages means adding a `<PackageVersion>` entry to [unoplatform/UnoplatformUI/Directory.Packages.props](unoplatform/UnoplatformUI/Directory.Packages.props) and a `<PackageReference Include="…"/>` (no `Version` attribute) in the csproj.

Later todo:

* MAUI Angular with WebView
* Avalonia UI
