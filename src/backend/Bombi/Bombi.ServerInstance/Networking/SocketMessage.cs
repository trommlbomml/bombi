using System.Net.WebSockets;
using System.Text;

namespace Bombi.ServerInstance.Networking;


public sealed class SocketMessage
{
    public static SocketMessage Empty { get; } = new();
    
    public bool Compress { get; private set; }
    
    public SocketMessage(string content)
    {
        Data = Encoding.UTF8.GetBytes(content);
        MessageType = WebSocketMessageType.Text;
    }

    private SocketMessage()
    {
        Data = [];
        MessageType = WebSocketMessageType.Binary;
    }

    public SocketMessage(ArraySegment<byte> data, bool compress = false)
    {
        Data = data.ToArray();
        Compress = compress;
        MessageType = WebSocketMessageType.Binary;
    }

    public bool IsEmpty => Data.Length == 0;
    
    public WebSocketMessageType MessageType { get; }
    
    public byte[] Data { get; }
    
    public string ReadAsString()
        => Encoding.UTF8.GetString(Data); 
}