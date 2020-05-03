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
        private readonly IShapeF shape;

        public IShapeF Bounds {
            get {
                shape.Position = Position;
                return shape;
            }
        }

        public ColliderTypes ColliderType { get; }
        public ColliderTypes CollidesWith { get; }

        public List<(int entityId, Vector2 penetrationVector)> Collisions { get; set; } = new List<(int entityId, Vector2 penetrationVector)>();

        public Vector2 Position { get; set; }
        public Vector2 OldPosition { get; set; }

        public RectangleF CollisionBounds { get; set; }

        public CollisionBody(IShapeF shape, Vector2 position, ColliderTypes colliderType, ColliderTypes collidesWith)
        {
            this.shape = shape;
            OldPosition = Position = position;
            ColliderType = colliderType;
            CollidesWith = collidesWith;
        }

        public Vector2 CalculatePenetrationVector(CollisionBody otherBody)
        {
            var bounds = Bounds;
            var otherBounds = otherBody.Bounds;
            if (otherBounds.Intersects(bounds)) return otherBounds.CalculatePenetrationVector(bounds);

            // Swept AABB / Circle algorithm

            var velocity = Position - OldPosition;
            var otherVelocity = otherBody.Position - otherBody.OldPosition;

            // Currently ignoring velocity of rectangles
            return bounds switch {
                RectangleF rect when otherBounds is RectangleF otherRect => Vector2.Zero,
                RectangleF rect when otherBounds is CircleF otherCircle => Vector2.Zero,
                CircleF circle when otherBounds is RectangleF otherRect => CollisionHelper.CalculatePenetrationVector(circle, otherRect, velocity),
                CircleF circle when otherBounds is CircleF otherCircle => CollisionHelper.CalculatePenetrationVector(circle, otherCircle, velocity, otherVelocity),
                _ => throw new NotImplementedException()
            };
        }

        public static IShapeF GetBounds(CollisionBody collisionBody, Vector2 position)
        {
            collisionBody.Bounds.Position = position;
            return collisionBody.Bounds;
        }

        public IShapeF GetBounds(Vector2 position)
        {
            Bounds.Position = position;
            return Bounds;
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
