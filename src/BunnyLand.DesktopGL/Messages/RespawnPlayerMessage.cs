using BunnyLand.DesktopGL.Services;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Messages
{
    internal class RespawnPlayerMessage : INotification
    {
        public PlayerIndex PlayerIndex { get; }

        public RespawnPlayerMessage(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }
    }
}