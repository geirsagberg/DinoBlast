using BunnyLand.DesktopGL.Enums;
using LanguageExt;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    public class PlayerState
    {
        public byte PlayerNumber { get; set; }
        public Option<PlayerIndex> LocalPlayerIndex { get; set; }
        public StandingOn StandingOn { get; set; }

        public bool IsLocal => LocalPlayerIndex.IsSome;
        public bool IsBraking { get; set; } = true;
        public Option<int> PeerId { get; set; }
        public bool IsBoosting { get; set; }

        public PlayerState(Option<PlayerIndex> playerIndex)
        {
            LocalPlayerIndex = playerIndex;
        }
    }
}
