using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class PhysicsSystem : EntityProcessingSystem
    {
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Transform2> transformMapper;

        public PhysicsSystem() : base(Aspect.All(typeof(Transform2), typeof(Movable)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            movableMapper = mapperService.GetMapper<Movable>();
            bodyMapper = mapperService.GetMapper<CollisionBody>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);
            var movable = movableMapper.Get(entityId);
            var body = bodyMapper.MaybeGet(entityId);

            movable.Velocity += movable.Acceleration;
            movable.Velocity = movable.Velocity.Truncate(10).Scale(0.9f);
            transform.Position += movable.Velocity -
                body.Match(some => some.CollisionInfo.Match(info => info.PenetrationVector, Vector2.Zero),
                    Vector2.Zero);
        }
    }
}
