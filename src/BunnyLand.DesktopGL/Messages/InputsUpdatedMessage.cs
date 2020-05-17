using System.Collections.Generic;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using MessagePack;

namespace BunnyLand.DesktopGL.Messages
{
    // HACK: Reusing hub message as net message; not kosher...
    [MessagePackObject]
    public class InputsUpdatedMessage : INotification, INetMessage
    {
        [Key(0)] public Dictionary<byte, PlayerInput> InputsByPlayerNumber { get; }

        [IgnoreMember] public NetMessageType NetMessageType => NetMessageType.PlayerInputs;

        public InputsUpdatedMessage(Dictionary<byte, PlayerInput> inputsByPlayerNumber)
        {
            InputsByPlayerNumber = inputsByPlayerNumber;
        }
    }
}
