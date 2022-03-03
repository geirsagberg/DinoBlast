using System.Net;

namespace BunnyLand.DesktopGL.Messages;

public class JoinServerRequest : IRequest<bool>
{
    public IPEndPoint EndPoint { get; }

    public JoinServerRequest(IPEndPoint endPoint)
    {
        EndPoint = endPoint;
    }
}