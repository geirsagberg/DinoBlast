using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class Projectile
    {
        public Movable Movable { get; }
        public CollisionBody CollisionBody { get; }

        public Projectile(Movable movable, CollisionBody collisionBody)
        {
            Movable = movable;
            CollisionBody = collisionBody;
        }
    }
}
