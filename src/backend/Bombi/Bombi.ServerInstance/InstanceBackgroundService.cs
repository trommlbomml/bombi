namespace Bombi.ServerInstance;

public sealed class InstanceBackgroundService(IGameInstanceService service) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
        }

        await service.ShutdownAllInstancesAsync().ConfigureAwait(false);
    }
}