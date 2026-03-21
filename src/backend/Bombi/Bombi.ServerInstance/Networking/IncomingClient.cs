using System.Net.WebSockets;

namespace Bombi.ServerInstance.Networking;

public sealed record IncomingClient(
    string Token, 
    WebSocket Socket, 
    TaskCompletionSource TaskCompletionSource);