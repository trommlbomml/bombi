namespace Bombi.ServerInstance;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddHostedService<InstanceBackgroundService>();
        builder.Services.AddSingleton<IGameInstanceService, GameInstanceService>();
        builder.Services.Configure<InstanceSettings>(builder.Configuration.GetSection("InstanceSettings"));

        var app = builder.Build();

        app.UseMiddleware<WebSocketAcceptMiddleware>();
        app.MapControllers();
        await app.RunAsync().ConfigureAwait(false);
    }
}