using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
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

        // TODO: Can it just be a tuple of vector vector?
        private readonly Dictionary<PlayerIndex, DirectionalInputs> directionalInputs =
            Enum
                .GetValues(typeof(PlayerIndex))
                .Cast<PlayerIndex>().ToDictionary(i => i,
                    _ => new DirectionalInputs());

        private readonly IDictionary<PlayerIndex, GamePadListener> gamePadListeners;
        private readonly KeyboardListener keyboardListener;
        private readonly MouseListener mouseListener;
        private readonly IDictionary<PlayerIndex, int> playerEntities = new Dictionary<PlayerIndex, int>();
        private readonly IDictionary<PlayerIndex, Player> players = new Dictionary<PlayerIndex, Player>();

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        private ComponentMapper<Player> playerMapper;

        private ComponentMapper<Transform2> transformMapper;
        // private ComponentMapper<Accelerator> acceleratorMapper;

        public InputSystem(MouseListener mouseListener, KeyboardListener keyboardListener,
            IEnumerable<GamePadListener> gamePadListeners, IButtonMap buttonMap
        ) : base(
            Aspect.All(typeof(Player)))
        {
            this.mouseListener = mouseListener;
            this.keyboardListener = keyboardListener;
            this.gamePadListeners = gamePadListeners.ToDictionary(l => l.PlayerIndex);
            this.buttonMap = buttonMap;

            mouseListener.MouseMoved += OnMouseMoved;
            mouseListener.MouseDown += OnMouseDown;
            mouseListener.MouseUp += OnMouseUp;
            keyboardListener.KeyPressed += OnKeyPressed;
            keyboardListener.KeyReleased += OnKeyReleased;
            foreach (var gamePadListener in this.gamePadListeners.Values) {
                gamePadListener.ButtonDown += OnButtonDown;
                gamePadListener.ButtonUp += OnButtonUp;
                gamePadListener.ThumbStickMoved += OnThumbStickMoved;
                gamePadListener.TriggerMoved += OnTriggerMoved;
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyInput(PlayerIndex.One, key, true));
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyInput(PlayerIndex.One, key));
        }

        private void OnMouseMoved(object? sender, MouseEventArgs e)
        {
            playerEntities.TryGetValue(PlayerIndex.One).IfSome(entityId => transformMapper.TryGet(entityId).IfSome(
                movable => { HandleAimInput(PlayerIndex.One, e.Position.ToVector2() - movable.Position); }));
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
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyInput(e.PlayerIndex, key));
        }

        private void OnButtonUp(object? sender, GamePadEventArgs e)
        {
            buttonMap.GetKey(e.Button).IfSome(key => HandlePlayerKeyInput(e.PlayerIndex, key, true));
        }

        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            buttonMap.GetKey(e.Key)
                .IfSome(key => HandlePlayerKeyInput(PlayerIndex.One, key));
        }

        private void OnKeyReleased(object? sender, KeyboardEventArgs e)
        {
            buttonMap.GetKey(e.Key)
                .IfSome(key => HandlePlayerKeyInput(PlayerIndex.One, key, true));
        }

        private void HandleAccelerationInput(PlayerIndex index, Vector2 acceleration)
        {
            players.TryGetValue(index)
                .IfSome(player => player.DirectionalInputs.AccelerationDirection = acceleration);
        }

        private void HandleAimInput(PlayerIndex index, Vector2 aimVector)
        {
            players.TryGetValue(index)
                .IfSome(player => player.DirectionalInputs.AimDirection = aimVector.NormalizedOrZero());
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
            transformMapper = mapperService.GetMapper<Transform2>();
            // acceleratorMapper = mapperService.GetMapper<Accelerator>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => {
                players[player.PlayerIndex] = player;
                playerEntities[player.PlayerIndex] = entityId;
            });
        }
    }
}
