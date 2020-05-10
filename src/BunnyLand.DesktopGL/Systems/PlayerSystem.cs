using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class PlayerSystem : EntityProcessingSystem
    {
        private readonly Variables variables;
        private ComponentMapper<Emitter> emitterMapper = null!;
        private ComponentMapper<PlayerInput> inputMapper = null!;

        public PlayerSystem(Variables variables) : base(
            Aspect.All(typeof(PlayerInput)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            inputMapper = mapperService.GetMapper<PlayerInput>();
            emitterMapper = mapperService.GetMapper<Emitter>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            inputMapper.TryGet(entityId).IfSome(player => {
                emitterMapper.TryGet(entityId).IfSome(emitter => {
                    var isShooting = player.PlayerKeys[PlayerKey.Fire].Pressed;
                    emitter.IsEmitting = isShooting;
                    emitter.EmitInterval = TimeSpan.FromSeconds(variables.Global[GlobalVariable.FiringInterval]);
                });
            });
        }
    }
}
