using System;

namespace BunnyLand.DesktopGL.Components
{
    public class Emitter
    {
        public TimeSpan EmitInterval { get; set; }
        public bool IsEmitting { get; set; }
        public TimeSpan TimeSinceLastEmit { get; set; }

        public EmitterType EmitterType { get; set; }
    }

    public enum EmitterType
    {
        Bullet
    }
}
