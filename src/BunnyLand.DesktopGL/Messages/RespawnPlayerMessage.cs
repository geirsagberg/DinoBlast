namespace BunnyLand.DesktopGL.Messages;

internal class RespawnPlayerMessage : INotification
{
    public byte PlayerNumber { get; }

    public RespawnPlayerMessage(byte playerNumber)
    {
        PlayerNumber = playerNumber;
    }
}