using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    public class Player
    {
        public Dictionary<PlayerKey, KeyState> PlayerKeys =
            EnumHelper.GetValues<PlayerKey>().ToDictionary(k => k, _ => KeyState.None);

        public DirectionalInputs DirectionalInputs { get; set; } = new DirectionalInputs();

        public PlayerIndex PlayerIndex { get; set; }
        public StandingOn StandingOn { get; set; }
        public PlayerState State { get; set; }
        public Facing Facing { get; set; }

        public bool IsLocal { get; set; } = true;

        public bool IsBraking { get; set; } = true;
        public Option<int> PeerId { get; set; }

        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }
    }
}
