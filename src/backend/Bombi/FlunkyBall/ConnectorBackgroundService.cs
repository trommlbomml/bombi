using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlunkyBall;

public interface IGameServerContext
{
    void EnqueueBroadCastMessage(SocketMessage message);
}

internal sealed class ConnectorBackgroundService : BackgroundService, IGameServerContext
{
    private readonly double _frameTimeSeconds;
    private readonly List<Client> _internalClients = new();
    private readonly List<IClient> _clients = new();
    private readonly ConcurrentQueue<SocketMessage> _broadCastMessages = new();
    private readonly ConcurrentQueue<Client> _disconnectedClients = new();
    private readonly ConcurrentQueue<Client> _clientsToClose = new();
    private readonly ILogger<ConnectorBackgroundService> _logger;
    private readonly IAuthService _authService;
    private readonly Func<IGameServer> _gameServerFunc;
    private readonly IOptions<FlunkyBallSettings> _settings;
    private readonly IncomingClientsQueue _incomingClientsQueue;
    
    private IGameServer? _gameServer;

    public ConnectorBackgroundService(ILogger<ConnectorBackgroundService> logger, 
        IAuthService authService, 
        Func<IGameServer> gameServerFunc,
        IOptions<FlunkyBallSettings> settings,
        IncomingClientsQueue incomingClientsQueue)
    {
        _logger = logger;
        _authService = authService;
        _gameServerFunc = gameServerFunc;
        _settings = settings;
        _incomingClientsQueue = incomingClientsQueue;
        _frameTimeSeconds = 1.0f / _settings.Value.FixedServerFramesPerSecond;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _gameServer = _gameServerFunc();
            await Task.Run(() => _gameServer.PrepareWorld(this, stoppingToken), stoppingToken);

            var sw = Stopwatch.StartNew();
            var serverTime = Stopwatch.StartNew();
            var runFrameStopwatch = Stopwatch.StartNew();
            while (!stoppingToken.IsCancellationRequested)
            {
                runFrameStopwatch.Restart();
            
                if (_clientsToClose.TryDequeue(out var client))
                {
                    await client.CloseAsync().ConfigureAwait(false);
                }
            
                var serverTimeMilliseconds = serverTime.ElapsedMilliseconds;
                sw.Restart();
            
                AddNewIncomingClients(stoppingToken);
                MoveDisconnectedClientsToRemovalQueue();
                SpreadNextBroadCastMessage();

                _gameServer.RunFrame(this, _clients, _frameTimeSeconds, serverTimeMilliseconds);
                runFrameStopwatch.Stop();
            
                var pauseTime = _frameTimeSeconds * 1000.0f - runFrameStopwatch.ElapsedMilliseconds;
                if (pauseTime > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(pauseTime), stoppingToken);
                }
            }

            if (stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(_internalClients.Select(c => c.CloseAsync()));
            }
        }
        catch (TaskCanceledException)
        {
            // task cancelled exception is not interesting.
        }
    }

    private void SpreadNextBroadCastMessage()
    {
        if (!_broadCastMessages.TryDequeue(out var message)) return;
        
        foreach (var client in _internalClients)
        {
            client.EnqueueMessage(message);
        }
    }

    private void MoveDisconnectedClientsToRemovalQueue()
    {
        if (_disconnectedClients.TryDequeue(out var client))
        {
            _internalClients.Remove(client);
            _clients.Remove(client);
            _clientsToClose.Enqueue(client);
        }
    }

    private void AddNewIncomingClients(CancellationToken stoppingToken)
    {
        var newClient = _incomingClientsQueue.GetNewClient();
        if (newClient == null) return;
        
        var client = new Client(
            newClient,
            _authService, 
            OnClientStateChanged,
            _logger,
            _settings,
            stoppingToken);

        if (_internalClients.Count >= _settings.Value.MaxClients)
        {
            _logger.LogInformation("Server full, no more clients can join.");
            _clientsToClose.Enqueue(client);
        }
        else
        {
            _internalClients.Add(client);   
            _clients.Add(client);
        }
    }

    private void OnClientStateChanged(Client client, ClientState state)
    {
        if (_gameServer == null) throw new InvalidOperationException("Game Server is null");
        
        switch (state)
        {
            case ClientState.Connected:
                _gameServer.OnClientConnected(this, client);
                _logger.LogInformation($"Client '{client.Name}' connected to the server.");
                break;
            case ClientState.Disconnected:
                _gameServer.OnClientDisconnected(this, client);
                _logger.LogInformation($"Client '{client.Name}' Disconnected from the server.");
                _disconnectedClients.Enqueue(client);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void EnqueueBroadCastMessage(SocketMessage message)
    {
        _broadCastMessages.Enqueue(message);
    }
}