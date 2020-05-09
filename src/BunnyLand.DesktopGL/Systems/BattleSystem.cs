using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class BattleSystem : EntityProcessingSystem
    {
        private readonly ConcurrentDictionary<int, int> entitiesBySerializableId = new ConcurrentDictionary<int, int>();

        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;

        private readonly List<INotification> newMessages = new List<INotification>();
        private readonly Dictionary<Type, Delegate> notificationHandlers;

        private readonly ConcurrentDictionary<int, Player> playerEntities = new ConcurrentDictionary<int, Player>();
        private readonly Random random;
        private readonly SharedContext sharedContext;

        private readonly Dictionary<PlayerIndex, Vector2> startPositions = new Dictionary<PlayerIndex, Vector2> {
            { PlayerIndex.One, new Vector2(100, 100) },
            { PlayerIndex.Two, new Vector2(900, 100) },
            { PlayerIndex.Three, new Vector2(100, 600) },
            { PlayerIndex.Four, new Vector2(900, 600) }
        };

        private ComponentMapper<CollisionBody> bodyMapper = null!;

        private ComponentMapper<Player> playerMapper = null!;

        private ComponentMapper<Serializable> serializableMapper = null!;
        private bool stateInitializationCompleted;

        public BattleSystem(EntityFactory entityFactory, GameSettings gameSettings, Random random, MessageHub messageHub, SharedContext sharedContext) :
            base(Aspect.All())
        {
            this.entityFactory = entityFactory;
            this.gameSettings = gameSettings;
            this.random = random;
            this.sharedContext = sharedContext;

            notificationHandlers = new Dictionary<Type, Delegate> {
                { typeof(ResetWorldMessage), new Action<ResetWorldMessage>(HandleResetWorld) },
                { typeof(UpdateGameMessage), new Action<UpdateGameMessage>(HandleUpdateGame) },
                { typeof(PlayerJoinedMessage), new Action<PlayerJoinedMessage>(HandlePlayerJoined) },
                { typeof(PlayerLeftMessage), new Action<PlayerLeftMessage>(HandlePlayerLeft) },
                { typeof(RespawnPlayerMessage), new Action<RespawnPlayerMessage>(msg => RespawnPlayer(msg.PlayerIndex)) }
            };

            messageHub.SubscribeMany(HandleNewMessage, notificationHandlers);
        }

        private void HandleNewMessage(INotification msg)
        {
            if (msg is UpdateGameMessage) {
                newMessages.RemoveAll(m => m is UpdateGameMessage);
            }
            newMessages.Add(msg);
        }

        private void HandlePlayerLeft(PlayerLeftMessage msg)
        {
            var (entityId, player) = playerEntities.FirstOrDefault(kvp => kvp.Value.PeerId == msg.PeerId);
            if (player != null) {
                DestroyEntity(entityId);
            }
        }

        private void HandlePlayerJoined(PlayerJoinedMessage msg)
        {
            var firstFreePlayerIndex = (PlayerIndex) Enumerable.Range(1, 16).First(i => playerEntities.Values.All(p => p.PlayerIndex != (PlayerIndex) i));
            entityFactory.CreatePlayer(CreateEntity(), startPositions[firstFreePlayerIndex], firstFreePlayerIndex, false, msg.PeerId);
        }

        private void HandleResetWorld(ResetWorldMessage msg)
        {
            entitiesBySerializableId.Clear();
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
        }

        private void HandleUpdateGame(UpdateGameMessage msg)
        {
            if (!stateInitializationCompleted) return;
            foreach (var serializable in msg.Components.SerializableIds) {
                if (entitiesBySerializableId.TryGetValue(serializable, out var entityId)) {
                    var entity = GetEntity(entityId);
                    if (msg.Components.Movables.TryGetValue(serializable, out var movable)) {
                        UpdateMovable(entity, movable);
                    }

                    if (msg.Components.Transforms.TryGetValue(serializable, out var serializableTransform)) {
                        UpdateTransform(entity, serializableTransform);
                    }

                    if (msg.Components.SpriteInfos.TryGetValue(serializable, out var spriteInfo)) {
                        UpdateSpriteInfo(entity, spriteInfo);
                    }
                } else {
                    CreateEntity(serializable, msg.Components);
                }
            }

            foreach (var (_, entityId) in entitiesBySerializableId.Where(kvp => !msg.Components.SerializableIds.Contains(kvp.Key))) {
                DestroyEntity(entityId);
            }
        }

        private static void UpdateSpriteInfo(Entity entity, SpriteInfo spriteInfo)
        {
            if (entity.Get<SpriteInfo>() is {} existing) {
                existing.Size = spriteInfo.Size;
                existing.SpriteType = spriteInfo.SpriteType;
            } else {
                entity.Attach(spriteInfo);
            }
        }

        private static void UpdateTransform(Entity entity, SerializableTransform serializableTransform)
        {
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

        private static void UpdateMovable(Entity entity, Movable movable)
        {
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

        private void SetupEntities(SerializableComponents gameStateComponents)
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);

            foreach (var serializable in gameStateComponents.SerializableIds) {
                CreateEntity(serializable, gameStateComponents);
            }
        }

        private void CreateEntity(int serializableId, SerializableComponents gameStateComponents)
        {
            var entity = CreateEntity();
            entity.Attach(new Serializable(serializableId));
            if (gameStateComponents.Transforms.TryGetValue(serializableId, out var serializableTransform)) {
                var transform = new Transform2(serializableTransform.Position, serializableTransform.Rotation, serializableTransform.Scale);
                entity.Attach(transform);
            }

            if (gameStateComponents.Movables.TryGetValue(serializableId, out var movable)) {
                entity.Attach(movable);
            }

            if (gameStateComponents.SpriteInfos.TryGetValue(serializableId, out var spriteInfo)) {
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
                    entityFactory.CreatePlayer(CreateEntity(), startPositions[index], index);
                }
            }
        }

        protected override void OnEntityAdded(int entityId)
        {
            serializableMapper.TryGet(entityId).IfSome(serializable => entitiesBySerializableId[serializable.Id] = entityId);
            playerMapper.TryGet(entityId).IfSome(player => playerEntities[entityId] = player);
        }

        protected override void OnEntityRemoved(int entityId)
        {
            serializableMapper.TryGet(entityId).IfSome(serializable => entitiesBySerializableId.Remove(serializable.Id, out _));
            playerEntities.Remove(entityId, out _);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            serializableMapper = mapperService.GetMapper<Serializable>();
            playerMapper = mapperService.GetMapper<Player>();
            bodyMapper = mapperService.GetMapper<CollisionBody>();
        }

        public override void End()
        {
            foreach (var message in newMessages) {
                // if (message is UpdateGameMessage updateGameMessage) {
                //     Console.WriteLine(updateGameMessage.Components.SerializableIds.ToJoinedString());
                // }
                notificationHandlers[message.GetType()].DynamicInvoke(message);
            }
            newMessages.Clear();
            if (!stateInitializationCompleted && ActiveEntities.Any()) {
                stateInitializationCompleted = true;
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            if (sharedContext.IsClient) return;

            bodyMapper.TryGet(entityId).IfSome(body => {
                var entity = GetEntity(entityId);

                foreach (var (other, _) in body.Collisions) {
                    var otherEntity = GetEntity(other);
                    entity.TryGet<Damaging>().IfSome(damaging => otherEntity.TryGet<Health>().IfSome(health => {
                        health.CurrentHealth -= damaging.Damage;
                        if (health.CurrentHealth < 0) {
                            Kill(otherEntity);
                        }
                    }));
                }
            });
        }

        private void Kill(Entity otherEntity)
        {
            otherEntity.TryGet<Player>().IfSome(player => {
                otherEntity.Attach(new Health(100));
                otherEntity.Attach(new Movable());
                otherEntity.Attach(new Transform2(startPositions[player.PlayerIndex]));
            });
        }
    }
}
