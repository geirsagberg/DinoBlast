using BunnyLand.DesktopGL.Enums;
using LiteNetLib.Utils;

namespace BunnyLand.DesktopGL.NetMessages
{
    public class ClientActionNetMessage : INetMessage
    {
        public ClientActionNetMessage(INetSerializable payload)
        {
            Payload = payload;
        }

        public NetMessageType NetMessageType { get; } = NetMessageType.ClientAction;
        public INetSerializable Payload { get; }
    }

    public class ClientAction
    {
        public int Frame { get; set; }

    }
}
