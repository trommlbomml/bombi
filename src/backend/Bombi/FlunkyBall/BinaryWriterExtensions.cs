namespace FlunkyBall;

public static class BinaryWriterExtensions
{
    public static void WriteBoolean(this BinaryWriter reader, bool value) 
        => reader.Write(value ? (byte)1 : (byte)0);
}