using System;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public static class CollisionHelper
    {
        public static Vector2 CalculatePenetrationVector(CircleF circle, CircleF otherCircle, Vector2 velocity, Vector2 otherVelocity, Vector2 oldDistance)
        {
            var sumRadius = circle.Radius + otherCircle.Radius;
            // Find quadratic formula coefficients:
            // u^2 coefficient
            var a = velocity.Dot(velocity);
            // u coefficient
            var b = 2 * velocity.Dot(oldDistance);
            // constant
            var c = oldDistance.Dot(oldDistance) - sumRadius * sumRadius;

            if (oldDistance.Dot(oldDistance) <= sumRadius * sumRadius) {
                // Already overlapping
                return circle.CalculatePenetrationVector(otherCircle);
            }

            if (SolveQuadraticFormula(a, b, c, out var u0, out var u1)) {
                var maxPenetrationAt = (u0 + u1) / 2;
                circle.Position += velocity * maxPenetrationAt;
                otherCircle.Position += otherVelocity * maxPenetrationAt;
                return circle.CalculatePenetrationVector(otherCircle);
            }
            return Vector2.Zero;
        }

        private static bool SolveQuadraticFormula(in float a, in float b, in float c, out float u0, out float u1)
        {
            var squareRootPart = b * b - 4 * a * c;
            if (squareRootPart > 0) {
                // Two real solutions
                u0 = (-b + (float)Math.Sqrt(squareRootPart)) / (2 * a);
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
