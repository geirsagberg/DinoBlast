using System;
using System.Collections.Generic;
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
        private ComponentMapper<Player> playerMapper;

        public AcceleratorSystem(GameSettings gameSettings, Variables variables) : base(Aspect.All(typeof(Player),
            typeof(Movable)))
        {
            this.gameSettings = gameSettings;
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<Player>();
            acceleratorMapper = mapperService.GetMapper<Accelerator>();
            movableMapper = mapperService.GetMapper<Movable>();
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
            // TODO: How to handle keyboard keys (WASD) vs Controller inputs? How to know which to use?
            var intendedAcceleration = player.DirectionalInputs.AccelerationDirection;

            if (intendedAcceleration.X > 0.5 || intendedAcceleration.Y > 0.5) {
                var kake = 5;
            }

            movable.Acceleration = player.StandingOn switch {
                StandingOn.Nothing => ResultAcceleration(player.PlayerKeys, intendedAcceleration, movable.Velocity),
                StandingOn.Planet => Vector2.Zero,
                _ => Vector2.Zero
            };

            movable.BrakingForce = player.IsBraking
                && (movable.Acceleration == Vector2.Zero || gameSettings.BrakeWhileJetpacking)
                    ? variables.Global[GlobalVariable.BrakePower]
                    : 0;
        }

        private Vector2 ResultAcceleration(
            Dictionary<PlayerKey, KeyState> keys,
            Vector2 intendedAcceleration,
            Vector2 currentVelocity)
        {
            var jetpackMaxSpeed = variables.Global[GlobalVariable.JetpackMaxSpeed];
            var jetpackBoostMaxSpeed = variables.Global[GlobalVariable.JetpackBoostMaxSpeed];

            var jetpackAcceleration = variables.Global[GlobalVariable.JetpackAcceleration];
            var jetpackBoostAcceleration = variables.Global[GlobalVariable.JetpackBoostAcceleration];

            var isBoosting = keys[PlayerKey.Jump] == KeyState.Pressed;
            var accelerationMultiplier = isBoosting ? jetpackBoostAcceleration : jetpackAcceleration;

            var attemptedNewVelocity = currentVelocity + intendedAcceleration * accelerationMultiplier;

            var actualNewVelocity = attemptedNewVelocity.Truncate(Math.Max(currentVelocity.Length(),
                isBoosting ? jetpackBoostMaxSpeed : jetpackMaxSpeed));

            var actualAcceleration = actualNewVelocity - currentVelocity;

            return actualAcceleration;
        }
    }
}
