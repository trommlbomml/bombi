using FlunkyBall;

namespace Bombi.Service;

public class SampleGameServer : IGameServer
{
    public void PrepareWorld(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public void RunFrame(IList<IClient> clients, double frameTimeSeconds, long serverTimeMilliseconds)
    {
        throw new NotImplementedException();
    }
}