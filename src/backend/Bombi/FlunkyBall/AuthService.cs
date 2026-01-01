using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace FlunkyBall;

public record AuthResult(int Id, string Token);

public record User(int Id, string Name);

public interface IAuthService
{
    AuthResult? Authenticate(string name, string password);

    bool VerifyIdentity(int id, string token);
    
    User GetUser(string token);
}

internal sealed class AuthService(IClock clock) : IAuthService
{
    private const string Password = "Test";
    private static readonly TimeSpan ClientHandShakeTimeoutSeconds = TimeSpan.FromSeconds(10);
    
    private readonly List<UserData> _users = new();
    private readonly object _lock = new();

    public AuthResult? Authenticate(string name, string password)
    {
        lock (_lock)
        {
            AuthResult? authResultOrNull = null;
            if (Password == password 
                && !_users.Any(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) 
                && IsValidUserName(name))
            {
                var nextId = IdGenerator.NextId();
                var token = GenerateUniqueToken(nextId);
                _users.Add(new UserData(nextId, name, token, clock.Now()));
                
                authResultOrNull = new AuthResult(nextId, token);
            }

            return authResultOrNull;
        }
    }

    public bool VerifyIdentity(int id, string token)
    {
        lock (_lock)
        {
            return _users.Any(u => u.Id == id && u.Token == token);
        }
    }

    public User GetUser(string token)
    {
        lock (_lock)
        {
            var data = _users.First(u => u.Token == token);
            return new User(data.Id, data.Name);
        }
    }

    public void RemoveOutdatedPendingClientRequests(List<Client> clients)
    {
        lock (_lock)
        {
            var now = clock.Now();

            var usersToRemove = new List<UserData>();
            foreach (var user in _users)
            {
                var diff = now - user.JoinedTimeStamp;
                if (diff > ClientHandShakeTimeoutSeconds
                    && clients.All(c => !user.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    usersToRemove.Add(user);
                }
            }

            foreach (var userName in usersToRemove)
            {
                _users.Remove(userName);
            }
        }
    }

    private static string GenerateUniqueToken(int id)
    {
        var tokenData = new byte[36];
        BitConverter.GetBytes(id).CopyTo(tokenData, 0);
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenData, 4, 32);
        }
        return WebEncoders.Base64UrlEncode(tokenData);
    }

    private static bool IsValidUserName(string name) 
        => name.All(n => char.IsLetterOrDigit(n) || n == '_' || n == '-');

    private record UserData(int Id, string Name, string Token, DateTimeOffset JoinedTimeStamp);
}