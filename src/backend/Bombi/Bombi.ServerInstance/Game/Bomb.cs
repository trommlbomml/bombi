using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public class Bomb
{
    public Vector2 Position { get; set; }
    
    public double RemainingTickTime { get; set; }

    public void SerializeTo(ISerializerTarget target)
    {
        target.Write(Position.X);
        target.Write(Position.Y);
        target.Write(RemainingTickTime);
    }
}