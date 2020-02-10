using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input.InputListeners;

namespace BunnyLand.DesktopGL.Systems
{
    public class InputSystem : EntityProcessingSystem
    {
        private readonly GamePadListener gamePadListener;
        private readonly KeyboardListener keyboardListener;
        private readonly IKeyMap keyMap;
        private readonly GameSettings gameSettings;
        private readonly IDictionary<PlayerIndex, Player> players = new Dictionary<PlayerIndex, Player>();

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        private ComponentMapper<Movable> movableMapper;

        private ComponentMapper<Player> playerMapper;

        public InputSystem(KeyboardListener keyboardListener, GamePadListener gamePadListener, IKeyMap keyMap, GameSettings gameSettings) : base(
            Aspect.All(typeof(Player), typeof(Movable)))
        {
            this.keyboardListener = keyboardListener;
            this.gamePadListener = gamePadListener;
            this.keyMap = keyMap;
            this.gameSettings = gameSettings;

            keyboardListener.KeyPressed += OnKeyPressed;
            keyboardListener.KeyReleased += OnKeyRelease;
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => players[player.PlayerIndex] = player);
        }

        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => {
                    pressedKeys[t.index].Add(t.key);
                    HandlePlayerKeyPressed(t.index, t.key);
                });
        }

        private void HandlePlayerKeyPressed(PlayerIndex index, PlayerKey key)
        {
            switch (key) {
                case PlayerKey.ToggleBrake:
                    players.TryGetValue(index)
                        .IfSome(player => player.IsBraking = !player.IsBraking);
                    break;
            }
        }

        private void OnKeyRelease(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => pressedKeys[t.index].Remove(t.key));
        }

        public new void Dispose()
        {
            base.Dispose();
            keyboardListener.KeyPressed -= OnKeyPressed;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<Player>();
            movableMapper = mapperService.GetMapper<Movable>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var player = playerMapper.Get(entityId);
            var movable = movableMapper.Get(entityId);

            var keys = pressedKeys[player.PlayerIndex];


            movable.Acceleration = player.StandingOn switch {
                StandingOn.Nothing => ResultAcceleration(keys, movable.Velocity),
                StandingOn.Planet => Vector2.Zero,
                _ => Vector2.Zero
            };


            movable.BrakingForce = player.IsBraking && (movable.Acceleration == Vector2.Zero || gameSettings.BrakeWhileJetpacking)
                ? player.BrakePower
                : 0;
        }

        private static Vector2 ResultAcceleration(ICollection<PlayerKey> keys,
            Vector2 currentVelocity)
        {
            const float jetpackMaxSpeed = 8;
            const float jetpackBoostMaxSpeed = 12;

            var isBoosting = keys.Contains(PlayerKey.Jump);
            var accelerationMultiplier = isBoosting ? 1f : 0.2f;


            var attemptedAcceleration = new Vector2(
                    (keys.Contains(PlayerKey.Left) ? -1 : 0) + (keys.Contains(PlayerKey.Right) ? 1 : 0),
                    (keys.Contains(PlayerKey.Up) ? -1 : 0) + (keys.Contains(PlayerKey.Down) ? 1 : 0))
                .NormalizedOrZero() * accelerationMultiplier;

            var attemptedNewVelocity = currentVelocity + attemptedAcceleration;

            var actualNewVelocity = attemptedNewVelocity.Truncate(Math.Max(currentVelocity.Length(),
                isBoosting ? jetpackBoostMaxSpeed : jetpackMaxSpeed));

            var actualAcceleration = actualNewVelocity - currentVelocity;

            return actualAcceleration;
        }
    }
}
