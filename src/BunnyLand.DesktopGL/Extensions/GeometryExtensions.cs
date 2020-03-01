using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Vector3 = System.Numerics.Vector3;

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

        public static RectangleF Expand(this RectangleF rectangle, Vector2 direction)
        {
            if (direction == Vector2.Zero) return rectangle;
            var translated = rectangle;
            translated.Position += direction;
            return rectangle.Union(translated);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CircleF Inflate(this CircleF circle, float addRadius) =>
            new CircleF(circle.Center, circle.Radius + addRadius);

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

        /// <summary>
        ///     Calculate a's penetration into b
        /// </summary>
        /// <param name="a">The penetrating shape.</param>
        /// <param name="b">The shape being penetrated.</param>
        /// <returns>The distance vector from the edge of b to a's Position</returns>
        public static Vector2 CalculatePenetrationVector(this IShapeF a, IShapeF b)
        {
            if (!a.Intersects(b)) return Vector2.Zero;
            var penetrationVector = a switch {
                RectangleF rectA when b is RectangleF rectB => PenetrationVector(rectA, rectB),
                CircleF circA when b is CircleF circB => PenetrationVector(circA, circB),
                CircleF circA when b is RectangleF rectB => PenetrationVector(circA, rectB),
                RectangleF rectA when b is CircleF circB => PenetrationVector(rectA, circB),
                _ => throw new NotSupportedException("Shapes must be either a CircleF or RectangleF")
            };
            return penetrationVector;
        }

        private static Vector2 PenetrationVector(RectangleF rect1, RectangleF rect2)
        {
            var intersectingRectangle = RectangleF.Intersection(rect1, rect2);
            if (intersectingRectangle.IsEmpty) return Vector2.Zero;

            Vector2 penetration;
            if (intersectingRectangle.Width < intersectingRectangle.Height) {
                var d = rect1.Center.X < rect2.Center.X
                    ? intersectingRectangle.Width
                    : -intersectingRectangle.Width;
                penetration = new Vector2(d, 0);
            } else {
                var d = rect1.Center.Y < rect2.Center.Y
                    ? intersectingRectangle.Height
                    : -intersectingRectangle.Height;
                penetration = new Vector2(0, d);
            }

            return penetration;
        }

        private static Vector2 PenetrationVector(CircleF circ1, CircleF circ2)
        {
            if (!circ1.Intersects(circ2)) {
                return Vector2.Zero;
            }

            var displacement = Point2.Displacement(circ1.Center, circ2.Center);

            Vector2 desiredDisplacement;
            if (displacement != Vector2.Zero) {
                desiredDisplacement = displacement.NormalizedCopy() * (circ1.Radius + circ2.Radius);
            } else {
                desiredDisplacement = -Vector2.UnitY * (circ1.Radius + circ2.Radius);
            }

            var penetration = displacement - desiredDisplacement;
            return penetration;
        }

        private static Vector2 PenetrationVector(CircleF circ, RectangleF rect)
        {
            var collisionPoint = rect.ClosestPointTo(circ.Center);
            var cToCollPoint = collisionPoint - circ.Center;

            if (rect.Contains(circ.Center) || cToCollPoint.Equals(Vector2.Zero)) {
                var displacement = Point2.Displacement(circ.Center, rect.Center);

                Vector2 desiredDisplacement;
                if (displacement != Vector2.Zero) {
                    // Calculate penetration as only in X or Y direction.
                    // Whichever is lower.
                    var dispx = new Vector2(displacement.X, 0);
                    var dispy = new Vector2(0, displacement.Y);
                    dispx.Normalize();
                    dispy.Normalize();

                    dispx *= circ.Radius + rect.Width / 2;
                    dispy *= circ.Radius + rect.Height / 2;

                    if (dispx.LengthSquared() < dispy.LengthSquared()) {
                        desiredDisplacement = dispx;
                        displacement.Y = 0;
                    } else {
                        desiredDisplacement = dispy;
                        displacement.X = 0;
                    }
                } else {
                    desiredDisplacement = -Vector2.UnitY * (circ.Radius + rect.Height / 2);
                }

                var penetration = displacement - desiredDisplacement;
                return penetration;
            } else {
                var penetration = circ.Radius * cToCollPoint.NormalizedCopy() - cToCollPoint;
                return penetration;
            }
        }

        private static Vector2 PenetrationVector(RectangleF rect, CircleF circ)
        {
            return -PenetrationVector(circ, rect);
        }
    }
}
