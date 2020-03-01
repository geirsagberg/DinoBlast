using System;
using System.Collections.Generic;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class CollisionBody
    {
        public ColliderTypes ColliderType { get; }
        public ColliderTypes CollidesWith { get; }
        private readonly IShapeF shape;
        private readonly Transform2 transform;

        public List<(CollisionBody body, Vector2 penetrationVector)> Collisions { get; set; } = new List<(CollisionBody body, Vector2 penetrationVector)>();

        public Vector2 OldPosition { get; set; }

        public IShapeF Bounds {
            get {
                shape.Position = transform.Position;
                return shape;
            }
        }

        public RectangleF CollisionBounds { get; set; }

        public CollisionBody(IShapeF shape, Transform2 transform, ColliderTypes isColliderType, ColliderTypes collidesWith)
        {
            ColliderType = isColliderType;
            CollidesWith = collidesWith;
            this.shape = shape;
            this.transform = transform;
            OldPosition = transform.Position;
        }

        public Vector2 CalculatePenetrationVector(CollisionBody other)
        {
            if (other.Bounds.Intersects(Bounds)) return other.Bounds.CalculatePenetrationVector(Bounds);

            // Swept AABB / Circle algorithm

            var velocity = transform.Position - OldPosition;
            var otherVelocity = other.transform.Position - other.OldPosition;
            var oldDistance = other.OldPosition - OldPosition;

            // Currently ignoring velocity of rectangles
            return Bounds switch {
                RectangleF rect when other.Bounds is RectangleF otherRect => Vector2.Zero,
                RectangleF rect when other.Bounds is CircleF otherCircle => Vector2.Zero,
                CircleF circle when other.Bounds is RectangleF otherRect => CollisionHelper.CalculatePenetrationVector(circle, otherRect, velocity),
                CircleF circle when other.Bounds is CircleF otherCircle => CollisionHelper.CalculatePenetrationVector(circle, otherCircle, velocity, otherVelocity),
                _ => throw new NotImplementedException()
            };
        }
    }

    /// Used to determine what to check collisions against
    [Flags]
    public enum ColliderTypes
    {
        None = 0,
        Player = 1,
        Static = 1 << 1,
        Projectile = 1 << 2
    }
}
