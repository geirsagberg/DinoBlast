using BunnyLand.DesktopGL.Enums;
using LiteNetLib.Utils;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class NetExtensions
    {
        public static void Put(this NetDataWriter writer, INetMessage netMessage)
        {
            writer.Put((byte) netMessage.NetMessageType);
            writer.Put(netMessage.Payload);
        }

        public static void Put(this NetDataWriter writer, NetMessageType netMessageType, byte[] bytes)
        {
            writer.Put((byte) netMessageType);
            writer.Put(bytes);
        }
    }
}
