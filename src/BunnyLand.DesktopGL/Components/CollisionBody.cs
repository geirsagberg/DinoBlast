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
        public IShapeF Shape { get; }
        public ColliderTypes ColliderType { get; }
        public ColliderTypes CollidesWith { get; }
        // private readonly IShapeF shape;
        // private readonly Transform2 transform;

        public List<(int entityId, Vector2 penetrationVector)> Collisions { get; set; } = new List<(int entityId, Vector2 penetrationVector)>();

        public Vector2 OldPosition { get; set; }

        // public IShapeF Bounds {
        //     get {
        //         shape.Position = transform.Position;
        //         return shape;
        //     }
        // }

        public RectangleF CollisionBounds { get; set; }

        public CollisionBody(IShapeF shape, Transform2 transform, ColliderTypes colliderType, ColliderTypes collidesWith)
        {
            Shape = shape;
            ColliderType = colliderType;
            CollidesWith = collidesWith;
            // this.shape = shape;
            // this.transform = transform;
            // OldPosition = transform.Position;
        }

        public Vector2 CalculatePenetrationVector(CollisionBody otherBody, Transform2 transform, Transform2 otherTransform)
        {
            var bounds = GetBounds(this, transform.Position);
            var otherBounds = GetBounds(otherBody, otherTransform.Position);
            if (otherBounds.Intersects(bounds)) return otherBounds.CalculatePenetrationVector(bounds);

            // Swept AABB / Circle algorithm

            var velocity = transform.Position - OldPosition;
            var otherVelocity = otherTransform.Position - otherBody.OldPosition;
            var oldDistance = otherBody.OldPosition - OldPosition;

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
            collisionBody.Shape.Position = position;
            return collisionBody.Shape;
        }

        public IShapeF GetBounds(Vector2 position)
        {
            Shape.Position = position;
            return Shape;
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
