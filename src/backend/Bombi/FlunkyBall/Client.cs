using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlunkyBall;

public enum ClientState
{
    Connected,
    Disconnected,
}

public interface IClient
{
    int Id { get; }
    string Name { get; }

    void EnqueueMessage(SocketMessage socketMessage);
    
    void EnqueueMessage(int channelIndex, SocketMessage socketMessage);

    SocketMessage? GetNextMessage();
    
    SocketMessage? GetNextMessage(int channelIndex);
}

internal sealed class Client : IClient
{
    private const int ClientHeartbeatTimeoutSeconds = 10;
    
    private readonly List<ClientSocketChannel> _socketChannels = [];
    private readonly Action<Client, ClientState> _stateChanged;
    private readonly Task _communicationTask;

    public Client(
        IncomingClient client, 
        IAuthService authService,
        Action<Client, ClientState> stateChanged,
        ILogger<ConnectorBackgroundService> logger,
        IOptions<FlunkyBallSettings> settings,
        CancellationToken stoppingToken)
    {
        var user = authService.GetUser(client.IdentityToken);
        Id = user.Id;
        Name = user.Name;
        _stateChanged = stateChanged;
        
        _socketChannels.AddRange(
            client.IncomingSockets.Select((incoming, index) => new ClientSocketChannel(
                Id, 
                incoming, 
                logger, 
                settings.Value.WebSocketChannels[index],
                index == settings.Value.HeartbeatChannelIndex ? settings.Value.ClientHeartbeatTimeout : null)
            )
        );

        _communicationTask = StartCommunicationAsync(stoppingToken);

        State = ClientState.Connected;
    }

    public int Id { get; }
    
    public ClientState State { get; set; }

    public string Name { get; private set; }
    
    public void EnqueueMessage(int channelIndex, SocketMessage socketMessage)
        => _socketChannels[channelIndex].EnqueueMessage(socketMessage);

    public void EnqueueMessage(SocketMessage socketMessage) 
        => EnqueueMessage(0, socketMessage);
    
    
    public SocketMessage? GetNextMessage(int channelIndex)
        => _socketChannels[channelIndex].GetNextMessage();

    public SocketMessage? GetNextMessage()
        => GetNextMessage(0);

    public async Task CloseAsync()
    {
        _socketChannels.ForEach(c => c.StartComplete());
        await _communicationTask;
    }

    private async Task StartCommunicationAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(
            _socketChannels.SelectMany(c => new[]
            {
                c.StartReadMessagesAsync(stoppingToken),
                c.StartWriteMessagesAsync(stoppingToken)
            }
            )).ConfigureAwait(false);
            
        State = ClientState.Disconnected;
    }
}