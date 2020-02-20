using System;
using System.Collections.Generic;
using System.Linq;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>();
    }
}
