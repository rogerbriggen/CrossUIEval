using BackendUiEval.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    // 10000 todos in one ListAll response can exceed the 4MB default.
    options.MaxSendMessageSize = 16 * 1024 * 1024;
    options.MaxReceiveMessageSize = 16 * 1024 * 1024;
});

// Browser clients (Avalonia.Browser etc.) reach gRPC over gRPC-Web on HTTP/1.1
// and must satisfy CORS since they're loaded from a different origin (dev server port).
// Open policy is fine for a development backend.
const string BrowserCors = "BrowserCors";
builder.Services.AddCors(o => o.AddPolicy(BrowserCors, p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding")));

builder.Services.AddSingleton<TodoStore>();
builder.Services.AddHostedService<TodoTickerService>();

builder.WebHost.ConfigureKestrel(options =>
{
    // Http1AndHttp2 so the same port serves native gRPC (h2c) and gRPC-Web (HTTP/1.1).
    options.ListenLocalhost(5080, listen => listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
});

var app = builder.Build();

app.UseCors(BrowserCors);
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGrpcService<SettingsService>();
app.MapGrpcService<TodoGrpcService>();
app.MapGet("/", () => "BackendUiEval gRPC host. Services: SettingsService, TodoService. See /Protos/*.proto for contracts.");

app.Run();
