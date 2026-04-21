using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public sealed class GameInstance
{
    public GameInstanceState State { get; set; }

    public List<Client> Clients { get; } = new();

    public void RunFrame(int serverTick, double elapsedSeconds)
    {
        
    }

    public void SerializeGameState(int serverTick, ISerializerTarget target)
    {
        
    }
}