using System;

namespace BunnyLand.DesktopGL.Messages
{
    public class JoinGameMessage
    {
        public OnlineType OnlineType { get; }

        public JoinGameMessage(OnlineType onlineType)
        {
            if (OnlineType == OnlineType.Local) throw new ArgumentOutOfRangeException(nameof(onlineType));
            OnlineType = onlineType;
        }
    }
}
