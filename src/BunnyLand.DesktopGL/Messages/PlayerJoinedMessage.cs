namespace BunnyLand.DesktopGL.Messages
{
    internal class PlayerJoinedMessage : INotification
    {
        public int PeerId { get; }

        public PlayerJoinedMessage(in int peerId)
        {
            PeerId = peerId;
        }
    }
}
