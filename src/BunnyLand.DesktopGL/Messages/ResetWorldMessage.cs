using BunnyLand.DesktopGL.Serialization;

namespace BunnyLand.DesktopGL.Messages
{
    internal class ResetWorldMessage : INotification
    {
        public FullGameState? GameState { get; }

        public ResetWorldMessage(FullGameState? gameState = null)
        {
            GameState = gameState;
        }
    }
}
