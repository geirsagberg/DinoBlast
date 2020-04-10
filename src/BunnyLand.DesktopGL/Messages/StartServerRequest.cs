using System;

namespace BunnyLand.DesktopGL.Messages
{
    public class StartServerRequest : IRequest<bool>
    {
        public OnlineType OnlineType { get; }

        public StartServerRequest(OnlineType onlineType)
        {
            if (onlineType == OnlineType.Offline) throw new ArgumentOutOfRangeException(nameof(onlineType));
            OnlineType = onlineType;
        }
    }
}
