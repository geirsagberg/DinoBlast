using System.Collections.Generic;
using BunnyLand.DesktopGL.Components;

namespace BunnyLand.DesktopGL.Messages
{
    public class InputsUpdatedMessage : INotification
    {
        public Dictionary<byte, PlayerInput> InputsByPlayerNumber { get; }

        public InputsUpdatedMessage(Dictionary<byte, PlayerInput> inputsByPlayerNumber)
        {
            InputsByPlayerNumber = inputsByPlayerNumber;
        }
    }
}
