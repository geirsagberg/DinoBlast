using BunnyLand.DesktopGL.Components;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    /// <summary>
    ///     Handles adding <see cref="CollisionBody" /> components to the <see cref="CollisionComponent" />, and resetting
    ///     their collision info. Collision checking itself is handled within <see cref="CollisionComponent" />.
    /// </summary>
    public class CollisionSystem : EntityProcessingSystem
    {
        private readonly CollisionComponent collisionComponent;
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Transform2> transformMapper;

        public CollisionSystem(CollisionComponent collisionComponent) : base(Aspect.All(typeof(CollisionBody),
            typeof(Transform2)))
        {
            this.collisionComponent = collisionComponent;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            bodyMapper = mapperService.GetMapper<CollisionBody>();
            transformMapper = mapperService.GetMapper<Transform2>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            var body = bodyMapper.Get(entityId);
            if (body != null) {
                collisionComponent.Insert(body);
            }
        }

        protected override void OnEntityRemoved(int entityId)
        {
            var body = bodyMapper.Get(entityId);
            if (body != null) {
                collisionComponent.Remove(body);
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var body = bodyMapper.Get(entityId);
            body.CollisionInfo = Option<CollisionEventArgs>.None;
        }
    }
}
