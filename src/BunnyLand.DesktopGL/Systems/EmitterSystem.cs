using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using PlayerState = BunnyLand.DesktopGL.Components.PlayerState;

namespace BunnyLand.DesktopGL.Systems
{
    public class EmitterSystem : EntityProcessingSystem, IPausable
    {
        private readonly EntityFactory entityFactory;
        private readonly SharedContext sharedContext;
        private readonly Variables variables;
        private ComponentMapper<Emitter> emitterMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;

        public EmitterSystem(Variables variables, EntityFactory entityFactory, SharedContext sharedContext) : base(Aspect.All(typeof(Emitter), typeof(Transform2)))
        {
            this.variables = variables;
            this.entityFactory = entityFactory;
            this.sharedContext = sharedContext;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            emitterMapper = mapperService.GetMapper<Emitter>();
            transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {




            var elapsedTimeSpan = gameTime.GetElapsedTimeSpan(variables);

            var emitter = emitterMapper.Get(entityId);
            var transform = transformMapper.Get(entityId);

            emitter.TimeSinceLastEmit += elapsedTimeSpan;

            if (emitter.IsEmitting && emitter.TimeSinceLastEmit >= emitter.EmitInterval) {
                var entity = GetEntity(entityId);

                switch (emitter.EmitterType) {
                    case EmitterType.Bullet: {
                        var direction = entity.TryGet<PlayerInput>()
                            .Some(player => player.DirectionalInputs.AimDirection.NormalizedOrZero())
                            .None(Vector2.Zero);
                        if (direction == Vector2.Zero) direction = Vector2.UnitX;
                        var velocity = entity.TryGet<Movable>()
                                .Some(movable => movable.Velocity)
                                .None(Vector2.Zero)
                            + direction * variables.Global[GlobalVariable.BulletSpeed];
                        EntityFactory.CreateBullet(CreateEntity(), transform.Position + direction * 20, velocity,
                            TimeSpan.FromSeconds(variables.Global[GlobalVariable.BulletLifespan]));
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                emitter.TimeSinceLastEmit = TimeSpan.Zero;
            }
        }
    }
}
