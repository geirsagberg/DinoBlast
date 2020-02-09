using LanguageExt;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace BunnyLand.DesktopGL.Components
{
    public class CollisionBody : ICollisionActor
    {
        private readonly IShapeF bounds;
        private readonly Transform2 transform;
        public Option<CollisionEventArgs> CollisionInfo { get; set; }

        public IShapeF Bounds {
            get {
                bounds.Position = transform.Position;
                return bounds;
            }
        }

        public CollisionBody(IShapeF bounds, Transform2 transform)
        {
            this.bounds = bounds;
            this.transform = transform;
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            CollisionInfo = collisionInfo;
        }
    }
}
