using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public sealed class GameInstance
{
    private readonly Level _level = new();
    private readonly List<Client> _clients = new();
    
    public GameInstanceState State { get; set; }

    public void OnClientJoining(int id, string name)
    {
        if (State == GameInstanceState.Empty)
        {
            State = GameInstanceState.Lobby;
        }
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

    public void OnClientLeft(int id) 
        => _clients.RemoveAll(c => c.Id == id);

    public void StartGame()
    {
        if (State != GameInstanceState.Lobby)
        {
            throw new InvalidOperationException($"Cannot start when state is not {GameInstanceState.Lobby} but {State}");
        }
        foreach (var client in _clients)
        {
            client.Position = _level.GetStartPositionForPlayer(client.Index);
        }
        State = GameInstanceState.ActiveMatch;
    }

    public void RunFrame(int serverTick, double elapsedSeconds)
    {
        if (State != GameInstanceState.ActiveMatch) return;
        
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

    public void AddInput(int id, InputSnapshot[] snapshots)
    {
        var client = _clients.First(c => c.Id == id);
        client.InputSnapshots.AddRange(snapshots);
    }
}