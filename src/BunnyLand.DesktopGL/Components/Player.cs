using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Systems;

namespace BunnyLand.DesktopGL.Components
{
    public class Player
    {
        public StandingOn StandingOn { get; set; }
        public PlayerState State { get; set; }
        public Facing Facing { get; set; }
    }

    public class PhysicalObject
    {

    }
}
