using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input.InputListeners;
using KeyState = BunnyLand.DesktopGL.Enums.KeyState;

namespace BunnyLand.DesktopGL.Systems
{
    public class InputSystem : EntityProcessingSystem
    {
        private readonly IButtonMap buttonMap;
        private readonly IDictionary<PlayerIndex, GamePadListener> gamePadListeners;
        private readonly GameSettings gameSettings;
        private readonly KeyboardListener keyboardListener;
        private readonly IKeyMap keyMap;
        private readonly IDictionary<PlayerIndex, Player> players = new Dictionary<PlayerIndex, Player>();

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        // TODO: Can it just be a tuple of vector vector?
        private readonly Dictionary<PlayerIndex, DirectionalInputs> directionalInputs =
            Enum
                .GetValues(typeof(PlayerIndex))
                .Cast<PlayerIndex>().ToDictionary(i => i,
                    _ => new DirectionalInputs());

        private readonly Variables variables;

        private ComponentMapper<Movable> movableMapper;

        private ComponentMapper<Player> playerMapper;
        // private ComponentMapper<Accelerator> acceleratorMapper;

        public InputSystem(KeyboardListener keyboardListener, IEnumerable<GamePadListener> gamePadListeners,
            IKeyMap keyMap, IButtonMap buttonMap,
            GameSettings gameSettings, Variables variables) : base(
            Aspect.All(typeof(Player), typeof(Movable)))
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
            // Console.WriteLine($"TriggerMoved: ${e.Button} - ${e.TriggerState}");
        }

        private void OnThumbStickMoved(object? sender, GamePadEventArgs e)
        {
            var button = e.Button;
            var stickState = e.ThumbStickState;
            var playerIndex = e.PlayerIndex;
            var direction = new Vector2(stickState.X, -stickState.Y);

            if (button.HasFlag(Buttons.LeftStick)) {
                HandleAccelerationInput(playerIndex, direction);
            } else {
                HandleAimInput(playerIndex, direction);
            }
        }

        private void OnButtonUp(object? sender, GamePadEventArgs e)
        {
            // Console.WriteLine($"ButtonUp: ${e.Button} - ${e.TriggerState}");
            buttonMap.GetKey(e.Button).IfSome(key => pressedKeys[e.PlayerIndex].Remove(key));
        }

        private void OnButtonDown(object? sender, GamePadEventArgs e)
        {
            // Console.WriteLine($"ButtonDown: ${e.Button} - ${e.TriggerState}");
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

        private void HandleAccelerationInput(PlayerIndex index, Vector2 acceleration)
        {
            players.TryGetValue(index)
                .IfSome(player => player.DirectionalInputs.AccelerationDirection = acceleration);
        }

        private void HandleAimInput(PlayerIndex index, Vector2 aimVector)
        {
            players.TryGetValue(index)
                .IfSome(player => player.DirectionalInputs.AimDirection = aimVector);

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

            if (key.IsDirectionInput()) {
                var direction = KeysToDirectionalInput(pressedKeys[index]);
                HandleAccelerationInput(index, direction);
            }
        }

        private void OnKeyReleased(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => {
                    pressedKeys[t.index].Remove(t.key);

                    if (t.key.IsDirectionInput()) {
                        var direction = KeysToDirectionalInput(pressedKeys[t.index]);
                        HandleAccelerationInput(t.index, direction);
                    }
                });
        }

        public new void Dispose()
        {
            base.Dispose();
            keyboardListener.KeyPressed -= OnKeyPressed;
            keyboardListener.KeyReleased -= OnKeyReleased;
            foreach (var gamePadListener in gamePadListeners.Values) {
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
            // acceleratorMapper = mapperService.GetMapper<Accelerator>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var player = playerMapper.Get(entityId);
            var movable = movableMapper.Get(entityId);
            // var accelerator = acceleratorMapper.Get(entityId);

            var keys = pressedKeys[player.PlayerIndex];

            UpdatePlayerKeys(player, keys);

            // Temp hack to also allow arrow keys
            /*
            if (directionalInputs[player.PlayerIndex].AccelerationDirection.Length() < 0.1) {
                player.DirectionalInputs.AccelerationDirection = KeysToDirectionalInput(keys);
            }
            */
            // accelerator.IntendedAcceleration = directionalInputs[player.PlayerIndex].AccelerationDirection;

            /*
            movable.Acceleration = player.StandingOn switch {
                StandingOn.Nothing => ResultAcceleration(keys, movable.Velocity),
                StandingOn.Planet => Vector2.Zero,
                _ => Vector2.Zero
            };

            movable.BrakingForce = player.IsBraking
                                   && (movable.Acceleration == Vector2.Zero || gameSettings.BrakeWhileJetpacking)
                ? variables.Global[GlobalVariable.BrakePower]
                : 0;
                */
        }

        private static void UpdatePlayerKeys(Player player, System.Collections.Generic.HashSet<PlayerKey> keys)
        {
            player.PlayerKeys = player.PlayerKeys.ToDictionary(kvp => kvp.Key,
                kvp =>
                    (keys.Contains(kvp.Key)
                        ? KeyState.Pressed
                        : KeyState.None)
                    | (keys.Contains(kvp.Key) != kvp.Value.HasFlag(KeyState.Pressed)
                        ? KeyState.Changed
                        : KeyState.None));
        }

        private Vector2 KeysToDirectionalInput(ICollection<PlayerKey> keys)
        {
            var attemptedAcceleration = new Vector2(
                    (keys.Contains(PlayerKey.Left) ? -1 : 0)
                    + (keys.Contains(PlayerKey.Right) ? 1 : 0),
                    (keys.Contains(PlayerKey.Up) ? -1 : 0)
                    + (keys.Contains(PlayerKey.Down) ? 1 : 0))
                .NormalizedOrZero();
            Console.WriteLine($"Keyboard direction: ${attemptedAcceleration}");
            return attemptedAcceleration;
        }
    }
}
