using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class GravityPoint : ISerializableComponent
    {
        [Key(0)] public float GravityMass { get; }

        public GravityPoint(float gravityMass)
        {
            GravityMass = gravityMass;
        }
    }
}
