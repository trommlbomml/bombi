using System.Buffers.Binary;
using System.Net.Sockets;

namespace Bombi.ServerInstance.Networking;

public interface ISerializerTarget
{
    void Write(int value);
}

public sealed class SocketMessage : ISerializerTarget
{
    public static readonly SocketMessage Empty = new (SocketMessageFactory.Empty, 0);
    
    private readonly SocketMessageFactory _factory;
    private readonly byte[] _data;
    
    private int _length;

    public SocketMessage(SocketMessageFactory factory) : this(factory, 128)
    {
    }

    private SocketMessage(SocketMessageFactory factory, int messageCapacity)
    {
        _factory = factory;
        _data = new byte[messageCapacity];
        Data = new ArraySegment<byte>(_data, 0, 0);
    }
    
    public bool IsEmpty => Data.Count == 0;
    
    public ArraySegment<byte> Data { get; private set; }

    public void Return()
        => _factory.Return(this);

    public void Write(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_data.AsSpan(Data.Count), value);
        _length += sizeof(int);
        Data = new ArraySegment<byte>(_data, 0, _length);
    }

    public void Write(ArraySegment<byte> value)
    {
        value.CopyTo(_data, _length);
        _length += value.Count;
    }
}