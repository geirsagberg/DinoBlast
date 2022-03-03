using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using MessagePack;

namespace BunnyLand.DesktopGL.NetMessages;

[MessagePackObject]
public class InputUpdateNetMessage : INetMessage
{
    [Key(0)] public byte PlayerNumber { get; }

    [Key(1)] public PlayerInput Input { get; }

    [IgnoreMember] public NetMessageType NetMessageType { get; } = NetMessageType.PlayerInputs;

    public InputUpdateNetMessage(byte playerNumber, PlayerInput input)
    {
        PlayerNumber = playerNumber;
        Input = input;
    }
}