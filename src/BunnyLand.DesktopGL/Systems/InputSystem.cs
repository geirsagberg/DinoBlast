using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Utils;
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
        private readonly DebugLogger debugLogger;
        private readonly OrthographicCamera camera;

        private readonly Dictionary<PlayerIndex, System.Collections.Generic.HashSet<PlayerKey>> pressedKeys = Enum
            .GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new System.Collections.Generic.HashSet<PlayerKey>());

        private readonly IButtonMap buttonMap;
        private readonly IDictionary<PlayerIndex, int> playerEntitiesByIndex = new Dictionary<PlayerIndex, int>();

        private readonly SharedContext sharedContext;

        private ComponentMapper<PlayerInput> inputMapper = null!;
        private ComponentMapper<PlayerState> playerMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;

        private Point2 previousMousePosition;

        public InputSystem(MouseListener mouseListener, KeyboardListener keyboardListener,
            IEnumerable<GamePadListener> gamePadListeners, IButtonMap buttonMap, SharedContext sharedContext, DebugLogger debugLogger, OrthographicCamera camera
        ) : base(
            Aspect.All(typeof(PlayerState), typeof(PlayerInput)))
        {
            this.buttonMap = buttonMap;
            this.sharedContext = sharedContext;
            this.debugLogger = debugLogger;
            this.camera = camera;

            mouseListener.MouseDown += OnMouseDown;
            mouseListener.MouseUp += OnMouseUp;
            keyboardListener.KeyPressed += OnKeyPressed;
            keyboardListener.KeyReleased += OnKeyReleased;
            foreach (var gamePadListener in gamePadListeners) {
                gamePadListener.ButtonDown += OnButtonDown;
                gamePadListener.ButtonUp += OnButtonUp;
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

        private void OnTriggerMoved(object? sender, GamePadEventArgs e)
        {
            // Console.WriteLine($"TriggerMoved: ${e.Button} - ${e.TriggerState}");
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

        private void HandlePlayerKeyInput(PlayerIndex index, PlayerKey key, bool released = false)
        {
            if (released) {
                pressedKeys[index].Remove(key);
            } else {
                pressedKeys[index].Add(key);
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var state = playerMapper.Get(entityId);
            var input = inputMapper.Get(entityId);
            input.CurrentFrame = sharedContext.FrameCounter;

            state.LocalPlayerIndex.IfSome(playerIndex => {
                var currentFrame = sharedContext.FrameCounter + sharedContext.FrameOffset;

                if (input.PlayerKeysByFrame.Count >= PlayerInput.InitialFrameBuffer) {
                    input.PlayerKeysByFrame.Remove(currentFrame - PlayerInput.InitialFrameBuffer);
                }

                if (input.DirectionalInputsByFrame.Count >= PlayerInput.InitialFrameBuffer) {
                    input.DirectionalInputsByFrame.Remove(currentFrame - PlayerInput.InitialFrameBuffer);
                }

                var playerPressedKeys = pressedKeys[playerIndex];


                if (!input.PlayerKeysByFrame.ContainsKey(currentFrame))
                    input.PlayerKeysByFrame[currentFrame] =
                        UpdatePlayerKeys(input.PlayerKeysByFrame.TryGetValue(currentFrame - 1, out var keys) ? keys : PlayerInput.DefaultPlayerKeys(),
                            playerPressedKeys);


                if (!input.DirectionalInputsByFrame.ContainsKey(currentFrame)) {
                    input.DirectionalInputsByFrame[currentFrame] = GetDirectionalInputs(playerIndex, input, currentFrame);
                }
            });

            debugLogger.AddObject(input);


            if (!input.IsUpToDate()) {
                sharedContext.IsPaused = true;
                sharedContext.IsSyncing = true;
            }
        }

        private DirectionalInputs GetDirectionalInputs(PlayerIndex playerIndex, PlayerInput input, int currentFrame)
        {
            var keyboardDirection = KeysToDirectionalInput(pressedKeys[playerIndex]);
            var (leftStick, rightStick) = GetThumbSticks(playerIndex);
            var mousePosition = Mouse.GetState().Position;
            var mouseAim = playerIndex == PlayerIndex.One && mousePosition != previousMousePosition
                ? (camera.ScreenToWorld(mousePosition.X, mousePosition.Y) - transformMapper.Get(playerEntitiesByIndex[playerIndex]).Position).NormalizedOrZero()
                : Vector2.Zero;

            var acceleration = keyboardDirection != default ? keyboardDirection : leftStick.NormalizedOrZero();
            var aim = mouseAim != default ? mouseAim : rightStick.NormalizedOrZero();
            if (aim == Vector2.Zero && input.DirectionalInputsByFrame.ContainsKey(currentFrame - 1))
                aim = input.DirectionalInputsByFrame[currentFrame - 1].AimDirection;

            var directionalInputs = new DirectionalInputs(acceleration, aim);
            return directionalInputs;
        }

        private (Vector2 leftStick, Vector2 rightStick) GetThumbSticks(PlayerIndex playerIndex)
        {
            Vector2 flipY(Vector2 vector)
            {
                return vector.SetY(-vector.Y);
            }

            var thumbSticks = GamePad.GetState(playerIndex, GamePadDeadZone.Circular).ThumbSticks;
            return (flipY(thumbSticks.Left), flipY(thumbSticks.Right));
        }

        public override void End()
        {
            if (sharedContext.IsPaused && sharedContext.IsSyncing && ActiveEntities.All(id => {
                var entity = GetEntity(id);
                return entity.Get<PlayerState>().IsLocal || entity.Get<PlayerInput>().IsUpToDate();
            })) {
                sharedContext.IsPaused = false;
                sharedContext.IsSyncing = false;
            }

            previousMousePosition = Mouse.GetState().Position;
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

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<PlayerState>();
            transformMapper = mapperService.GetMapper<Transform2>();
            inputMapper = mapperService.GetMapper<PlayerInput>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => player.LocalPlayerIndex.IfSome(playerIndex =>
                playerEntitiesByIndex[playerIndex] = entityId
            ));
        }
    }
}
