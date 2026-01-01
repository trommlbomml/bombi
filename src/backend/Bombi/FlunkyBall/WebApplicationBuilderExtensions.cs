using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FlunkyBall;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddGameServer(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<ConnectorBackgroundService>();
        return builder;
    }
}