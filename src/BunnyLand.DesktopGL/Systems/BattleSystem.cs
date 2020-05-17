using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using BunnyLand.DesktopGL.Utils;
using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class BattleSystem : EntityProcessingSystem
    {
        private static readonly IEnumerable<byte> _playerNumbers = Enumerable.Range(1, 16).Select(i => (byte) i);
        private readonly IDictionary<int, int> entitiesByPeerId = new Dictionary<int, int>();
        private readonly IDictionary<byte, int> entitiesByPlayerNumber = new Dictionary<byte, int>();
        private readonly IDictionary<int, int> entitiesBySerializableId = new Dictionary<int, int>();

        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;

        private readonly List<INotification> newMessages = new List<INotification>();
        private readonly Dictionary<Type, BoundMethod> notificationHandlers;
        private readonly SharedContext sharedContext;

        private ComponentMapper<CollisionBody> bodyMapper = null!;
        private ComponentMapper<PlayerState> playerMapper = null!;
        private ComponentMapper<Serializable> serializableMapper = null!;
        private bool stateInitializationCompleted;

        public BattleSystem(EntityFactory entityFactory, GameSettings gameSettings, MessageHub messageHub, SharedContext sharedContext) :
            base(Aspect.All())
        {
            this.entityFactory = entityFactory;
            this.gameSettings = gameSettings;
            this.sharedContext = sharedContext;

            notificationHandlers = new Dictionary<Type, BoundMethod> {
                { typeof(ResetWorldMessage), new Action<ResetWorldMessage>(HandleResetWorld).Method.Bind() },
                { typeof(UpdateGameMessage), new Action<UpdateGameMessage>(HandleUpdateGame).Method.Bind() },
                { typeof(PlayerJoinedMessage), new Action<PlayerJoinedMessage>(HandlePlayerJoined).Method.Bind() },
                { typeof(PlayerLeftMessage), new Action<PlayerLeftMessage>(HandlePlayerLeft).Method.Bind() },
                { typeof(RespawnPlayerMessage), new Action<RespawnPlayerMessage>(HandleRespawnPlayer).Method.Bind() }
            };

            messageHub.SubscribeMany(HandleNewMessage, notificationHandlers);
        }

        private void HandleRespawnPlayer(RespawnPlayerMessage msg)
        {
            RespawnPlayer(msg.PlayerNumber);
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
            entitiesByPeerId.TryGetValue(msg.PeerId).IfSome(DestroyEntity);
        }

        private void HandlePlayerJoined(PlayerJoinedMessage msg)
        {
            var firstFreePlayerNumber = GetFirstFreePlayerNumber();
            entityFactory.CreatePlayer(CreateEntity(), GetStartPosition(firstFreePlayerNumber), firstFreePlayerNumber, default, msg.PeerId);
        }

        private byte GetFirstFreePlayerNumber() => _playerNumbers.First(i => !entitiesByPlayerNumber.ContainsKey(i));

        private static Vector2 GetStartPosition(in int playerNumber) => playerNumber switch {
            1 => new Vector2(100, 100),
            2 => new Vector2(900, 600),
            3 => new Vector2(600, 100),
            4 => new Vector2(600, 600),
            _ => new Vector2(900, 900)
        };

        private void HandleResetWorld(ResetWorldMessage msg)
        {
            entitiesBySerializableId.Clear();
            foreach (var entity in ActiveEntities) {
                DestroyEntity(entity);
            }

            sharedContext.FrameCounter = msg.FrameCounter;
            if (msg.GameState == null) {
                SetupEntities();
            } else if (msg.GameState.Components != null) {
                SetupEntities(msg.GameState.Components);
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

            if (gameStateComponents.Movables.TryGetValue(serializableId, out var movable))
                entity.Attach(movable);
            if (gameStateComponents.SpriteInfos.TryGetValue(serializableId, out var spriteInfo))
                entity.Attach(spriteInfo);
            if (gameStateComponents.Damagings.TryGetValue(serializableId, out var damaging))
                entity.Attach(damaging);
            if (gameStateComponents.Healths.TryGetValue(serializableId, out var health))
                entity.Attach(health);
            if (gameStateComponents.Levels.TryGetValue(serializableId, out var level))
                entity.Attach(level);
            if (gameStateComponents.CollisionBodies.TryGetValue(serializableId, out var collisionBody))
                entity.Attach(collisionBody);
            if (gameStateComponents.GravityFields.TryGetValue(serializableId, out var gravityField))
                entity.Attach(gravityField);
            if (gameStateComponents.GravityPoints.TryGetValue(serializableId, out var gravityPoint))
                entity.Attach(gravityPoint);
            if (gameStateComponents.PlayerInputs.TryGetValue(serializableId, out var playerInput))
                entity.Attach(playerInput);
        }

        public void RespawnPlayer(byte playerNumber)
        {
            var entity = entitiesByPlayerNumber[playerNumber];
            var playerState = GetEntity(entity).Get<PlayerState>();
            entityFactory.CreatePlayer(CreateEntity(),
                GetStartPosition(playerNumber), playerNumber,
                playerState.LocalPlayerIndex);
        }

        private void SetupEntities()
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(250, 500), 3000, 0.3f);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(700, 300), 5000, 0.5f);
            entityFactory.CreatePlayer(CreateEntity(), new Vector2(100, 100), GetFirstFreePlayerNumber(), PlayerIndex.One);
            // entityFactory.CreatePlayer(CreateEntity(), new Vector2(800, 700), PlayerIndex.Two);
            // entityFactory.CreatePlanet(CreateEntity(), new Vector2(800, 600), 0, 0.05f);
            // entityFactory.CreateBlock(CreateEntity(), new RectangleF(600, 600, 10, 200));
            var playerIndices = EnumHelper.GetValues<PlayerIndex>();
            foreach (var index in playerIndices.Skip(1)) {
                if (GamePad.GetState(index).IsConnected) {
                    var nextPlayerNumber = GetFirstFreePlayerNumber();
                    entityFactory.CreatePlayer(CreateEntity(), GetStartPosition(nextPlayerNumber), nextPlayerNumber, index);
                }
            }
        }

        protected override void OnEntityAdded(int entityId)
        {
            serializableMapper.TryGet(entityId).IfSome(serializable => entitiesBySerializableId[serializable.Id] = entityId);
            playerMapper.TryGet(entityId).IfSome(player => {
                entitiesByPlayerNumber[player.PlayerNumber] = entityId;
                player.PeerId.IfSome(peerId => entitiesByPeerId[peerId] = entityId);
            });
        }

        protected override void OnEntityRemoved(int entityId)
        {
            serializableMapper.TryGet(entityId).IfSome(serializable => entitiesBySerializableId.Remove(serializable.Id, out _));
            playerMapper.TryGet(entityId).IfSome(player => {
                entitiesByPlayerNumber.Remove(player.PlayerNumber, out _);
                player.PeerId.IfSome(peerId => entitiesByPeerId.Remove(peerId, out _));
            });
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            serializableMapper = mapperService.GetMapper<Serializable>();
            playerMapper = mapperService.GetMapper<PlayerState>();
            bodyMapper = mapperService.GetMapper<CollisionBody>();
        }

        public override void End()
        {
            foreach (var message in newMessages) {
                var notificationHandler = notificationHandlers[message.GetType()];
                notificationHandler(this, new object[] { message });
            }

            newMessages.Clear();
            if (!stateInitializationCompleted && ActiveEntities.Any()) {
                stateInitializationCompleted = true;
            }

            if (stateInitializationCompleted)
                sharedContext.FrameCounter++;
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            if (sharedContext.IsClient) return;

            var entity = GetEntity(entityId);
            var body = entity.Get<CollisionBody>();
            if (body != null) {
                foreach (var (other, _) in body.Collisions) {
                    var otherEntity = GetEntity(other);
                    var damaging = entity.Get<Damaging>();
                    var health = otherEntity.Get<Health>();
                    if (damaging != null && health != null) {
                        health.CurrentHealth -= damaging.Damage;
                        if (health.CurrentHealth < 0) {
                            Kill(otherEntity);
                        }
                    }
                }
            }
        }

        private static void Kill(Entity otherEntity)
        {
            otherEntity.TryGet<PlayerState>().IfSome(player => {
                otherEntity.Attach(new Health(100));
                otherEntity.Attach(new Movable());
                otherEntity.Attach(new Transform2(GetStartPosition(player.PlayerNumber)));
            });
        }
    }
}
