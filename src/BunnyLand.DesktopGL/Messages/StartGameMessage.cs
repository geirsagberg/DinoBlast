using BunnyLand.DesktopGL.Serialization;

namespace BunnyLand.DesktopGL.Messages
{
    internal class StartGameMessage : INotification
    {
        public FullGameState? GameState { get; }

        public StartGameMessage(FullGameState gameState = null)
        {
            GameState = gameState;
        }
    }
}
