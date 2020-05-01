using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class BattleSystem : EntitySystem
    {
        private readonly HashSet<int> entities = new HashSet<int>();

        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;
        private readonly Random random;

        private readonly Dictionary<PlayerIndex, (int, int)> startPositions = new Dictionary<PlayerIndex, (int, int)> {
            { PlayerIndex.One, (100, 100) },
            { PlayerIndex.Two, (900, 100) },
            { PlayerIndex.Three, (100, 600) },
            { PlayerIndex.Four, (900, 600) }
        };

        public BattleSystem(EntityFactory entityFactory, GameSettings gameSettings, Random random, MessageHub messageHub) : base(Aspect.All())
        {
            this.entityFactory = entityFactory;
            this.gameSettings = gameSettings;
            this.random = random;
            messageHub.Subscribe((ResetWorldMessage msg) => {
                foreach (var entity in entities) {
                    DestroyEntity(entity);
                }

                if (msg.GameState == null)
                    SetupEntities();
                else
                    SetupEntities(msg.GameState);
            });
            messageHub.Subscribe((RespawnPlayerMessage msg) => RespawnPlayer(msg.PlayerIndex));
        }

        private void SetupEntities(FullGameState gameState)
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);

            if (gameState.Components != null) {
                foreach (var serializable in gameState.Components.Serializables) {
                    var entity = CreateEntity();
                    entity.Attach(serializable);
                    if (gameState.Components.Transforms.TryGetValue(serializable.Id, out var serializableTransform)) {
                        var transform = new Transform2(serializableTransform.Position, serializableTransform.Rotation, serializableTransform.Scale);
                        entity.Attach(transform);
                    }

                    if (gameState.Components.Movables.TryGetValue(serializable.Id, out var movable)) {
                        entity.Attach(movable);
                    }

                    if (gameState.Components.SpriteInfos.TryGetValue(serializable.Id, out var spriteInfo)) {
                        entity.Attach(spriteInfo);
                    }
                }
            }
        }

        public void RespawnPlayer(PlayerIndex playerIndex)
        {
            entityFactory.CreatePlayer(CreateEntity(),
                new Vector2(gameSettings.Width * random.NextSingle(), gameSettings.Height * random.NextSingle()),
                playerIndex);
        }

        private void SetupEntities()
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(250, 500), 3000, 0.3f);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(700, 300), 5000, 0.5f);
            entityFactory.CreatePlayer(CreateEntity(), new Vector2(100, 100), PlayerIndex.One);
            // entityFactory.CreatePlayer(CreateEntity(), new Vector2(800, 700), PlayerIndex.Two);
            // entityFactory.CreatePlanet(CreateEntity(), new Vector2(800, 600), 0, 0.05f);
            // entityFactory.CreateBlock(CreateEntity(), new RectangleF(600, 600, 10, 200));
            var playerIndices = EnumHelper.GetValues<PlayerIndex>();
            foreach (var index in playerIndices.Skip(1)) {
                if (GamePad.GetState(index).IsConnected) {
                    var (x, y) = startPositions[index];
                    entityFactory.CreatePlayer(CreateEntity(), new Vector2(x, y), index);
                }
            }
        }

        protected override void OnEntityAdded(int entityId)
        {
            entities.Add(entityId);
        }

        protected override void OnEntityRemoved(int entityId)
        {
            entities.Remove(entityId);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
        }
    }
}
