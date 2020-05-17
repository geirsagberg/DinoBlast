using System.Collections.Generic;
using BunnyLand.DesktopGL.Components;

namespace BunnyLand.DesktopGL.Messages
{
    public class SendInputsRequest : IRequest
    {
        public Dictionary<int, PlayerInput> InputsByFrame { get; }

        public SendInputsRequest(Dictionary<int, PlayerInput> inputsByFrame)
        {
            InputsByFrame = inputsByFrame;
        }
    }
}