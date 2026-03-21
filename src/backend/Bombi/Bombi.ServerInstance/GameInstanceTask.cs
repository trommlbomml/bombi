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
        var clientId = _nextFreeClientId++;
        _joiningQueue.Enqueue(new JoiningClient(clientId, userName));
        return clientId;
    }

    public void ClientJoined(int id, IncomingNetworkClient networkClient)
    {
        _joinedQueue.Enqueue(new JoinedClient(id, networkClient));
    }
    
    private async Task RunGameInstanceAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var tick = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            HandleClientJoining();
            
            _gameInstance.RunFrame(tick, _tickTime.TotalSeconds);

            await SleepForNextTickAsync(sw,  cancellationToken).ConfigureAwait(false);
            tick++;
        }
    }

    private void HandleClientJoining()
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
            _networkClients.Add(joinedClient.Id, new NetworkClient(joinedClient.Id, joinedClient.NetworkClient,
                (networkClient, state) => { }, _logger, CancellationToken.None));
        }
    }

    private async Task SleepForNextTickAsync(Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        var elapsedTime = stopwatch.Elapsed;
        stopwatch.Reset();
        var waitTime = _tickTime - elapsedTime;
        if (waitTime > TimeSpan.Zero)
        {
            await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
        }
    }

    private record JoiningClient(int Id, string Name);
    
    private record JoinedClient(int Id, IncomingNetworkClient NetworkClient);
}