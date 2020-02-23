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
using tainicom.Aether.Physics2D.Dynamics;

namespace BunnyLand.DesktopGL.Systems
{
    public class InputSystem : EntityProcessingSystem
    {
        private readonly GameSettings gameSettings;
        private readonly Variables variables;
        private readonly KeyboardListener keyboardListener;
        private readonly IKeyMap keyMap;
        private readonly IButtonMap buttonMap;
        private readonly IDictionary<PlayerIndex, Player> players = new Dictionary<PlayerIndex, Player>();

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        private ComponentMapper<Movable> movableMapper;

        private ComponentMapper<Player> playerMapper;
        private readonly IDictionary<PlayerIndex, GamePadListener> gamePadListeners;
        private ComponentMapper<Body> bodyMapper;

        public InputSystem(KeyboardListener keyboardListener, IEnumerable<GamePadListener> gamePadListeners, IKeyMap keyMap, IButtonMap buttonMap,
            GameSettings gameSettings, Variables variables) : base(
            Aspect.All(typeof(Player)))
        {
            this.keyboardListener = keyboardListener;
            this.gamePadListeners = gamePadListeners.ToDictionary(l => l.PlayerIndex);
            this.keyMap = keyMap;
            this.buttonMap = buttonMap;
            this.gameSettings = gameSettings;
            this.variables = variables;

            keyboardListener.KeyPressed += OnKeyPressed;
            keyboardListener.KeyReleased += OnKeyReleased;
            foreach (var gamePadListener in this.gamePadListeners.Values) {
                gamePadListener.ButtonDown += OnButtonDown;
                gamePadListener.ButtonUp += OnButtonUp;
                gamePadListener.ThumbStickMoved += OnThumbStickMoved;
                gamePadListener.TriggerMoved += OnTriggerMoved;
            }
        }

        private void OnTriggerMoved(object? sender, GamePadEventArgs e)
        {
        }

        private void OnThumbStickMoved(object? sender, GamePadEventArgs e)
        {

        }

        private void OnButtonUp(object? sender, GamePadEventArgs e)
        {
        }

        private void OnButtonDown(object? sender, GamePadEventArgs e)
        {
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyPressed(e.PlayerIndex, key));
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => players[player.PlayerIndex] = player);
        }

        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => HandlePlayerKeyPressed(t.index, t.key));
        }

        private void HandlePlayerKeyPressed(PlayerIndex index, PlayerKey key)
        {
            pressedKeys[index].Add(key);
            switch (key) {
                case PlayerKey.ToggleBrake:
                    players.TryGetValue(index)
                        .IfSome(player => player.IsBraking = !player.IsBraking);
                    break;
            }
        }

        private void OnKeyReleased(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => pressedKeys[t.index].Remove(t.key));
        }

        public new void Dispose()
        {
            base.Dispose();
            keyboardListener.KeyPressed -= OnKeyPressed;
            keyboardListener.KeyReleased -= OnKeyReleased;
            foreach (var gamePadListener in gamePadListeners.Values)
            {
                gamePadListener.ButtonDown -= OnButtonDown;
                gamePadListener.ButtonUp -= OnButtonUp;
                gamePadListener.ThumbStickMoved -= OnThumbStickMoved;
                gamePadListener.TriggerMoved -= OnTriggerMoved;
            }
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<Player>();
            movableMapper = mapperService.GetMapper<Movable>();
            bodyMapper = mapperService.GetMapper<Body>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var player = playerMapper.Get(entityId);

            var keys = pressedKeys[player.PlayerIndex];

            movableMapper.TryGet(entityId).IfSome(movable => {

                movable.Acceleration = player.StandingOn switch {
                    StandingOn.Nothing => ResultAcceleration(keys, movable.Velocity),
                    StandingOn.Planet => Vector2.Zero,
                    _ => Vector2.Zero
                };

                movable.BrakingForce = player.IsBraking
                    && (movable.Acceleration == Vector2.Zero || gameSettings.BrakeWhileJetpacking)
                        ? variables.Global[GlobalVariable.BrakePower]
                        : 0;

            });



            bodyMapper.TryGet(entityId).IfSome(body => {



                var accelerationMultiplier = 100000f;

                var attemptedAcceleration = new Vector2(
                        (keys.Contains(PlayerKey.Left) ? -1 : 0) + (keys.Contains(PlayerKey.Right) ? 1 : 0),
                        (keys.Contains(PlayerKey.Up) ? -1 : 0) + (keys.Contains(PlayerKey.Down) ? 1 : 0))
                    .NormalizedOrZero() * accelerationMultiplier;


                body.ApplyForce(attemptedAcceleration);
                // var resultAcceleration = ResultAcceleration(keys, body.LinearVelocity);
                // if (resultAcceleration != Vector2.Zero) {
                    // body.ApplyForce(resultAcceleration * 100000);
                    // body.ApplyLinearImpulse(new Vector2(10, 0));
                // }
            });


        }

        private Vector2 ResultAcceleration(ICollection<PlayerKey> keys,
            Vector2 currentVelocity)
        {
            var jetpackMaxSpeed = variables.Global[GlobalVariable.JetpackMaxSpeed];
            var jetpackBoostMaxSpeed = variables.Global[GlobalVariable.JetpackBoostMaxSpeed];

            var jetpackAcceleration = variables.Global[GlobalVariable.JetpackAcceleration];
            var jetpackBoostAcceleration = variables.Global[GlobalVariable.JetpackBoostAcceleration];

            var isBoosting = keys.Contains(PlayerKey.Jump);
            var accelerationMultiplier = isBoosting ? jetpackBoostAcceleration : jetpackAcceleration;

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
