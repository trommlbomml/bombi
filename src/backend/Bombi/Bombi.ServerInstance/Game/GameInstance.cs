using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public sealed class GameInstance
{
    private Level _level = new();
    
    public GameInstanceState State { get; set; }

    public List<Client> Clients { get; } = new();

    public void RunFrame(int serverTick, double elapsedSeconds)
    {
        
    }

    public void SerializeGameState(int serverTick, ISerializerTarget target)
    {
        target.Write(serverTick);
        _level.SerializeTo(target);
    }
}