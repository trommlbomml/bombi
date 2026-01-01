using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FlunkyBall;

internal sealed class ClientSocketChannel
{
    private const string KeepAliveMessagePayload = "ping";

    private readonly int _clientId;
    private readonly WebSocket _socket;
    private readonly TaskCompletionSource _taskCompletionSource;
    private readonly ILogger<ConnectorBackgroundService> _logger;
    private readonly int? _heartBeatTimeout;
    private readonly byte[] _receiveBuffer = new byte[1024];
    private readonly byte[] _messageBuffer = new byte[1024];
    private readonly ConcurrentQueue<SocketMessage> _incomingMessages = new();
    private readonly ConcurrentQueue<SocketMessage> _outgoing = new();

    public ClientSocketChannel(
        int clientId,
        WebSocket socket, 
        TaskCompletionSource taskCompletionSource,
        ILogger<ConnectorBackgroundService> logger,
        int? heartBeatTimeout)
    {
        _clientId = clientId;
        _socket = socket;
        _taskCompletionSource = taskCompletionSource;
        _logger = logger;
        _heartBeatTimeout = heartBeatTimeout;
    }

    public SocketMessage? GetNextMessage()
        => _incomingMessages.TryDequeue(out var msg) ? msg : null;

    public int MessageCount => _incomingMessages.Count;

    public void StartComplete()
    {
        _taskCompletionSource.SetResult();
    }

    public Task CloseAsync(WebSocketCloseStatus status, string reason, CancellationToken stoppingToken) 
        => _socket.CloseAsync(status, reason, stoppingToken);

    public async Task StartReadMessagesAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                if (_heartBeatTimeout.HasValue)
                {
                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_heartBeatTimeout.Value));
                    var newStoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, stoppingToken);
                    
                    var message = await ReadMessageFromSocketAsync(newStoppingToken.Token).ConfigureAwait(false);
                    if (!newStoppingToken.Token.IsCancellationRequested && !message.IsEmpty)
                    {
                        _incomingMessages.Enqueue(message);
                    }
                }
                else
                {
                    var message = await ReadMessageFromSocketAsync(stoppingToken).ConfigureAwait(false);
                    if (!stoppingToken.IsCancellationRequested && !message.IsEmpty)
                    {
                        _incomingMessages.Enqueue(message);
                    }
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

    public void EnqueueMessage(SocketMessage message) 
        => _outgoing.Enqueue(message);

    public async Task StartWriteMessagesAsync(CancellationToken stoppingToken)
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
    
    public async Task SendMessageAsync(SocketMessage message, CancellationToken stoppingToken)
    {
        if (message.MessageType == WebSocketMessageType.Text)
        {
            await _socket.SendAsync(message.Data, message.MessageType, true, stoppingToken).ConfigureAwait(false);
        }
        else
        {
            await using var memoryStream = new MemoryStream();
            if (message.Compress)
            {
                memoryStream.WriteByte(1);
                await using var deflate = new DeflateStream(memoryStream, CompressionLevel.Optimal, true);
                await deflate.WriteAsync(message.Data, stoppingToken).ConfigureAwait(false);
            }
            else
            {
                memoryStream.WriteByte(0);
                memoryStream.Write(message.Data);
            }
            memoryStream.Position = 0;
        
            await _socket.SendAsync(new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length), message.MessageType, true, stoppingToken).ConfigureAwait(false);   
        }
    }
    
    public async Task<SocketMessage> ReadMessageFromSocketAsync(CancellationToken stoppingToken)
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
                case WebSocketMessageType.Text:
                    var content = Encoding.UTF8.GetString(_messageBuffer, 0, receivedBytes);
                    return content == KeepAliveMessagePayload ? SocketMessage.Empty : new SocketMessage(content);
                case WebSocketMessageType.Binary:
                    return new SocketMessage(new ArraySegment<byte>(_receiveBuffer, 0, receivedBytes));
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