using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class PhysicsSystem : EntityProcessingSystem, IPausable
    {
        private readonly SharedContext sharedContext;
        private readonly Variables variables;
        private ComponentMapper<CollisionBody> bodyMapper = null!;
        private ComponentMapper<Level> levelMapper = null!;
        private ComponentMapper<Movable> movableMapper = null!;
        private ComponentMapper<PlayerState> playerStateMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;

        private Option<Level> Level { get; set; }

        public PhysicsSystem(Variables variables, SharedContext sharedContext) : base(Aspect.All(typeof(Transform2)).One(typeof(Movable)))
        {
            this.variables = variables;
            this.sharedContext = sharedContext;
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
            playerStateMapper = mapperService.GetMapper<PlayerState>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);
            var movable = movableMapper.Get(entityId);
            var elapsedTicks = gameTime.GetElapsedTicks(variables);
            var oldPosition = transform.Position;

            if (playerStateMapper.Get(entityId) is { StandingOn: StandingOn.Planet } playerState && playerState.StandingOnEntity != null) {
                var planetTransform = transformMapper.Get(playerState.StandingOnEntity.Value);
                var normal = transform.Position - planetTransform.Position;

                const float planetSpeed = 2f;

                var newNormal = normal.Rotate(planetSpeed * elapsedTicks * movable.Acceleration.X / normal.Length());

                transform.Position = planetTransform.Position + newNormal;

                const float planetJumpVelocity = 10f;

                if (playerState.IsBoosting) {
                    // Jump off planet
                    var playerInput = GetEntity(entityId).Get<PlayerInput>();

                    movable.Velocity = playerInput.DirectionalInputs.AimDirection.NormalizedCopy() * planetJumpVelocity;
                    transform.Position += movable.Velocity;
                    playerState.StandingOnEntity = null;
                    playerState.StandingOn = StandingOn.Nothing;
                }
            } else {
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
                transform.Position += movable.Velocity * elapsedTicks;
            }

            // Update collision body
            bodyMapper.TryGet(entityId).IfSome(body => {
                body.OldPosition = oldPosition;
                body.Position = transform.Position;
            });

            Level.IfSome(level => {
                switch (movable.LevelBoundsBehavior) {
                    case LevelBoundsBehavior.Wrap:
                        transform.Wrap(level.Bounds);
                        break;
                    case LevelBoundsBehavior.Destroy when transform.WorldPosition.X < 0
                        || transform.WorldPosition.X >= level.Bounds.Width
                        || transform.WorldPosition.Y < 0
                        || transform.WorldPosition.Y >= level.Bounds.Height:
                        DestroyEntity(entityId);
                        break;
                }
            });
        }
    }
}
