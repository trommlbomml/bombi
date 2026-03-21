namespace Bombi.ServerInstance.Game;

public sealed class Client
{
    public int Id { get; set; }
    
    public bool IsJoined { get; set; }
    
    public required string Name { get; set; }
}