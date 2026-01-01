using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlunkyBall;

internal sealed class WebSocketAcceptMiddleware(
    IAuthService authService,
    IncomingClientsQueue incomingClientsQueue, 
    ILogger<WebSocketAcceptMiddleware> logger, 
    IOptions<FlunkyBallSettings> settings,
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
            
                if (authService.VerifyIdentity(userId, token) && IsValidSocketType(type))
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
    
    private bool IsValidSocketType(string type)
     => settings.Value.WebSocketChannels.Any(s => s == type);
}