using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
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
        private const float ThumbStickDeadZone = 0.1f;
        private readonly IButtonMap buttonMap;

        private readonly Dictionary<PlayerIndex, DirectionalInputs> directionalInputs = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new DirectionalInputs());

        private readonly IDictionary<PlayerIndex, GamePadListener> gamePadListeners;
        private readonly KeyboardListener keyboardListener;
        private readonly IDictionary<PlayerIndex, int> playerEntitiesByIndex = new Dictionary<PlayerIndex, int>();

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        private readonly SharedContext sharedContext;

        private ComponentMapper<PlayerInput> inputMapper = null!;
        private ComponentMapper<PlayerState> playerMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;

        public InputSystem(MouseListener mouseListener, KeyboardListener keyboardListener,
            IEnumerable<GamePadListener> gamePadListeners, IButtonMap buttonMap, SharedContext sharedContext
        ) : base(
            Aspect.All(typeof(PlayerState), typeof(PlayerInput)))
        {
            this.keyboardListener = keyboardListener;
            this.gamePadListeners = gamePadListeners.ToDictionary(l => l.PlayerIndex);
            this.buttonMap = buttonMap;
            this.sharedContext = sharedContext;

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
            playerEntitiesByIndex.TryGetValue(PlayerIndex.One)
                .IfSome(entityId => transformMapper.TryGet(entityId)
                    .IfSome(
                        movable => HandleAimInput(PlayerIndex.One, e.Position.ToVector2() - movable.Position)));
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
            directionalInputs[index] = new DirectionalInputs(acceleration, directionalInputs[index].AimDirection);
        }

        private void HandleAimInput(PlayerIndex index, Vector2 aimVector)
        {
            directionalInputs[index] = new DirectionalInputs(directionalInputs[index].AccelerationDirection, aimVector.NormalizedOrZero());
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
            var state = playerMapper.Get(entityId);
            var input = inputMapper.Get(entityId);
            input.CurrentFrame = sharedContext.FrameCounter;

            state.LocalPlayerIndex.IfSome(playerIndex => {
                var playerPressedKeys = pressedKeys[playerIndex];

                var currentFrame = sharedContext.FrameCounter + sharedContext.FrameOffset;

                if (input.PlayerKeysByFrame.Count >= PlayerInput.InitialFrameBuffer) {
                    input.PlayerKeysByFrame.Remove(currentFrame - PlayerInput.InitialFrameBuffer);
                }

                if (input.DirectionalInputsByFrame.Count >= PlayerInput.InitialFrameBuffer) {
                    input.DirectionalInputsByFrame.Remove(currentFrame - PlayerInput.InitialFrameBuffer);
                }

                if (!input.PlayerKeysByFrame.ContainsKey(currentFrame))
                    input.PlayerKeysByFrame[currentFrame] =
                        UpdatePlayerKeys(input.PlayerKeysByFrame.TryGetValue(currentFrame - 1, out var keys) ? keys : PlayerInput.DefaultPlayerKeys(),
                            playerPressedKeys);
                if (!input.DirectionalInputsByFrame.ContainsKey(currentFrame))
                    input.DirectionalInputsByFrame[currentFrame] = directionalInputs[playerIndex];
            });
        }

        private static Dictionary<PlayerKey, KeyState> UpdatePlayerKeys(
            Dictionary<PlayerKey, KeyState> currentPlayerKeys,
            ICollection<PlayerKey> pressedKeys)
        {
            return currentPlayerKeys.ToDictionary(kvp => kvp.Key,
                kvp => new KeyState(pressedKeys.Contains(kvp.Key), pressedKeys.Contains(kvp.Key) != kvp.Value.Pressed));
        }

        private static Vector2 KeysToDirectionalInput(ICollection<PlayerKey> keys)
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
            playerMapper = mapperService.GetMapper<PlayerState>();
            transformMapper = mapperService.GetMapper<Transform2>();
            inputMapper = mapperService.GetMapper<PlayerInput>();
            // acceleratorMapper = mapperService.GetMapper<Accelerator>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => player.LocalPlayerIndex.IfSome(playerIndex =>
                playerEntitiesByIndex[playerIndex] = entityId
            ));
        }
    }
}
