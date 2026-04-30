using System.Collections.Concurrent;
using System.Diagnostics;
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
    private readonly ConcurrentQueue<int> _leavingQueue = new();
    private readonly Dictionary<int, NetworkClient> _networkClients = new();
    private readonly SocketMessageFactory _factory;
    
    public Guid Id { get; } = Guid.NewGuid();
    
    public GameInstanceState State => _gameInstance.State;

    public GameInstanceTask(InstanceSettings settings, CancellationToken cancellationToken, ILogger<IGameInstanceService> logger)
    {
        _logger = logger;
        _factory = new SocketMessageFactory();
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

    private void OnClientStateChanged(NetworkClient client, ClientState state)
    {
        if (state == ClientState.Disconnected)
        {
            _leavingQueue.Enqueue(client.Id);
        }
    }
    
    private async Task RunGameInstanceAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var tick = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            HandleClientJoiningAndLeaving();
            
            _gameInstance.RunFrame(tick, _tickTime.TotalSeconds);

            foreach (var client in _networkClients)
            {
                var message = _factory.Rent();
                _gameInstance.SerializeGameState(tick, message);
                client.Value.EnqueueMessage(message);
            }
            
            await SleepForNextTickAsync(sw,  cancellationToken).ConfigureAwait(false);
            tick++;
        }
    }

    private void HandleClientJoiningAndLeaving()
    {
        if (_joiningQueue.TryDequeue(out var client))
        {
            _gameInstance.OnClientJoining(client.Id,  client.Name);
            _logger.LogInformation("Client [{Id}] is joining the sever", client.Id);
        }
        else if (_joinedQueue.TryDequeue(out var joinedClient))
        {
            _gameInstance.OnClientJoined(joinedClient.Id);
            _networkClients.Add(joinedClient.Id, new NetworkClient(joinedClient.Id, joinedClient.NetworkClient,
                OnClientStateChanged, _factory, _logger, CancellationToken.None));
            _logger.LogInformation("Client [{Id}] has joined the sever", joinedClient.Id);
        }

        if (_leavingQueue.TryDequeue(out var leavingClient))
        {
            _gameInstance.OnClientLeft(leavingClient);
            _logger.LogInformation("Client [{Id}] has left the sever", leavingClient);
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