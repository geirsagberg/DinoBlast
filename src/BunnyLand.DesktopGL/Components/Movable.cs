using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class Movable
    {
        public Transform2 Transform { get; }

        public Vector2 Position => Transform.Position;

        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float GravityMultiplier { get; set; }
        public Vector2 GravityPull { get; set; }

        public float BrakingForce { get; set; }
        public bool WrapAround { get; set; } = true;

        public Movable(Transform2 transform, float gravityMultiplier = 1)
        {
            Transform = transform;
            GravityMultiplier = gravityMultiplier;
        }
    }
}
