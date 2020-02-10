using System;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class NumberExtensions
    {
        public static float ToFloat(this double d) => Convert.ToSingle(d);

        /// Return f, or a minimum of min
        public static float WithLowerBound(this float f, float min) => Math.Max(f, min);
    }
}
