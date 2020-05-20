using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Serialization;
using LiteNetLib;
using LiteNetLib.Utils;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class NetExtensions
    {
        public static void Put<T>(this NetDataWriter writer, T netMessage, Serializer serializer) where T : INetMessage
        {
            writer.Put((byte) netMessage.NetMessageType);
            writer.Put(serializer.Serialize(netMessage));
        }

        public static void Put(this NetDataWriter writer, NetMessageType netMessageType, byte[] bytes)
        {
            writer.Put((byte) netMessageType);
            writer.Put(bytes);
        }

        public static void Send<T>(this NetPeer peer, T netMessage, DeliveryMethod deliveryMethod, Serializer serializer) where T : INetMessage
        {
            var writer = new NetDataWriter();
            writer.Put(netMessage, serializer);
            peer.Send(writer, deliveryMethod);
        }
    }
}
