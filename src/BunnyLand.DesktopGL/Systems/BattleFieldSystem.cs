using BunnyLand.DesktopGL.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class CollisionSystem : EntityProcessingSystem
    {
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Transform2> transformMapper;

        public CollisionSystem() : base(Aspect.All(typeof(CollisionBody), typeof(Transform2)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            bodyMapper = mapperService.GetMapper<CollisionBody>();
            transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BattleFieldSystem : EntityProcessingSystem
    {
        private ComponentMapper<Transform2> transformMapper = null!;

        public BattleFieldSystem() : base(Aspect.All(typeof(Transform2)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
        }

        protected override void OnEntityAdded(int entityId)
        {
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);


        }
    }

    public class PhysicsSystem : EntityProcessingSystem
    {
        public PhysicsSystem() : base(Aspect.All(typeof(Transform2), typeof(PhysicalObject)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            throw new System.NotImplementedException();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            throw new System.NotImplementedException();
        }
    }
}
