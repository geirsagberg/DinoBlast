using BunnyLand.DesktopGL.Enums;
using LiteNetLib.Utils;

namespace BunnyLand.DesktopGL
{
    public interface INetMessage
    {
        NetMessageType NetMessageType { get; }
        INetSerializable Payload { get; }
    }
}
