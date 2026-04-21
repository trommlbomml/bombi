using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public sealed class GameInstance
{
    private readonly Level _level = new();
    private readonly List<Client> _clients = new();
    
    public GameInstanceState State { get; set; }


    public void OnClientJoining(int id, string name)
    {
        var client = new Client
        {
            Id = id,
            Name = name,
            Index = _clients.Count,
            IsJoined = false,
        };
        _clients.Add(client);
    }

    public void OnClientJoined(int id)
    {
        var client = _clients.First(c => c.Id == id);
        client.IsJoined = true;
    }

    public void RunFrame(int serverTick, double elapsedSeconds)
    {
        
    }

    public void SerializeGameState(int serverTick, ISerializerTarget target)
    {
        target.Write(serverTick);
        _level.SerializeTo(target);
        
        target.Write((byte)_clients.Count);
        foreach (var client in _clients)
        {
            client.SerializeTo(target);
        }
    }
}