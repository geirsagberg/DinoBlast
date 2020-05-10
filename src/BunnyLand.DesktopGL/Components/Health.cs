using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class Health
    {
        [Key(0)] public float MaxHealth { get; set; }

        [Key(1)] public float CurrentHealth { get; set; }

        public Health(float maxHealth)
        {
            CurrentHealth = MaxHealth = maxHealth;
        }
    }
}
