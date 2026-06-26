using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using BackendUiEval.Grpc;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace AvaloniaUiEval.Services;

public class BackendClient : IDisposable
{
    // Android emulator maps host loopback to 10.0.2.2; everything else uses localhost.
    // Runtime check rather than `#if ANDROID` because this is a single-target net10.0
    // library shared across all head projects — the Android symbol is never defined here.
    public static readonly string DefaultHost = OperatingSystem.IsAndroid() ? "10.0.2.2" : "localhost";
    public const int DefaultPort = 5080;

    private readonly object _lock = new();
    private readonly string? _configPath;

    private GrpcChannel _channel = null!;
    private SettingsService.SettingsServiceClient _settings = null!;
    private TodoService.TodoServiceClient _todos = null!;

    public string Host { get; private set; } = DefaultHost;
    public int Port { get; private set; } = DefaultPort;
    public string Address => $"http://{Host}:{Port}";

    public event EventHandler? AddressChanged;

    public SettingsService.SettingsServiceClient Settings { get { lock (_lock) return _settings; } }
    public TodoService.TodoServiceClient Todos { get { lock (_lock) return _todos; } }

    public BackendClient()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        _configPath = TryGetConfigPath();

        if (_configPath is not null && TryLoadPersisted(out var host, out var port))
        {
            Host = host;
            Port = port;
        }
        Rebuild();
    }

    public void UpdateEndpoint(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Host cannot be empty.", nameof(host));
        if (port is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(port), "Port must be 1..65535.");

        lock (_lock)
        {
            _channel.Dispose();
            Host = host.Trim();
            Port = port;
            Rebuild();
            Persist();
        }
        AddressChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Rebuild()
    {
        var options = new GrpcChannelOptions
        {
            MaxReceiveMessageSize = 16 * 1024 * 1024,
        };

        if (OperatingSystem.IsBrowser())
        {
            // WASM can't speak HTTP/2 frames — route through gRPC-Web on HTTP/1.1.
            // The browser's fetch-based handler is the only viable inner handler under WASM;
            // SocketsHttpHandler doesn't run there.
            options.HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
        }

        _channel = GrpcChannel.ForAddress(Address, options);
        _settings = new SettingsService.SettingsServiceClient(_channel);
        _todos = new TodoService.TodoServiceClient(_channel);
    }

    private static string? TryGetConfigPath()
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrossUIEval");
            if (string.IsNullOrEmpty(dir)) return null;
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "backend.json");
        }
        catch
        {
            // No filesystem (WASM browser) or denied access — run with in-memory defaults only.
            return null;
        }
    }

    private bool TryLoadPersisted(out string host, out int port)
    {
        host = DefaultHost;
        port = DefaultPort;
        try
        {
            if (_configPath is null || !File.Exists(_configPath)) return false;
            var json = File.ReadAllText(_configPath);
            var cfg = JsonSerializer.Deserialize<PersistedConfig>(json);
            if (cfg is null || string.IsNullOrWhiteSpace(cfg.Host) || cfg.Port is < 1 or > 65535) return false;
            host = cfg.Host;
            port = cfg.Port;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void Persist()
    {
        if (_configPath is null) return;
        try
        {
            File.WriteAllText(_configPath, JsonSerializer.Serialize(new PersistedConfig { Host = Host, Port = Port }));
        }
        catch
        {
            // Persistence failure is non-fatal.
        }
    }

    public void Dispose()
    {
        lock (_lock) _channel.Dispose();
    }

    private sealed class PersistedConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = DefaultPort;
    }
}
