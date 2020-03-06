using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    public class Player
    {
        public Dictionary<PlayerKey, KeyState> PlayerKeys =
            EnumHelper.GetValues<PlayerKey>().ToDictionary(k => k, _ => KeyState.None);

        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public PlayerIndex PlayerIndex { get; set; }
        public StandingOn StandingOn { get; set; }
        public PlayerState State { get; set; }
        public Facing Facing { get; set; }
        public bool IsBraking { get; set; } = true;
    }
}
