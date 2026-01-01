using FlunkyBall;

namespace Bombi.Service;

public sealed class SampleGameServer : IGameServer
{
    public void PrepareWorld(IGameServerContext context, CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public void RunFrame(IGameServerContext context, IList<IClient> clients, double frameTimeSeconds, long serverTimeMilliseconds)
    {
        throw new NotImplementedException();
    }

    public void OnClientConnected(IGameServerContext context, IClient client)
    {
        throw new NotImplementedException();
    }

    public void OnClientDisconnected(IGameServerContext context, IClient client)
    {
        throw new NotImplementedException();
    }
}