using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Extensions;

public static class RandomExtensions
{
    public static Vector2 NextUnitVector(this Random random)
    {
        var num = random.NextAngle();
        return new Vector2((float) Math.Cos(num), (float) Math.Sin(num));
    }
}