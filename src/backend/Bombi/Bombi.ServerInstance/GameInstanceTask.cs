using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Bombi.ServerInstance.Game;
using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance;

public sealed class GameInstanceTask
{
    private readonly ILogger<IGameInstanceService> _logger;
    private int _nextFreeClientId = 1;
    private readonly GameInstance _gameInstance;
    private readonly TimeSpan _tickTime;
    private readonly ConcurrentQueue<JoiningClient> _joiningQueue = new();
    private readonly ConcurrentQueue<JoinedClient> _joinedQueue = new();
    private readonly Dictionary<int, NetworkClient> _networkClients = new();
    
    public Guid Id { get; } = Guid.NewGuid();
    
    public GameInstanceState State => _gameInstance.State;

    public GameInstanceTask(InstanceSettings settings, CancellationToken cancellationToken, ILogger<IGameInstanceService> logger)
    {
        _logger = logger;
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

    public void ClientJoined(int id, IncomingClient client)
    {
        _joinedQueue.Enqueue(new JoinedClient(id, client));
    }
    
    private async Task RunGameInstanceAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var tick = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_joiningQueue.TryDequeue(out var client))
            {
                _gameInstance.Clients.Add(new Client
                {
                    Id = client.Id,
                    Name = client.Name,
                    IsJoined = false
                });
            }
            else if (_joinedQueue.TryDequeue(out var joinedClient))
            {
                _gameInstance.Clients.Single(c => c.Id == joinedClient.Id).IsJoined = true;
                _networkClients.Add(client.Id, new NetworkClient(joinedClient.Id, joinedClient.Client,
                    (networkClient, state) => { }, _logger, CancellationToken.None));
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
    
    private record JoinedClient(int Id, IncomingClient Client);
}