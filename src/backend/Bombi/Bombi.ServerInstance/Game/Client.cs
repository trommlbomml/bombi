using System.Numerics;
using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public sealed class Client
{
    public int Id { get; set; }
    
    public int Index { get; set; }
    
    public bool IsJoined { get; set; }
    
    public required string Name { get; set; }
    
    public Vector2 Position { get; set; }

    public List<InputSnapshot> InputSnapshots { get; } = new();

    public void SerializeTo(ISerializerTarget target)
    {
        target.Write(Id);
        target.Write((byte)Index);
        target.Write(Position.X);
        target.Write(Position.Y);
        target.Write(Name);
    }
}