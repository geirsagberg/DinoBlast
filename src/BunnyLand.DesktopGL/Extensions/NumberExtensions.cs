using System;

namespace BunnyLand.DesktopGL.Extensions;

public static class NumberExtensions
{
    public static float ToFloat(this double d) => Convert.ToSingle(d);

    /// Return f, or a minimum of min
    public static float WithLowerBound(this float f, float min) => Math.Max(f, min);

    public static float Constrain(this float f, float min, float max) => f < min ? min : f > max ? max : f;
}