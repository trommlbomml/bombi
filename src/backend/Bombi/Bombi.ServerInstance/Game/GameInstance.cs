namespace Bombi.ServerInstance.Game;

public sealed class GameInstance
{
    public GameInstanceState State { get; set; }

    public List<Client> Clients { get; } = new();

    public void ClientJoining(int id, string name)
    {
        Clients.Add(new Client
        {
            Id = id,
            Name = name,
            IsJoined = false
        });

        if (State == GameInstanceState.Empty)
        {
            State = GameInstanceState.Lobby;
        }
    }

    public void ClientJoined(int id)
    {
        Clients.Single(c => c.Id == id).IsJoined = true;
    }
    
    public void RunFrame(int serverTick, double elapsedSeconds)
    {
        
    }
}