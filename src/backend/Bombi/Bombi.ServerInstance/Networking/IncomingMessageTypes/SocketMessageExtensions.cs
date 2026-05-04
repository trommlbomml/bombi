using Bombi.ServerInstance.Game;

namespace Bombi.ServerInstance.Networking.IncomingMessageTypes;

public static class SocketMessageExtensions
{
    public static MessageType GetMessageType(this SocketMessage message) 
        => (MessageType)message.Data[0];

    public static InputSnapshot[] GetInputSnapshot(this SocketMessage message)
    {
        var itemCount = message.ReadByte();
        var snapshots = new InputSnapshot[itemCount];
        for (var i = 0; i < itemCount; i++)
        {
            var tick = message.ReadInt32();
            var data = message.ReadByte();
            snapshots[i] = new InputSnapshot
            {
                ServerTick = tick,
                Left = (data & 1) == 1,
                Right = (data & 2) == 2,
                Up = (data & 4) == 4,
                Down = (data & 8) == 8,
                Action = (data & 16) == 16
            };
        }

        return snapshots;
    }
}