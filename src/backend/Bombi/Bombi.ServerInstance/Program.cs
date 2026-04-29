using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddHostedService<InstanceBackgroundService>();
        builder.Services.AddSingleton<IGameInstanceService, GameInstanceService>();
        builder.Services.AddTransient<WebSocketAcceptMiddleware>();
        builder.Services.Configure<InstanceSettings>(builder.Configuration.GetSection("InstanceSettings"));

        var cors = builder.Configuration.GetSection("Cors").Get<string[]>() ?? [];

        if (cors.Any())
        {
            builder.Services.AddCors(options => options.AddDefaultPolicy(
                policy  => policy.WithOrigins(cors)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));
        }
        
        var app = builder.Build();

        app.UseMiddleware<WebSocketAcceptMiddleware>();
        if (cors.Any()) app.UseCors();
        app.MapControllers();
        
        await app.RunAsync().ConfigureAwait(false);
    }
}