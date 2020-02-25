using System;

namespace BunnyLand.DesktopGL.Components
{
    public class Lifetime
    {
        public TimeSpan CreatedAt { get; }
        public TimeSpan LifeSpan { get; }

        public Lifetime(TimeSpan createdAt, TimeSpan lifeSpan)
        {
            CreatedAt = createdAt;
            LifeSpan = lifeSpan;
        }
    }
}
