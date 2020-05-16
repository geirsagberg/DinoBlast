using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class GravityPoint
    {
        [Key(0)] public float GravityMass { get; }

        public GravityPoint(float gravityMass)
        {
            GravityMass = gravityMass;
        }
    }
}
