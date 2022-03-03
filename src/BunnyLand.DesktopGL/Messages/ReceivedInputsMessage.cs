using BunnyLand.DesktopGL.Components;

namespace BunnyLand.DesktopGL.Messages;

internal class ReceivedInputMessage : INotification
{
    public byte PlayerNumber { get; }
    public PlayerInput Input { get; }

    public ReceivedInputMessage(byte playerNumber, PlayerInput input)
    {
        PlayerNumber = playerNumber;
        Input = input;
    }
}