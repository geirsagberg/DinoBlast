using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Messages
{
    internal class ResetWorldMessage
    {
    }

    internal class RespawnPlayerMessage
    {
        public PlayerIndex PlayerIndex { get; }

        public RespawnPlayerMessage(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }
    }
}
