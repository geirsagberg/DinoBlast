using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using LiteNetLib.Utils;
using MessagePack;

namespace BunnyLand.DesktopGL.NetMessages
{
    public class ClientActionNetMessage : INetMessage
    {
        public NetMessageType NetMessageType { get; } = NetMessageType.ClientAction;
        public INetSerializable Payload { get; }

        public ClientActionNetMessage(INetSerializable payload)
        {
            Payload = payload;
        }
    }

    [MessagePackObject]
    public class ClientAction
    {
        [Key(0)] public int Frame { get; }

        [Key(1)] public PlayerInput PlayerInput { get; }

        public ClientAction(int frame, PlayerInput playerInput)
        {
            Frame = frame;
            PlayerInput = playerInput;
        }
    }
}
