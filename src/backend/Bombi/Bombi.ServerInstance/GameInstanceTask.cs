using System.Collections.Concurrent;
using System.Diagnostics;
using Bombi.ServerInstance.Game;

namespace Bombi.ServerInstance;

public sealed class GameInstanceTask
{
    private int _nextFreeClientId = 1;
    private readonly GameInstance _gameInstance;
    private readonly TimeSpan _tickTime;
    private readonly ConcurrentQueue<JoiningClient> _joiningQueue = new();
    private readonly ConcurrentQueue<int> _joinedQueue = new();
    
    public Guid Id { get; } = Guid.NewGuid();
    
    public GameInstanceState State => _gameInstance.State;

    public GameInstanceTask(InstanceSettings settings, CancellationToken cancellationToken)
    {
        _tickTime = TimeSpan.FromSeconds(1.0 / settings.TickRate);
        _gameInstance = new GameInstance();
        Task = RunGameInstanceAsync(cancellationToken);
    }

    public Task Task { get; }

    public int ClientJoining(string userName)
    {
        _joiningQueue.Enqueue(new JoiningClient(++_nextFreeClientId, userName));
        return 1;
    }

    public void ClientJoined(int id)
    {
        _joinedQueue.Enqueue(id);
    }
    
    private async Task RunGameInstanceAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var tick = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_joiningQueue.TryDequeue(out var client))
            {
                _gameInstance.ClientJoining(client.Id, client.Name);
            }
            else if (_joinedQueue.TryDequeue(out var id))
            {
                _gameInstance.ClientJoined(id);
            }
            
            _gameInstance.RunFrame(tick, _tickTime.TotalSeconds);

            tick++;
            var elapsedTime = sw.Elapsed;
            sw.Reset();
            var waitTime = _tickTime - elapsedTime;
            if (waitTime > TimeSpan.Zero)
            {
                await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private record JoiningClient(int Id, string Name);
}