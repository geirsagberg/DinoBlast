using System;

namespace BunnyLand.DesktopGL.Components
{
    public class Lifetime
    {
        public TimeSpan LifeSpan { get; }
        public TimeSpan LifeSpanLeft { get; set; }

        public Lifetime(TimeSpan lifeSpan)
        {
            LifeSpanLeft = LifeSpan = lifeSpan;
        }
    }
}
