using System.Collections.Generic;
using BunnyLand.DesktopGL.Components;

namespace BunnyLand.DesktopGL.Messages
{
    internal class ReceivedInputsMessage : INotification
    {
        public Dictionary<byte, PlayerInput> InputsByPlayerNumber { get; }

        public ReceivedInputsMessage(Dictionary<byte, PlayerInput> inputsByPlayerNumber)
        {
            InputsByPlayerNumber = inputsByPlayerNumber;
        }
    }
}