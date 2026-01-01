namespace FlunkyBall;

public interface IGameServer
{
    void PrepareWorld(CancellationToken stoppingToken);
    
    void RunFrame(IList<IClient> clients, double frameTimeSeconds, long serverTimeMilliseconds);
}