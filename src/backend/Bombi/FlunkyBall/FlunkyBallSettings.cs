namespace FlunkyBall;

public sealed class FlunkyBallSettings
{
    public TimeSpan ClientHandShakeTimeout { get; set; } = TimeSpan.FromSeconds(10);
    
    public TimeSpan ClientHeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public int MaxClients { get; set; } = 30;
    
    public int FixedServerFramesPerSecond { get; set; } = 30;

    public string[] WebSocketChannels { get; set; } = ["Default"];
    
    public int HeartbeatChannelIndex { get; set; } = 0;
}