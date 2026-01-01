namespace FlunkyBall;

public interface IGameServer
{
    void PrepareWorld(IGameServerContext context, CancellationToken stoppingToken);
    
    void RunFrame(IGameServerContext context, IList<IClient> clients, double frameTimeSeconds, long serverTimeMilliseconds);
    
    void OnClientConnected(IGameServerContext context, IClient client);
    
    void OnClientDisconnected(IGameServerContext context, IClient client);
}