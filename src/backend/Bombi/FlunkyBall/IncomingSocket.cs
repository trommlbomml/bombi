using System.Net.WebSockets;

namespace FlunkyBall;

internal sealed record IncomingSocket(WebSocket Socket, string Type, TaskCompletionSource TaskCompletionSource);