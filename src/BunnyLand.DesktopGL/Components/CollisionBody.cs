using System;
using System.Collections.Generic;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Collisions;

namespace BunnyLand.DesktopGL.Components
{
    public class CollisionBody : ICollisionActor
    {
        public ColliderTypes ColliderType { get; }
        public ColliderTypes CollidesWith { get; }
        private readonly IShapeF bounds;
        private readonly Transform2 transform;

        public List<(CollisionBody body, Vector2 penetrationVector)> Collisions { get; set; } = new List<(CollisionBody body, Vector2 penetrationVector)>();

        public Vector2 OldPosition { get; set; }

        public IShapeF Bounds {
            get {
                bounds.Position = transform.Position;
                return bounds;
            }
        }
        
        public RectangleF CollisionBounds { get; set; }

        public CollisionBody(IShapeF bounds, Transform2 transform, ColliderTypes isColliderType, ColliderTypes collidesWith)
        {
            ColliderType = isColliderType;
            CollidesWith = collidesWith;
            this.bounds = bounds;
            this.transform = transform;
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            Console.WriteLine("Collision");
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
