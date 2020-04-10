using BunnyLand.DesktopGL.Services;

namespace BunnyLand.DesktopGL.Messages
{
    internal class StartGameMessage : INotification
    {
        public GameOptions GameOptions { get; }

        public StartGameMessage(GameOptions gameOptions)
        {
            GameOptions = gameOptions;
        }
    }
}
