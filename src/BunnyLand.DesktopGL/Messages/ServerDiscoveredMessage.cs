using System.Net;

namespace BunnyLand.DesktopGL.Messages;

public class ServerDiscoveredMessage : INotification
{
    public IPEndPoint EndPoint { get; }

    public ServerDiscoveredMessage(IPEndPoint endPoint)
    {
        EndPoint = endPoint;
    }
}