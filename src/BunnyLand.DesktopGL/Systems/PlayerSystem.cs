using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class PlayerSystem : EntityProcessingSystem
    {
        private readonly EntityFactory entityFactory;
        private readonly Random random;
        private readonly Variables variables;
        private ComponentMapper<Emitter> emitterMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Player> playerMapper;
        private ComponentMapper<Transform2> transformMapper;

        public PlayerSystem(EntityFactory entityFactory, Random random, Variables variables) : base(
            Aspect.All(typeof(Player)))
        {
            this.entityFactory = entityFactory;
            this.random = random;
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            playerMapper = mapperService.GetMapper<Player>();
            movableMapper = mapperService.GetMapper<Movable>();
            emitterMapper = mapperService.GetMapper<Emitter>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => {
                transformMapper.TryGet(entityId).IfSome(transform => {
                    movableMapper.TryGet(entityId).IfSome(movable => {
                        var isShooting = player.PlayerKeys[PlayerKey.Fire].HasFlag(KeyState.Pressed);

                        emitterMapper.TryGet(entityId).IfSome(emitter => {
                            emitter.IsEmitting = isShooting;
                            emitter.Emit ??= entity => {
                                var velocity = movable.Velocity + random.NextUnitVector();

                                entityFactory.CreateBullet(entity,
                                    transform.Position + velocity.NormalizedCopy() * 16,
                                    velocity, TimeSpan.FromSeconds(4));
                            };
                        });
                    });
                });
            });
        }
    }
}
