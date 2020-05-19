using System;
using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class Lifetime
    {
        [Key(0)] public TimeSpan LifeSpan { get; }

        [Key(1)] public TimeSpan LifeSpanLeft { get; set; }

        public Lifetime(TimeSpan lifeSpan)
        {
            LifeSpanLeft = LifeSpan = lifeSpan;
        }
    }
}
