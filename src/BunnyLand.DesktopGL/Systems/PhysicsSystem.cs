using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class PhysicsSystem : EntityProcessingSystem
    {
        private readonly Variables variables;
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Level> levelMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Transform2> transformMapper;

        private Option<Level> Level { get; set; }

        public PhysicsSystem(Variables variables) : base(Aspect.All(typeof(Transform2)).One(typeof(Movable)))
        {
            this.variables = variables;
        }

        protected override void OnEntityAdded(int entityId)
        {
            levelMapper.TryGet(entityId).IfSome(level => { Level = level; });
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            movableMapper = mapperService.GetMapper<Movable>();
            bodyMapper = mapperService.GetMapper<CollisionBody>();
            levelMapper = mapperService.GetMapper<Level>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);
            var movable = movableMapper.Get(entityId);
            var elapsedTicks = gameTime.GetElapsedTicks(variables);

            // Calculate change in velocity
            var deltaVelocity = (movable.Acceleration + movable.GravityPull) * elapsedTicks;
            movable.Velocity += deltaVelocity;

            // Apply braking if any
            movable.Velocity =
                movable.Velocity.SubtractLength(
                    Math.Min(movable.Velocity.Length(), movable.BrakingForce * elapsedTicks));

            // Limit to max speed
            movable.Velocity = movable.Velocity.Truncate(variables.Global[GlobalVariable.GlobalMaxSpeed])
                * variables.Global[GlobalVariable.InertiaRatio];

            // Update position
            bodyMapper.TryGet(entityId).IfSome(body => body.OldPosition = transform.Position);
            transform.Position += movable.Velocity * elapsedTicks;

            if (movable.WrapAround) {
                Level.IfSome(level => transform.Wrap(level.Bounds));
            }
        }
    }
}
