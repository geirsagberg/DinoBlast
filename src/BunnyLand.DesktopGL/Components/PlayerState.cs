using BunnyLand.DesktopGL.Enums;
using LanguageExt;
using MessagePack;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class PlayerState
    {
        [Key(0)] public byte PlayerNumber { get; }

        [Key(1)] public bool IsBraking { get; set; } = true;

        [IgnoreMember] public Option<PlayerIndex> LocalPlayerIndex { get; set; }

        [IgnoreMember] public StandingOn StandingOn { get; set; }

        [IgnoreMember] public bool IsLocal => LocalPlayerIndex.IsSome;



        // [IgnoreMember]
        // public Option<(int, PlayerIndex)> PeerIdAndRemotePlayerIndex { get; set; }

        [IgnoreMember] public bool IsBoosting { get; set; }

        public PlayerState(byte playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public PlayerState(byte playerNumber, Option<PlayerIndex> playerIndex)
        {
            PlayerNumber = playerNumber;
            LocalPlayerIndex = playerIndex;
        }
    }
}
