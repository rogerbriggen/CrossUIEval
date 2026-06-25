using BackendUiEval.Grpc;
using Grpc.Net.Client;

namespace MauiUiEval.Services;

public class BackendClient : IDisposable
{
    // Android emulator maps host loopback to 10.0.2.2; everything else uses localhost.
#if ANDROID
    public const string BackendAddress = "http://10.0.2.2:5080";
#else
    public const string BackendAddress = "http://localhost:5080";
#endif

    private readonly GrpcChannel _channel;

    public SettingsService.SettingsServiceClient Settings { get; }
    public TodoService.TodoServiceClient Todos { get; }

    public BackendClient()
    {
        // h2c (HTTP/2 cleartext) for development. Production should use TLS.
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        _channel = GrpcChannel.ForAddress(BackendAddress, new GrpcChannelOptions
        {
            // ListAll of 10k todos can exceed the 4MB default.
            MaxReceiveMessageSize = 16 * 1024 * 1024,
        });
        Settings = new SettingsService.SettingsServiceClient(_channel);
        Todos = new TodoService.TodoServiceClient(_channel);
    }

    public void Dispose() => _channel.Dispose();
}
