using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class EmitterSystem : EntityProcessingSystem
    {
        private readonly Variables variables;
        private ComponentMapper<Emitter> emitterMapper;

        public EmitterSystem(Variables variables) : base(Aspect.All(typeof(Emitter)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            emitterMapper = mapperService.GetMapper<Emitter>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var elapsedTimeSpan = gameTime.GetElapsedTimeSpan(variables);

            var emitter = emitterMapper.Get(entityId);

            emitter.TimeSinceLastEmit += elapsedTimeSpan;

            if (emitter.IsEmitting && emitter.TimeSinceLastEmit >= emitter.EmitInterval) {
                emitter.Emit?.Invoke(CreateEntity());
                emitter.TimeSinceLastEmit = TimeSpan.Zero;
            }
        }
    }
}
