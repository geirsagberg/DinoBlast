using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class Damaging : ISerializableComponent
    {
        [Key(0)] public float Damage { get; }

        public Damaging(float damage)
        {
            Damage = damage;
        }
    }
}
