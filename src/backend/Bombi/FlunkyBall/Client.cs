using Microsoft.Extensions.Logging;

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

    void EnqueueChunkDataMessage(SocketMessage socketMessage);

    SocketMessage? GetNextMessage();

    SocketMessage? GetNextChunkLoadRequest();

    int MessageCount { get; }
}

internal sealed class Client : IClient
{
    private const int ClientHeartbeatTimeoutSeconds = 10;
    
    private readonly ClientSocketChannel _chunkDataChannel;
    private readonly ClientSocketChannel _updateChannel;
    private readonly Action<Client, ClientState> _stateChanged;
    private readonly Task _communicationTask;

    public Client(
        IncomingClient client, 
        IAuthService authService,
        Action<Client, ClientState> stateChanged,
        ILogger<ConnectorBackgroundService> logger,
        CancellationToken stoppingToken)
    {
        if (client.ChunkDataSocket == null) throw new InvalidOperationException("Chunk Data Socket null");
        if (client.UpdateSocket == null) throw new InvalidOperationException("Update Socket null");
        
        var user = authService.GetUser(client.IdentityToken);
        Id = user.Id;
        Name = user.Name;
        _stateChanged = stateChanged;
        
        _chunkDataChannel = new ClientSocketChannel(
            Id,
            client.ChunkDataSocket, 
            client.ChunkDataSocketTaskCompletionSource, 
            logger, 
            ClientHeartbeatTimeoutSeconds);
        _updateChannel = new ClientSocketChannel(
            Id,
            client.UpdateSocket,
            client.UpdateSocketTaskCompletionSource,
            logger,
            null);

        _communicationTask = StartCommunicationAsync(stoppingToken);

        State = ClientState.Connected;
    }

    public int Id { get; private set; }
    
    public ClientState State { get; set; }

    public string Name { get; private set; }

    public void EnqueueMessage(SocketMessage socketMessage) 
        => _updateChannel.EnqueueMessage(socketMessage);
    
    public void EnqueueChunkDataMessage(SocketMessage socketMessage) 
        => _chunkDataChannel.EnqueueMessage(socketMessage);

    public SocketMessage? GetNextMessage()
        => _updateChannel.GetNextMessage();
    
    public SocketMessage? GetNextChunkLoadRequest()
        => _chunkDataChannel.GetNextMessage();

    public int MessageCount => _updateChannel.MessageCount;

    public async Task CloseAsync()
    {
        _chunkDataChannel.StartComplete();
        _updateChannel.StartComplete();
        await _communicationTask;
    }

    private async Task StartCommunicationAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(
            _chunkDataChannel.StartReadMessagesAsync(stoppingToken),
            _updateChannel.StartReadMessagesAsync(stoppingToken),
            _chunkDataChannel.StartWriteMessagesAsync(stoppingToken),
            _updateChannel.StartWriteMessagesAsync(stoppingToken)
        ).ConfigureAwait(false);
            
        State = ClientState.Disconnected;
    }
}