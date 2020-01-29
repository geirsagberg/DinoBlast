using System;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class NumberExtensions
    {
        public static float ToFloat(this double d) => Convert.ToSingle(d);
    }
}