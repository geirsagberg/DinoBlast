using BunnyLand.DesktopGL.Enums;
using MessagePack;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class Movable
    {
        [Key(0)]
        public Vector2 Velocity { get; set; }

        [Key(1)]
        public Vector2 Acceleration { get; set; }

        [Key(2)]
        public float GravityMultiplier { get; set; } = 1f;

        [Key(3)]
        public Vector2 GravityPull { get; set; }

        [Key(4)]
        public float BrakingForce { get; set; }

        [Key(5)]
        public LevelBoundsBehavior LevelBoundsBehavior { get; set; } = LevelBoundsBehavior.Wrap;

        [Key(6)]
        public bool ExpandsCamera { get; set; }

        public Movable()
        {
        }

        public Movable(float gravityMultiplier)
        {
            GravityMultiplier = gravityMultiplier;
        }
    }
}
