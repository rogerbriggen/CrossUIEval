using BackendUiEval.Grpc;
using Grpc.Net.Client;

namespace UnoplatformUI.Services;

public class BackendClient : IDisposable
{
#if __ANDROID__
    public const string BackendAddress = "http://10.0.2.2:5080";
#else
    public const string BackendAddress = "http://localhost:5080";
#endif

    private readonly GrpcChannel _channel;

    public SettingsService.SettingsServiceClient Settings { get; }
    public TodoService.TodoServiceClient Todos { get; }

    public BackendClient()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        _channel = GrpcChannel.ForAddress(BackendAddress, new GrpcChannelOptions
        {
            MaxReceiveMessageSize = 16 * 1024 * 1024,
        });
        Settings = new SettingsService.SettingsServiceClient(_channel);
        Todos = new TodoService.TodoServiceClient(_channel);
    }

    public void Dispose() => _channel.Dispose();
}
