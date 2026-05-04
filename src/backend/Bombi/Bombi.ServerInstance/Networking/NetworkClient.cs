using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Bombi.ServerInstance.Networking;

public enum ClientState
{
    Connected,
    Disconnected,
}

public interface INetworkClient
{
    int Id { get; }

    void EnqueueMessage(SocketMessage socketMessage);

    SocketMessage? GetNextMessage();
}

internal sealed class NetworkClient : INetworkClient
{
    private readonly int _clientId;
    private readonly WebSocket _socket;
    private readonly TaskCompletionSource _taskCompletionSource;
    private readonly ILogger<IGameInstanceService> _logger;
    private readonly byte[] _receiveBuffer = new byte[1024];
    private readonly byte[] _messageBuffer = new byte[1024];
    private readonly ConcurrentQueue<SocketMessage> _incomingMessages = new();
    private readonly ConcurrentQueue<SocketMessage> _outgoing = new();
    
    private readonly Action<NetworkClient, ClientState> _stateChanged;
    private readonly SocketMessageFactory _factory;
    private readonly Task _communicationTask;
    private ClientState _state;

    public NetworkClient(
        int id,
        IncomingNetworkClient networkClient, 
        Action<NetworkClient, ClientState> stateChanged,
        SocketMessageFactory factory,
        ILogger<IGameInstanceService> logger,
        CancellationToken stoppingToken)
    {
        _stateChanged = stateChanged;
        _factory = factory;

        _clientId = id;
        _socket = networkClient.Socket;
        _taskCompletionSource = networkClient.TaskCompletionSource;
        _logger = logger;
        _communicationTask = StartCommunicationAsync(stoppingToken);

        State = ClientState.Connected;
    }
    
    public int MessageCount => _incomingMessages.Count;

    public int Id { get; }

    public ClientState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                _stateChanged(this, _state);
            }
        }
    }
    

    public void EnqueueMessage(SocketMessage socketMessage) 
        => _outgoing.Enqueue(socketMessage);

    public SocketMessage? GetNextMessage()
        => _incomingMessages.TryDequeue(out var msg) ? msg : null;

    public async Task CloseAsync()
    {
        _taskCompletionSource.SetResult();
        await _communicationTask;
    }
    
    public Task CloseAsync(WebSocketCloseStatus status, string reason, CancellationToken stoppingToken) 
        => _socket.CloseAsync(status, reason, stoppingToken);

    private async Task StartCommunicationAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(
            StartReadMessagesAsync(stoppingToken),
            StartWriteMessagesAsync(stoppingToken)).ConfigureAwait(false);
            
        State = ClientState.Disconnected;
    }


    private async Task StartReadMessagesAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var newStoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, stoppingToken);
                    
                var message = await ReadMessageFromSocketAsync(newStoppingToken.Token).ConfigureAwait(false);
                if (!newStoppingToken.Token.IsCancellationRequested && !message.IsEmpty)
                {
                    _incomingMessages.Enqueue(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client {Id} will be disconnected due to heartbeat timeout", _clientId);
            if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)
            {
                await _socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Receive timeout",
                    CancellationToken.None);
            }
        }
    }

    private async Task StartWriteMessagesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
        {
            if (_outgoing.TryDequeue(out var message))
            {
                await SendMessageAsync(message, stoppingToken).ConfigureAwait(false);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task SendMessageAsync(SocketMessage message, CancellationToken stoppingToken)
    {
        await _socket.SendAsync(message.Data, WebSocketMessageType.Binary, true, stoppingToken).ConfigureAwait(false);   
    }

    private async Task<SocketMessage> ReadMessageFromSocketAsync(CancellationToken stoppingToken)
    {
        try
        {
            var receivedBytes = 0;
            WebSocketReceiveResult result;
            do
            {
                result = await _socket.ReceiveAsync(_receiveBuffer, stoppingToken).ConfigureAwait(false);

                if (result.CloseStatus.HasValue)
                {
                    return SocketMessage.Empty;
                }

                _receiveBuffer.CopyTo(_messageBuffer, receivedBytes);
                receivedBytes += result.Count;

            } while (!result.EndOfMessage);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Binary:
                    var socketMessage = _factory.Rent();
                    socketMessage.Write(new ArraySegment<byte>(_receiveBuffer, 0, receivedBytes));
                    return socketMessage;
                case WebSocketMessageType.Close:
                    return SocketMessage.Empty;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading message from socket");
            return SocketMessage.Empty;
        }
    }
}