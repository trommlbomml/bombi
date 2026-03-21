using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Bombi.ServerInstance;

internal sealed class IncomingClientsQueue
{
    private readonly ConcurrentQueue<IncomingClient> _clients = new();

    public TaskCompletionSource AddSocket(string token, WebSocket webSocket)
    {
        var socket = new IncomingClient(token, webSocket, new TaskCompletionSource());
        _clients.Enqueue(socket);
        return socket.TaskCompletionSource;
    }

    public IncomingClient? GetNewClient()
        => _clients.TryDequeue(out var client) ? client : null;
}