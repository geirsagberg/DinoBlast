using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class PlayerSystem : EntityProcessingSystem, IPausable
    {
        private readonly Variables variables;
        private ComponentMapper<Emitter> emitterMapper = null!;
        private ComponentMapper<PlayerInput> inputMapper = null!;
        private ComponentMapper<PlayerState> playerMapper = null!;

        public PlayerSystem(Variables variables) : base(
            Aspect.All(typeof(PlayerInput)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            inputMapper = mapperService.GetMapper<PlayerInput>();
            emitterMapper = mapperService.GetMapper<Emitter>();
            playerMapper = mapperService.GetMapper<PlayerState>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            inputMapper.TryGet(entityId).IfSome(input => {
                emitterMapper.TryGet(entityId).IfSome(emitter => {
                    var isShooting = input.PlayerKeys[PlayerKey.Fire].Pressed;
                    emitter.IsEmitting = isShooting;
                    emitter.EmitInterval = TimeSpan.FromSeconds(variables.Global[GlobalVariable.FiringInterval]);
                });
                playerMapper.TryGet(entityId).IfSome(state => {
                    if (input.PlayerKeys[PlayerKey.ToggleBrake].JustPressed) {
                        state.IsBraking = !state.IsBraking;
                    }
                    state.IsBoosting = input.PlayerKeys[PlayerKey.Jump].Pressed;
                });
            });
        }
    }
}
