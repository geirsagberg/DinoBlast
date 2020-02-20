using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class GeometryExtensions
    {
        public static Vector2 Scale(this Vector2 vector2, float scale) =>
            new Vector2(vector2.X * scale, vector2.Y * scale);

        public static Vector2 NormalizedOrZero(this Vector2 vector2) =>
            vector2 == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector2);

        public static Vector2 WidthVector(this Size2 size) => new Vector2(size.Width, 0);
        public static Vector2 HeightVector(this Size2 size) => new Vector2(0, size.Height);

        public static Vector2 WidthVector(this RectangleF rectangle) => new Vector2(rectangle.Width, 0);
        public static Vector2 HeightVector(this RectangleF rectangle) => new Vector2(0, rectangle.Height);

        public static bool IsBetween(this float f, float a, float b) => f >= (a < b ? a : b) && f <= (a < b ? b : a);

        public static bool Contains(this RectangleF first, RectangleF second) =>
            first.Contains(second.TopLeft) && first.Contains(second.BottomRight);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SubtractLength(this Vector2 vector, float length) =>
            vector.NormalizedOrZero() * (vector.Length() - length);

        public static void Wrap(this Transform2 transform, RectangleF levelSize)
        {
            if (transform.Parent != null) return;

            if (transform.Position.X < 0) {
                transform.Position += levelSize.WidthVector();
            } else if (transform.Position.X >= levelSize.Width) {
                transform.Position -= levelSize.WidthVector();
            }

            if (transform.Position.Y < 0) {
                transform.Position += levelSize.HeightVector();
            } else if (transform.Position.Y >= levelSize.Height) {
                transform.Position -= levelSize.HeightVector();
            }
        }
    }
}
