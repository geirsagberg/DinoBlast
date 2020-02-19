using System;
using System.Collections.Generic;
using System.Linq;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>();

        public static IEnumerable<T> GetFlags<T>(this T input) where T : Enum =>
            GetValues<T>().Where(v => input.HasFlag(v));
    }
}
