using System.Collections.Concurrent;

namespace Bombi.ServerInstance.Networking;

public sealed class SocketMessageFactory
{
    private readonly ConcurrentQueue<SocketMessage> _queue;

    public static readonly SocketMessageFactory Empty = new(0);

    public SocketMessageFactory(): this(100)  {}

    private SocketMessageFactory(int preCreatedMessageCount) =>
        _queue = new ConcurrentQueue<SocketMessage>(Enumerable
            .Range(1, preCreatedMessageCount)
            .Select(_ => new SocketMessage(this)));

    public SocketMessage Rent() 
        => _queue.TryDequeue(out var message) ? message : new SocketMessage(this);

    public void Return(SocketMessage message) 
        => _queue.Enqueue(message);
}