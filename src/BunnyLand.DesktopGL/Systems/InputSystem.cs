using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
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

        private readonly Dictionary<PlayerIndex, HashSet<PlayerKey>> pressedKeys = Enum.GetValues(typeof(PlayerIndex))
            .Cast<PlayerIndex>().ToDictionary(i => i, _ => new HashSet<PlayerKey>());

        private ComponentMapper<Movable> movableMapper;

        private ComponentMapper<Player> playerMapper;

        public InputSystem(KeyboardListener keyboardListener, GamePadListener gamePadListener, IKeyMap keyMap) : base(
            Aspect.All(typeof(Player), typeof(Movable)))
        {
            this.keyboardListener = keyboardListener;
            this.gamePadListener = gamePadListener;
            this.keyMap = keyMap;

            keyboardListener.KeyPressed += OnKeyPressed;
            keyboardListener.KeyReleased += OnKeyRelease;
        }

        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            keyMap.GetKey(e.Key)
                .IfSome(t => pressedKeys[t.index].Add(t.key));
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

            var accelerationMultiplier = keys.Contains(PlayerKey.Jump) ? 2f : 0.5f;
            movable.Acceleration = player.StandingOn switch {
                StandingOn.Nothing => new Vector2(
                    (keys.Contains(PlayerKey.Left) ? -1 : 0) + (keys.Contains(PlayerKey.Right) ? 1 : 0),
                    (keys.Contains(PlayerKey.Up) ? -1 : 0) + (keys.Contains(PlayerKey.Down) ? 1 : 0)).NormalizedOrZero() * accelerationMultiplier,
                StandingOn.Planet => Vector2.Zero,
                _ => Vector2.Zero
            };

            // if (player.StandingOn == StandingOn.Nothing) {
            //     if (keys.Contains(PlayerKey.Left))
            // }
        }
    }
}
