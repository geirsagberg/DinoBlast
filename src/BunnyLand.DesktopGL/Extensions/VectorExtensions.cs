using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 Scale(this Vector2 vector2, float scale) =>
            new Vector2(vector2.X * scale, vector2.Y * scale);

        public static Vector2 NormalizedOrZero(this Vector2 vector2) =>
            vector2 == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector2);
    }
}
