using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FlunkyBall;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddFlunkyBall<T>(this WebApplicationBuilder builder) where T : class, IGameServer
    {
        builder.Services.Configure<FlunkyBallSettings>(
            builder.Configuration.GetSection("FlunkyBall"));
        builder.Services.AddHostedService<ConnectorBackgroundService>();
        builder.Services.AddSingleton<IGameServer, T>();
        return builder;
    }
}