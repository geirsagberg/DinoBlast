using System;
using LanguageExt;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Components
{
    public class Emitter
    {
        public TimeSpan EmitInterval { get; set; }
        public bool IsEmitting { get; set; }
        public Option<TimeSpan> LastEmitted { get; set; }

        public Action<Entity, TimeSpan>? Emit { get; set; }
    }
}
