using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.Extensions.Options;

namespace FlunkyBall;

internal sealed class IncomingClientsQueue(IOptions<FlunkyBallSettings> options)
{
    private readonly ConcurrentQueue<IncomingClient> _clients = new();
    private readonly List<SocketConnection> _pendingConnections = new();

    public TaskCompletionSource AddSocket(string token, WebSocket webSocket, string type)
    {
        lock (_clients)
        {
            var socketConnection = new SocketConnection(webSocket, token, type);
            _pendingConnections.Add(socketConnection);
            
            BuildClientFromPendingConnections();
            
            return socketConnection.TaskCompletionSource;
        }
    }

    public IncomingClient? GetNewClient()
        => _clients.TryDequeue(out var client) ? client : null;

    private void BuildClientFromPendingConnections()
    {
        var removedClientsByToken = new List<string>();
        
        foreach (var pendingConnections in _pendingConnections.GroupBy(c => c.Token))
        {
            var connections = pendingConnections.ToArray();
            if (connections.Length == options.Value.WebSocketChannels.Length)
            {
                var incomingClient = new IncomingClient(
                    pendingConnections.Key,
                    connections.Select(c => new IncomingSocket(c.Socket, c.Type, c.TaskCompletionSource))
                );
                _clients.Enqueue(incomingClient);
                    
                removedClientsByToken.Add(pendingConnections.Key);
            }
        }
        
        _pendingConnections.RemoveAll(c => removedClientsByToken.Contains(c.Token));
    }
    
    private record SocketConnection(WebSocket Socket, string Token, string Type)
    {
        public TaskCompletionSource TaskCompletionSource { get; } = new();
    }
}