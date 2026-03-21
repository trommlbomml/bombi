using Microsoft.AspNetCore.Connections.Features;

namespace Bombi.ServerInstance.Networking;

internal sealed class WebSocketAcceptMiddleware(
    IGameInstanceService gameInstanceService, 
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

                var token = context.Request.Query["token"].FirstOrDefault() ?? string.Empty;
            
                if (!string.IsNullOrEmpty(token))
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    var taskCompletionSource = new TaskCompletionSource();
                    gameInstanceService.AcceptIncomingWebSocket(new IncomingClient(token, webSocket, taskCompletionSource));
                
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