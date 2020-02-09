using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
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
}
