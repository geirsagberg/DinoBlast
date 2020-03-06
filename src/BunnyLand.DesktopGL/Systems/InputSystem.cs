using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        private readonly MouseListener mouseListener;
        private readonly KeyboardListener keyboardListener;
        private readonly IKeyMap keyMap;
        private readonly IDictionary<PlayerIndex, Player> players = new Dictionary<PlayerIndex, Player>();

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        private ComponentMapper<Player> playerMapper;
        // private ComponentMapper<Accelerator> acceleratorMapper;

        public InputSystem(MouseListener mouseListener, KeyboardListener keyboardListener, IEnumerable<GamePadListener> gamePadListeners,
            IKeyMap keyMap, IButtonMap buttonMap) : base(
            Aspect.All(typeof(Player)))
        {
            this.mouseListener = mouseListener;
            this.keyboardListener = keyboardListener;
            this.gamePadListeners = gamePadListeners.ToDictionary(l => l.PlayerIndex);
            this.keyMap = keyMap;
            this.buttonMap = buttonMap;

            mouseListener.MouseMoved += OnMouseMoved;
            keyboardListener.KeyPressed += OnKeyPressed;
            keyboardListener.KeyReleased += OnKeyReleased;
            foreach (var gamePadListener in this.gamePadListeners.Values) {
                gamePadListener.ButtonDown += OnButtonDown;
                gamePadListener.ButtonUp += OnButtonUp;
                gamePadListener.ThumbStickMoved += OnThumbStickMoved;
                gamePadListener.TriggerMoved += OnTriggerMoved;
            }
        }

        private void OnMouseMoved(object? sender, MouseEventArgs e)
        {
            Console.WriteLine(e.Position);
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

        private void OnButtonDown(object? sender, GamePadEventArgs e)
        {
            // Console.WriteLine($"ButtonDown: ${e.Button} - ${e.TriggerState}");
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyInput(e.PlayerIndex, key));
        }

        private void OnButtonUp(object? sender, GamePadEventArgs e)
        {
            // Console.WriteLine($"ButtonUp: ${e.Button} - ${e.TriggerState}");
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyInput(e.PlayerIndex, key, true));
        }

        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => HandlePlayerKeyInput(t.index, t.key));
        }

        private void OnKeyReleased(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => HandlePlayerKeyInput(t.index, t.key, true));
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

        private void HandlePlayerKeyInput(PlayerIndex index, PlayerKey key, bool released = false)
        {
            if (released) {
                pressedKeys[index].Remove(key);
            } else {
                pressedKeys[index].Add(key);
            }

            if (key.IsDirectionInput()) {
                var direction = KeysToDirectionalInput(pressedKeys[index]);
                HandleAccelerationInput(index, direction);
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var player = playerMapper.Get(entityId);
            // var accelerator = acceleratorMapper.Get(entityId);

            var playerPressedKeys = pressedKeys[player.PlayerIndex];

            player.PlayerKeys = UpdatePlayerKeys(player.PlayerKeys, playerPressedKeys);

            if (player.PlayerKeys[PlayerKey.ToggleBrake].HasFlag(KeyState.JustPressed)) {
                player.IsBraking = !player.IsBraking;
            }
        }

        private static Dictionary<PlayerKey, KeyState> UpdatePlayerKeys(
            Dictionary<PlayerKey, KeyState> currentPlayerKeys,
            System.Collections.Generic.HashSet<PlayerKey> pressedKeys)
        {
            return currentPlayerKeys.ToDictionary(kvp => kvp.Key,
                kvp =>
                    (pressedKeys.Contains(kvp.Key)
                        ? KeyState.Pressed
                        : KeyState.None)
                    | (pressedKeys.Contains(kvp.Key) != kvp.Value.HasFlag(KeyState.Pressed)
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
            return attemptedAcceleration;
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
            // acceleratorMapper = mapperService.GetMapper<Accelerator>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => players[player.PlayerIndex] = player);
        }
    }
}
