namespace FlunkyBall;

internal sealed class IncomingClient(
    string identityToken, IEnumerable<IncomingSocket> sockets)
{
    public string IdentityToken { get; } = identityToken;

    public IEnumerable<IncomingSocket> IncomingSockets { get; } = sockets;
}