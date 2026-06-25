using BackendUiEval.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    // 10000 todos in one ListAll response can exceed the 4MB default.
    options.MaxSendMessageSize = 16 * 1024 * 1024;
    options.MaxReceiveMessageSize = 16 * 1024 * 1024;
});

builder.Services.AddSingleton<TodoStore>();
builder.Services.AddHostedService<TodoTickerService>();

builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/2 cleartext for gRPC during development; MAUI/clients connect to http://localhost:5080
    options.ListenLocalhost(5080, listen => listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

var app = builder.Build();

app.MapGrpcService<SettingsService>();
app.MapGrpcService<TodoGrpcService>();
app.MapGet("/", () => "BackendUiEval gRPC host. Services: SettingsService, TodoService. See /Protos/*.proto for contracts.");

app.Run();
