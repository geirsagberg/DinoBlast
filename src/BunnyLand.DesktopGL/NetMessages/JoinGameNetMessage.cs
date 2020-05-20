using BunnyLand.DesktopGL.Enums;
using MessagePack;

namespace BunnyLand.DesktopGL.NetMessages
{
    [MessagePackObject]
    public class JoinGameNetMessage : INetMessage
    {
        [Key(0)] public byte PlayerCount { get; }

        [IgnoreMember] public NetMessageType NetMessageType { get; } = NetMessageType.JoinGameRequest;

        public JoinGameNetMessage(byte playerCount)
        {
            PlayerCount = playerCount;
        }
    }
}
