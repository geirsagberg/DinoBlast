using BunnyLand.DesktopGL.Serialization;

namespace BunnyLand.DesktopGL.Messages
{
    internal class ResetWorldMessage : INotification
    {
        public FullGameState? GameState { get; }
        public int FrameCounter { get; }

        public ResetWorldMessage(FullGameState? gameState = null, int frameCounter = 0)
        {
            FrameCounter = frameCounter;
            GameState = gameState;
        }
    }
}
