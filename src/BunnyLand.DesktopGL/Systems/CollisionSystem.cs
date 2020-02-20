using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
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
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Level> levelMapper;

        public Option<Level> Level { get; set; }
        public Option<CollisionComponent> CollisionComponent { get; set; }

        public CollisionSystem() : base(Aspect.All(typeof(CollisionBody)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            bodyMapper = mapperService.GetMapper<CollisionBody>();
            levelMapper = mapperService.GetMapper<Level>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            levelMapper.TryGet(entityId).IfSome(level => {
                Level = level;
                var component = new CollisionComponent(level.Bounds);
                foreach (var entity in ActiveEntities) {
                    component.Insert(bodyMapper.Get(entity));
                }

                CollisionComponent = component;
            });
            CollisionComponent.IfSome(collisionComponent =>
                bodyMapper.TryGet(entityId).IfSome(collisionComponent.Insert));
        }

        protected override void OnEntityRemoved(int entityId)
        {
            CollisionComponent.IfSome(collisionComponent =>
                bodyMapper.TryGet(entityId).IfSome(collisionComponent.Remove));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CollisionComponent.IfSome(component => component.Update(gameTime));
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var body = bodyMapper.Get(entityId);
            body.CollisionInfo = Option<CollisionEventArgs>.None;
        }
    }
}
