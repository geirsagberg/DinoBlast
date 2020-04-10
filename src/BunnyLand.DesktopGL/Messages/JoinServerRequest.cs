using System;
using BunnyLand.DesktopGL.Services;

namespace BunnyLand.DesktopGL.Messages
{
    public class JoinServerRequest : IRequest<bool>
    {
        public OnlineType OnlineType { get; }

        public JoinServerRequest(OnlineType onlineType)
        {
            if (onlineType == OnlineType.Offline) throw new ArgumentOutOfRangeException(nameof(onlineType));
            OnlineType = onlineType;
        }
    }
}
