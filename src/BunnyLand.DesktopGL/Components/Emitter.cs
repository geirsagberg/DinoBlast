using System;
using MessagePack;

namespace BunnyLand.DesktopGL.Components;

[MessagePackObject]
public class Emitter
{
    [Key(0)] public TimeSpan EmitInterval { get; set; }

    [Key(1)] public bool IsEmitting { get; set; }

    [Key(2)] public TimeSpan TimeSinceLastEmit { get; set; }

    [Key(3)] public EmitterType EmitterType { get; set; }
}

public enum EmitterType
{
    Bullet
}