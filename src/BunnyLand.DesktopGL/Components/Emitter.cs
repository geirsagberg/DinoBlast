using System;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Components
{
    public class Emitter
    {
        public TimeSpan EmitInterval { get; set; }
        public bool IsEmitting { get; set; }
        public TimeSpan TimeSinceLastEmit { get; set; }

        public Emit? Emit { get; set; }
    }

    public delegate void Emit(Entity entity);
}
