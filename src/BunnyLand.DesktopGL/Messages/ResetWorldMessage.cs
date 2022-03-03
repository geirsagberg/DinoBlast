using BunnyLand.DesktopGL.Serialization;

namespace BunnyLand.DesktopGL.Messages;

internal class ResetWorldMessage : INotification
{
    public FullGameState? GameState { get; }
    public int FrameCounter { get; }

    public ResetWorldMessage(FullGameState? gameState = null)
    {
        FrameCounter = gameState?.FrameCounter ?? 0;
        GameState = gameState;
    }
}