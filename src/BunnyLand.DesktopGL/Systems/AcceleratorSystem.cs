using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class AcceleratorSystem : EntityProcessingSystem
    {
        private readonly GameSettings gameSettings;

        private readonly Variables variables;
        private ComponentMapper<Accelerator> acceleratorMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<PlayerInput> playerInputMapper;
        private ComponentMapper<PlayerState> playerMapper;

        public AcceleratorSystem(GameSettings gameSettings, Variables variables) : base(Aspect.All(typeof(PlayerState), typeof(PlayerInput),
            typeof(Movable)))
        {
            this.gameSettings = gameSettings;
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<PlayerState>();
            acceleratorMapper = mapperService.GetMapper<Accelerator>();
            movableMapper = mapperService.GetMapper<Movable>();
            playerInputMapper = mapperService.GetMapper<PlayerInput>();
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            // Accelerator holds intended acceleration
            // TODO: Use acceleration in some way to calculate new acceleration
            var player = playerMapper.Get(entityId);
            var movable = movableMapper.Get(entityId);
            var input = playerInputMapper.Get(entityId);
            // TODO: How to handle keyboard keys (WASD) vs Controller inputs? How to know which to use?
            var intendedAcceleration = input.DirectionalInputs.AccelerationDirection;

            movable.Acceleration = player.StandingOn switch {
                StandingOn.Nothing => ResultAcceleration(intendedAcceleration, movable.Velocity, player.IsBoosting),
                StandingOn.Planet => Vector2.Zero,
                _ => Vector2.Zero
            };

            movable.BrakingForce = player.IsBraking
                && (movable.Acceleration == Vector2.Zero || gameSettings.BrakeWhileJetpacking)
                    ? variables.Global[GlobalVariable.BrakePower]
                    : 0;
        }

        private Vector2 ResultAcceleration(Vector2 intendedAcceleration,
            Vector2 currentVelocity, bool isBoosting)
        {
            var jetpackMaxSpeed = variables.Global[GlobalVariable.JetpackMaxSpeed];
            var jetpackBoostMaxSpeed = variables.Global[GlobalVariable.JetpackBoostMaxSpeed];

            var jetpackAcceleration = variables.Global[GlobalVariable.JetpackAcceleration];
            var jetpackBoostAcceleration = variables.Global[GlobalVariable.JetpackBoostAcceleration];

            var accelerationMultiplier = isBoosting ? jetpackBoostAcceleration : jetpackAcceleration;

            var attemptedNewVelocity = currentVelocity + intendedAcceleration * accelerationMultiplier;

            var actualNewVelocity = attemptedNewVelocity.Truncate(Math.Max(currentVelocity.Length(),
                isBoosting ? jetpackBoostMaxSpeed : jetpackMaxSpeed));

            var actualAcceleration = actualNewVelocity - currentVelocity;

            return actualAcceleration;
        }
    }
}
