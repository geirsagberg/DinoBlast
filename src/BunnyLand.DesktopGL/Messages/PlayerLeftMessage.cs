namespace BunnyLand.DesktopGL.Messages;

internal class PlayerLeftMessage : INotification
{
    public int PeerId { get; }

    public PlayerLeftMessage(in int peerId)
    {
        PeerId = peerId;
    }
}