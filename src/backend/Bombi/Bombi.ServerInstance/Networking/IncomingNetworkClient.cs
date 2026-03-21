using System.Net.WebSockets;

namespace Bombi.ServerInstance.Networking;

public sealed record IncomingNetworkClient(
    string Token, 
    WebSocket Socket, 
    TaskCompletionSource TaskCompletionSource);