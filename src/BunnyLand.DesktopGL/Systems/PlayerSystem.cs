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
        private ComponentMapper<Player> playerMapper = null!;

        public PlayerSystem(Variables variables) : base(
            Aspect.All(typeof(Player)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<Player>();
            emitterMapper = mapperService.GetMapper<Emitter>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => {
                emitterMapper.TryGet(entityId).IfSome(emitter => {
                    var isShooting = player.PlayerKeys[PlayerKey.Fire].HasFlag(KeyState.Pressed);
                    emitter.IsEmitting = isShooting;
                    emitter.EmitInterval = TimeSpan.FromSeconds(variables.Global[GlobalVariable.FiringInterval]);
                });
            });
        }
    }
}
