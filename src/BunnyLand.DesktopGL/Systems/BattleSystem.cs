using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
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
        private readonly ConcurrentDictionary<int, int> entitiesBySerializableId = new ConcurrentDictionary<int, int>();

        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;
        private readonly Random random;

        private readonly Dictionary<PlayerIndex, (int, int)> startPositions = new Dictionary<PlayerIndex, (int, int)> {
            { PlayerIndex.One, (100, 100) },
            { PlayerIndex.Two, (900, 100) },
            { PlayerIndex.Three, (100, 600) },
            { PlayerIndex.Four, (900, 600) }
        };

        private ComponentMapper<Serializable> serializableMapper = null!;

        public BattleSystem(EntityFactory entityFactory, GameSettings gameSettings, Random random, MessageHub messageHub) : base(Aspect.All())
        {
            this.entityFactory = entityFactory;
            this.gameSettings = gameSettings;
            this.random = random;
            messageHub.Subscribe((ResetWorldMessage msg) => {
                foreach (var entity in ActiveEntities) {
                    DestroyEntity(entity);
                }

                if (msg.GameState == null)
                    SetupEntities();
                else {
                    if (msg.GameState.Components != null) {
                        SetupEntities(msg.GameState.Components);
                    }
                }
            });
            messageHub.Subscribe((UpdateGameMessage msg) => {
                foreach (var serializable in msg.Components.Serializables) {
                    if (entitiesBySerializableId.TryGetValue(serializable.Id, out var entityId)) {
                        var entity = GetEntity(entityId);
                        if (msg.Components.Movables.TryGetValue(serializable.Id, out var movable)) {
                            if (entity.Get<Movable>() is {} existing) {
                                existing.Acceleration = movable.Acceleration;
                                existing.Velocity = movable.Velocity;
                                existing.BrakingForce = movable.BrakingForce;
                                existing.GravityMultiplier = movable.GravityMultiplier;
                                existing.GravityPull = movable.GravityPull;
                                existing.WrapAround = movable.WrapAround;
                            } else {
                                entity.Attach(movable);
                            }
                        }

                        if (msg.Components.Transforms.TryGetValue(serializable.Id, out var serializableTransform)) {
                            if (entity.Get<Transform2>() is {} existing) {
                                existing.Position = serializableTransform.Position;
                                existing.Rotation = serializableTransform.Rotation;
                                existing.Scale = serializableTransform.Scale;
                            } else {
                                var transform = new Transform2 {
                                    Position = serializableTransform.Position,
                                    Rotation = serializableTransform.Rotation,
                                    Scale = serializableTransform.Scale
                                };
                                entity.Attach(transform);
                            }
                        }

                        if (msg.Components.SpriteInfos.TryGetValue(serializable.Id, out var spriteInfo)) {
                            if (entity.Get<SpriteInfo>() is {} existing) {
                                existing.Size = spriteInfo.Size;
                                existing.SpriteType = spriteInfo.SpriteType;
                            } else {
                                entity.Attach(spriteInfo);
                            }
                        }
                    } else {
                        CreateEntity(serializable, msg.Components);
                    }
                }
            });
            messageHub.Subscribe((RespawnPlayerMessage msg) => RespawnPlayer(msg.PlayerIndex));
        }

        private void SetupEntities(SerializableComponents gameStateComponents)
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);

            foreach (var serializable in gameStateComponents.Serializables) {
                CreateEntity(serializable, gameStateComponents);
            }
        }

        private void CreateEntity(Serializable serializable, SerializableComponents gameStateComponents)
        {
            var entity = CreateEntity();
            entity.Attach(serializable);
            if (gameStateComponents.Transforms.TryGetValue(serializable.Id, out var serializableTransform)) {
                var transform = new Transform2(serializableTransform.Position, serializableTransform.Rotation, serializableTransform.Scale);
                entity.Attach(transform);
            }

            if (gameStateComponents.Movables.TryGetValue(serializable.Id, out var movable)) {
                entity.Attach(movable);
            }

            if (gameStateComponents.SpriteInfos.TryGetValue(serializable.Id, out var spriteInfo)) {
                entity.Attach(spriteInfo);
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
            serializableMapper.TryGet(entityId).IfSome(serializable => entitiesBySerializableId[serializable.Id] = entityId);
        }

        protected override void OnEntityRemoved(int entityId)
        {
            serializableMapper.TryGet(entityId).IfSome(serializable => entitiesBySerializableId.Remove(serializable.Id, out _));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            serializableMapper = mapperService.GetMapper<Serializable>();
        }
    }
}
