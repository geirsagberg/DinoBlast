using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class GravityPoint
    {
        private readonly Transform2 transform;

        public Vector2 Position => transform.Position;

        public float GravityMass { get; set; }

        public GravityPoint(Transform2 transform, float gravityMass)
        {
            this.transform = transform;
            GravityMass = gravityMass;
        }
    }
}
