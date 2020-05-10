using System;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Utils
{
    public static class CollisionHelper
    {
        public static Vector2 CalculatePenetrationVector(CircleF circle, RectangleF otherRect, Vector2 velocity)
        {
            return Vector2.Zero;
        }

        public static Vector2 CalculatePenetrationVector(Point2 point, Segment2 segment, Vector2 velocity)
        {
            var oldPosition = point - velocity;
            float t;
            if (segment.Start.X.Equals(segment.End.X)) {
                // Horizontal line

                // pos(t) = oldPos + t * velocity, t {0,1}
                // x(t) = oldX + t * vel.X
                // oldX + t * vel.X = segment.X
                // t = (segment.X - oldX) / vel.X
                t = (segment.Start.X - oldPosition.X) / velocity.X;
            } else if (segment.Start.Y.Equals(segment.End.Y)) {
                // Vertical line

                t = (segment.Start.Y - oldPosition.Y) / velocity.Y;
            } else {
                throw new NotImplementedException("Only vertical and horizontal line segments are supported");
            }

            if (t < 0 || t > 1) return Vector2.Zero;

            var pointOfCollision = oldPosition + t * velocity;
            var penetrationVector = point - pointOfCollision;
            return penetrationVector;
        }

        public static Vector2 CalculatePenetrationVector(Point2 point, CircleF otherCircle, Vector2 velocity,
            Vector2 otherVelocity, Vector2 oldDistance)
        {
            var a = velocity.Dot(velocity);
            var b = 2 * velocity.Dot(oldDistance);
            var c = oldDistance.Dot(oldDistance);
            if (c < 0) {
                // already overlapping
                return new CircleF(point, 0).CalculatePenetrationVector(otherCircle);
            }

            if (SolveQuadraticFormula(a, b, c, out var u0, out var u1)) {
                var maxPenetrationAt = (u0 + u1) / 2;
                point += velocity * maxPenetrationAt;
                otherCircle.Position += otherVelocity * maxPenetrationAt;
                return new CircleF(point, 0).CalculatePenetrationVector(otherCircle);
            }

            return Vector2.Zero;
        }

        public static Vector2 CalculatePenetrationVector(CircleF circle, CircleF otherCircle, Vector2 velocity,
            Vector2 otherVelocity)
        {
            // make other circle static, expand its radius and do a ray cast
            var relativeVelocity = velocity - otherVelocity;
            // Reset the circles to their old positions
            circle.Center -= velocity;
            otherCircle.Center -= otherVelocity;
            var oldDistance = circle.Center - otherCircle.Center;

            var sumRadius = circle.Radius + otherCircle.Radius;
            // Find quadratic formula coefficients:
            // u^2 coefficient
            var a = relativeVelocity.Dot(relativeVelocity);
            // u coefficient
            var b = 2 * relativeVelocity.Dot(oldDistance);
            // constant
            var c = oldDistance.Dot(oldDistance) - sumRadius * sumRadius;

            if (oldDistance.Dot(oldDistance) <= sumRadius * sumRadius) {
                // Already overlapping
                return circle.CalculatePenetrationVector(otherCircle);
            }

            if (SolveQuadraticFormula(a, b, c, out var u0, out var u1) && u0.IsBetween(0, 1) && u1.IsBetween(0,1)) {
                var maxPenetrationAt = (u0 + u1) / 2;
                circle.Position += velocity * maxPenetrationAt;
                otherCircle.Position += otherVelocity * maxPenetrationAt;
                return circle.CalculatePenetrationVector(otherCircle);
            }

            return Vector2.Zero;
        }

        private static bool SolveQuadraticFormula(in float a, in float b, in float c, out float u0, out float u1)
        {
            if (b * b <= 4 * a * c) {
                u0 = u1 = 0;
                return false;
            }
            var squareRootPart = b * b - 4 * a * c;
            if (squareRootPart > 0) {
                // Two real solutions
                u0 = (-b + (float) Math.Sqrt(squareRootPart)) / (2 * a);
                u1 = (-b - (float) Math.Sqrt(squareRootPart)) / (2 * a);
                return true;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (squareRootPart == 0) {
                u0 = u1 = (-b + (float) Math.Sqrt(squareRootPart)) / (2 * a);
                return true;
            }

            u0 = u1 = 0;

            return false;
        }
    }
}
