using System;
using System.Collections.Generic;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Utils;
using MessagePack;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Rectangle = BunnyLand.DesktopGL.Models.Rectangle;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class CollisionBody
    {
        [IgnoreMember]
        public IShapeF Bounds => Shape switch {
            Circle circle => new CircleF(Position, circle.Radius),
            Rectangle rectangle => new RectangleF(Position, new Size2(rectangle.Width, rectangle.Height)),
            _ => throw new ArgumentOutOfRangeException()
        };

        [Key(0)] public SomeShape Shape { get; set; } = new Circle(0);

        [Key(1)] public ColliderTypes ColliderType { get; set; }

        [Key(2)] public ColliderTypes CollidesWith { get; set; }

        [IgnoreMember] public List<(int entityId, Vector2 penetrationVector)> Collisions { get; set; } = new List<(int entityId, Vector2 penetrationVector)>();

        [Key(3)] public Vector2 Position { get; set; }

        [IgnoreMember] public Vector2 OldPosition { get; set; }

        [IgnoreMember] public RectangleF CollisionBounds { get; set; }

        public CollisionBody()
        {
        }

        public CollisionBody(SomeShape shape, Vector2 position, ColliderTypes colliderType, ColliderTypes collidesWith)
        {
            OldPosition = Position = position;
            Shape = shape;
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
                CircleF circle when otherBounds is CircleF otherCircle => CollisionHelper.CalculatePenetrationVector(circle, otherCircle, velocity,
                    otherVelocity),
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
