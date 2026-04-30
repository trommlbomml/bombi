using System.Security.Cryptography;
using Bombi.ServerInstance.Game;
using Bombi.ServerInstance.Networking;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Bombi.ServerInstance;

public interface IGameInstanceService
{
    string StartCreateInstance(string userName);

    string StartJoinInstance(Guid instanceId, string userName);

    void AcceptIncomingWebSocket(IncomingNetworkClient networkClient);

    Task ShutdownAllInstancesAsync();
}

public sealed class GameInstanceService : IGameInstanceService
{
    private readonly List<GameInstanceTask> _instanceTasks;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public GameInstanceService(IOptions<InstanceSettings> options, ILogger<IGameInstanceService> logger)
    {
        _instanceTasks = Enumerable
            .Range(1, options.Value.MaxInstances)
            .Select(_ => new GameInstanceTask(options.Value, _cancellationTokenSource.Token, logger))
            .ToList();
    }

    public string StartCreateInstance(string userName)
    {
        var empty = _instanceTasks.FirstOrDefault(x => x.State == GameInstanceState.Empty);
            
        if (empty == null)
        {
            throw new BadHttpRequestException("No Empty Lobby");
        }

        var clientId = empty.ClientJoining(userName);

        return GenerateUniqueToken(empty, clientId);
    }

    public string StartJoinInstance(Guid instanceId, string userName)
    {
        var existingGameInstance = _instanceTasks.FirstOrDefault(x => x.Id == instanceId);
        if (existingGameInstance == null)
        {
            throw new BadHttpRequestException("Lobby not found");
        }
            
        var clientId = existingGameInstance.ClientJoining(userName);

        return GenerateUniqueToken(existingGameInstance, clientId);
    }

    public void AcceptIncomingWebSocket(IncomingNetworkClient networkClient)
    {
        var byteArray = WebEncoders.Base64UrlDecode(networkClient.Token);
        
        var instanceId = new Guid(new ReadOnlySpan<byte>(byteArray, 0, 16));
        var clientId = BitConverter.ToInt32(new ReadOnlySpan<byte>(byteArray, 16, 4));
        
        var existingGameInstance = _instanceTasks.Single(x => x.Id == instanceId);
        existingGameInstance.ClientJoined(clientId, networkClient);
    }

    public async Task ShutdownAllInstancesAsync()
    {
        await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await Task.WhenAll(_instanceTasks.Select(x => x.Task)).ConfigureAwait(false);
    }
    
    private static string GenerateUniqueToken(GameInstanceTask gameInstanceTask, int clientId)
    {
        const int tokenLength = 32;
        var tokenData = new byte[tokenLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            var idAsBytes = gameInstanceTask.Id.ToByteArray();
            idAsBytes.CopyTo(tokenData, 0);
            
            var numberAsBytes = BitConverter.GetBytes(clientId);
            numberAsBytes.CopyTo(tokenData, idAsBytes.Length);
            
            rng.GetBytes(
                tokenData, 
                idAsBytes.Length + numberAsBytes.Length,
                tokenLength - idAsBytes.Length - numberAsBytes.Length);
        }
        return WebEncoders.Base64UrlEncode(tokenData);
    }
}