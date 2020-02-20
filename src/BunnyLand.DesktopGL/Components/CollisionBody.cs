using System;
using LanguageExt;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace BunnyLand.DesktopGL.Components
{
    public class CollisionBody : ICollisionActor
    {
        public ColliderTypes ColliderType { get; }
        public ColliderTypes CollidesWith { get; }
        private readonly IShapeF bounds;
        private readonly Transform2 transform;
        public Option<CollisionEventArgs> CollisionInfo { get; set; }

        public IShapeF Bounds {
            get {
                bounds.Position = transform.Position;
                return bounds;
            }
        }

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
            CollisionInfo = collisionInfo;
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
