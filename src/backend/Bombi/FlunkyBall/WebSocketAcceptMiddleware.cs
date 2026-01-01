using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlunkyBall;

internal sealed class IncomingClientsQueue
{
    private readonly ConcurrentQueue<IncomingClient> _clients = new();
    private readonly List<SocketConnection> _pendingConnections = new();

    public TaskCompletionSource AddSocket(string token, WebSocket webSocket, string socketType)
    {
        lock (_clients)
        {
            var socketConnection = new SocketConnection(webSocket, token, socketType);
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
            if (connections.Length > 1)
            {
                var chunkDataChannel = connections.FirstOrDefault(c => c.SocketType == IncomingClient.SocketTypeChunkData); 
                var updateChannel = connections.FirstOrDefault(c => c.SocketType == IncomingClient.SocketTypeUpdate);

                if (chunkDataChannel != null && updateChannel != null)
                {
                    var incomingClient = new IncomingClient(
                        pendingConnections.Key,
                        chunkDataChannel.Socket,
                        chunkDataChannel.TaskCompletionSource,
                        updateChannel.Socket,
                        updateChannel.TaskCompletionSource
                    );
                    _clients.Enqueue(incomingClient);
                    
                    removedClientsByToken.Add(pendingConnections.Key);
                }
            }
        }
        
        _pendingConnections.RemoveAll(c => removedClientsByToken.Contains(c.Token));
    }
    
    private record SocketConnection(WebSocket Socket, string Token, string SocketType)
    {
        public TaskCompletionSource TaskCompletionSource { get; } = new();
    }
}

internal sealed class IncomingClient(string identityToken, WebSocket? chunkDataSocket, TaskCompletionSource chunkDataTcs, WebSocket? updateSocket, TaskCompletionSource updateTcs)
{
    public const string SocketTypeChunkData = "chunkData";
    public const string SocketTypeUpdate = "update";

    public static bool IsValidSocketType(string type)
        => type == SocketTypeChunkData || type == SocketTypeUpdate;

    public string IdentityToken { get; } = identityToken;

    public WebSocket? ChunkDataSocket { get; } = chunkDataSocket;

    public TaskCompletionSource ChunkDataSocketTaskCompletionSource { get; } = chunkDataTcs;

    public WebSocket? UpdateSocket { get; } = updateSocket;

    public TaskCompletionSource UpdateSocketTaskCompletionSource { get; } = updateTcs;
}

internal sealed class WebSocketAcceptMiddleware(
    IAuthService authService,
    IncomingClientsQueue incomingClientsQueue, 
    ILogger<WebSocketAcceptMiddleware> logger, 
    IHostApplicationLifetime appLifetime) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var connectionSocketFeature = context.Features.Get<IConnectionSocketFeature>();
                if (connectionSocketFeature != null)
                {
                    connectionSocketFeature.Socket.NoDelay = true;    
                }
                else
                {
                    logger.LogWarning("Could not set NoDelay for websocket connection, this may lead to performance problems.");
                }

                var type = context.Request.Query["type"].FirstOrDefault() ?? string.Empty;
                var userId = int.TryParse(context.Request.Query["userId"].FirstOrDefault() ?? string.Empty, out var id) ? id : -1;
                var token = context.Request.Query["token"].FirstOrDefault() ?? string.Empty;
            
                if (authService.VerifyIdentity(userId, token) && IncomingClient.IsValidSocketType(type))
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    var taskCompletionSource = incomingClientsQueue.AddSocket(token, webSocket, type);
                
                    var shutdownCts = new CancellationTokenSource();
                    var registration = appLifetime.ApplicationStopping.Register(() =>
                    {
                        shutdownCts.Cancel();
                    });
                    
                    try
                    {
                        await taskCompletionSource.Task.WaitAsync(registration.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Task cancelled is intended when the token is cancelling due to shutdown of application.
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            await next(context);
        }
    }
}