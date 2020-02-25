using BunnyLand.DesktopGL.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class EmitterSystem : EntityProcessingSystem
    {
        private ComponentMapper<Emitter> emitterMapper;

        public EmitterSystem() : base(Aspect.All(typeof(Emitter)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            emitterMapper = mapperService.GetMapper<Emitter>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var emitter = emitterMapper.Get(entityId);

            if (emitter.IsEmitting && emitter.LastEmitted
                .Some(lastEmitted => lastEmitted + emitter.EmitInterval < gameTime.TotalGameTime).None(true)) {
                emitter.Emit?.Invoke(CreateEntity(), gameTime.TotalGameTime);
                emitter.LastEmitted = gameTime.TotalGameTime;
            }
        }
    }
}
