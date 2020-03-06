namespace BunnyLand.DesktopGL.Components
{
    public class Health
    {
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }

        public Health(float maxHealth)
        {
            CurrentHealth = MaxHealth = maxHealth;
        }
    }
}
