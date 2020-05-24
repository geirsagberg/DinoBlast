using BunnyLand.DesktopGL.Enums;
using MessagePack;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class PlayerState
    {
        [Key(0)] public byte PlayerNumber { get; set; }

        [Key(1)] public bool IsBraking { get; set; } = true;

        [Key(2)] public int? PeerId { get; set; }

        [Key(3)] public StandingOn StandingOn { get; set; }

        [Key(4)] public int? StandingOnEntity { get; set; }

        [IgnoreMember] public PlayerIndex? LocalPlayerIndex { get; set; }

        [IgnoreMember] public bool IsLocal => LocalPlayerIndex.HasValue;

        [IgnoreMember] public bool IsBoosting { get; set; }
    }
}
