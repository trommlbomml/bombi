using System.Buffers.Binary;
using System.Text;

namespace Bombi.ServerInstance.Networking;

public interface ISerializerTarget
{
    void Write(int value);
    
    void Write(double value);

    void Write(string value);
    
    void Write(ArraySegment<byte> value);

    void Write(byte value);
}

public sealed class SocketMessage : ISerializerTarget
{
    private const int MaxMessageLength = 1014;
    
    public static readonly SocketMessage Empty = new (SocketMessageFactory.Empty, 0);
    
    private readonly SocketMessageFactory _factory;
    private readonly byte[] _data;
    
    private int _length;

    public SocketMessage(SocketMessageFactory factory) : this(factory, MaxMessageLength)
    {
    }

    private SocketMessage(SocketMessageFactory factory, int messageCapacity)
    {
        _factory = factory;
        _data = new byte[messageCapacity];
    }
    
    public bool IsEmpty => Data.Count == 0;
    
    public ArraySegment<byte> Data => new(_data, 0, _length);

    public void Return()
        => _factory.Return(this);

    public void Write(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_data.AsSpan(Data.Count), value);
        _length += sizeof(int);
    }

    public void Write(double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(_data.AsSpan(Data.Count), value);
        _length += sizeof(double);
    }

    public void Write(string value)
    {
        Write(value.Length);
        Write(Encoding.UTF8.GetBytes(value));
        _length += value.Length + sizeof(int);
    }

    public void Write(ArraySegment<byte> value)
    {
        value.CopyTo(_data, _length);
        _length += value.Count;
    }

    public void Write(byte value)
    {
        _data[_length++] = value;
    }
}