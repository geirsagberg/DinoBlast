using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Systems;
using LanguageExt;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    public class Player
    {
        public PlayerIndex PlayerIndex { get; set; }
        public StandingOn StandingOn { get; set; }
        public PlayerState State { get; set; }
        public Facing Facing { get; set; }
    }
}
