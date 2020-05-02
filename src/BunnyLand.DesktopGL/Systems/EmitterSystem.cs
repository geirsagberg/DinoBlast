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
    public class EmitterSystem : EntityProcessingSystem
    {
        private readonly EntityFactory entityFactory;
        private readonly Variables variables;
        private ComponentMapper<Emitter> emitterMapper;
        private ComponentMapper<Transform2> transformMapper;

        public EmitterSystem(Variables variables, EntityFactory entityFactory) : base(Aspect.All(typeof(Emitter), typeof(Transform2)))
        {
            this.variables = variables;
            this.entityFactory = entityFactory;
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
                        var direction = entity.TryGet<Player>()
                            .Some(player => player.DirectionalInputs.AimDirection.NormalizedOrZero())
                            .None(Vector2.Zero);
                        if (direction == Vector2.Zero) direction = Vector2.UnitX;
                        var velocity = entity.TryGet<Movable>()
                                .Some(movable => movable.Velocity)
                                .None(Vector2.Zero)
                            + direction * variables.Global[GlobalVariable.BulletSpeed];
                        entityFactory.CreateBullet(CreateEntity(), transform.Position + direction * 20, velocity,
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
