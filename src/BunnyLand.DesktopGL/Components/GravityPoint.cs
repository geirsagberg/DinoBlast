using MessagePack;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class GravityPoint
    {
        [Key(0)] private readonly Transform2 transform;

        [Key(1)] public Vector2 Position => transform.Position;

        [Key(2)] public float GravityMass { get; set; }

        public GravityPoint(Transform2 transform, float gravityMass)
        {
            this.transform = transform;
            GravityMass = gravityMass;
        }
    }
}
