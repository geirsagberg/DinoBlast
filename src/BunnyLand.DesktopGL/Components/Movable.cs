using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class Movable
    {
        private Transform2 Transform { get; }

        public Vector2 Position => Transform.Position;

        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float GravityMultiplier { get; set; }
        public Vector2 GravityPull { get; set; }

        public Movable(Transform2 transform, float gravityMultiplier = 1)
        {
            Transform = transform;
            GravityMultiplier = gravityMultiplier;
        }
    }
}
