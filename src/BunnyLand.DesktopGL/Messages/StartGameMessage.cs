namespace BunnyLand.DesktopGL.Messages
{
    internal class StartGameMessage
    {
        public GameOptions GameOptions { get; }

        public StartGameMessage(GameOptions gameOptions)
        {
            GameOptions = gameOptions;
        }
    }
}