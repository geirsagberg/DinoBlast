namespace BunnyLand.DesktopGL.Messages;

internal class PlayerJoinedMessage : INotification
{
    public int PeerId { get; }
    public int PlayerCount { get; }

    public PlayerJoinedMessage(int peerId, int playerCount)
    {
        PeerId = peerId;
        PlayerCount = playerCount;
    }
}