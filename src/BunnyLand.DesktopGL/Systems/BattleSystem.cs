using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using BunnyLand.DesktopGL.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class BattleSystem : EntityProcessingSystem
    {
        private static readonly IEnumerable<byte> PlayerNumbers = Enumerable.Range(1, 16).Select(i => (byte) i);

        private readonly IDictionary<byte, int> entitiesByPlayerNumber = new Dictionary<byte, int>();

        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;
        private readonly MessageHub messageHub;

        private readonly List<INotification> newMessages = new List<INotification>();
        private readonly Dictionary<Type, BoundMethod> notificationHandlers;
        private readonly SharedContext sharedContext;

        private ComponentMapper<PlayerState> playerMapper = null!;

        public BattleSystem(EntityFactory entityFactory, GameSettings gameSettings, MessageHub messageHub, SharedContext sharedContext) :
            base(Aspect.All())
        {
            this.entityFactory = entityFactory;
            this.gameSettings = gameSettings;
            this.messageHub = messageHub;
            this.sharedContext = sharedContext;

            notificationHandlers = new Dictionary<Type, BoundMethod> {
                { typeof(ResetWorldMessage), new Action<ResetWorldMessage>(HandleResetWorld).Method.Bind() },
                { typeof(PlayerJoinedMessage), new Action<PlayerJoinedMessage>(HandlePlayerJoined).Method.Bind() },
                { typeof(PlayerLeftMessage), new Action<PlayerLeftMessage>(HandlePlayerLeft).Method.Bind() },
                { typeof(RespawnPlayerMessage), new Action<RespawnPlayerMessage>(HandleRespawnPlayer).Method.Bind() },
                { typeof(ReceivedInputMessage), new Action<ReceivedInputMessage>(HandleReceivedInputs).Method.Bind() }
            };

            messageHub.SubscribeMany(HandleNewMessage, notificationHandlers);
        }

        private void HandleReceivedInputs(ReceivedInputMessage msg)
        {
            if (entitiesByPlayerNumber.TryGetValue(msg.PlayerNumber, out var entity)) {
                msg.Input.CurrentFrame = sharedContext.FrameCounter;
                GetEntity(entity).Attach(msg.Input);
            }
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
            // entitiesByPeerId.TryGetValue(msg.PeerId).IfSome(DestroyEntity);
        }

        private void HandlePlayerJoined(PlayerJoinedMessage msg)
        {
            for (var i = 1; i <= msg.PlayerCount; i++) {
                var firstFreePlayerNumber = GetFirstFreePlayerNumber();
                entityFactory.CreatePlayer(CreateEntity(), GetStartPosition(firstFreePlayerNumber), firstFreePlayerNumber, default, msg.PeerId);
            }
        }

        private byte GetFirstFreePlayerNumber() => PlayerNumbers.First(i => !entitiesByPlayerNumber.ContainsKey(i));

        private static Vector2 GetStartPosition(in int playerNumber) => playerNumber switch {
            1 => new Vector2(100, 100),
            2 => new Vector2(900, 600),
            3 => new Vector2(600, 100),
            4 => new Vector2(600, 600),
            _ => new Vector2(900, 900)
        };

        private void HandleResetWorld(ResetWorldMessage msg)
        {
            foreach (var entity in ActiveEntities) {
                DestroyEntity(entity);
            }

            sharedContext.FrameCounter = msg.FrameCounter;
            if (msg.GameState == null) {
                SetupEntities();
            } else {
                SetupEntities(msg.GameState.Components);
            }
        }

        private void SetupEntities(SerializableComponents gameStateComponents)
        {
            foreach (var serializable in gameStateComponents.EntityIds.OrderBy(id => id)) {
                CreateEntity(serializable, gameStateComponents);
            }
        }

        private void CreateEntity(int entityId, SerializableComponents gameStateComponents)
        {
            var entity = CreateEntity();
            Debug.Assert(entity.Id == entityId, "Expected created ID to match serializable ID");

            if (gameStateComponents.Transforms.TryGetValue(entityId, out var serializableTransform)) {
                var transform = new Transform2(serializableTransform.Position, serializableTransform.Rotation, serializableTransform.Scale);
                entity.Attach(transform);
            }

            if (gameStateComponents.Movables.TryGetValue(entityId, out var movable))
                entity.Attach(movable);
            if (gameStateComponents.SpriteInfos.TryGetValue(entityId, out var spriteInfo))
                entity.Attach(spriteInfo);
            if (gameStateComponents.Damagings.TryGetValue(entityId, out var damaging))
                entity.Attach(damaging);
            if (gameStateComponents.Healths.TryGetValue(entityId, out var health))
                entity.Attach(health);
            if (gameStateComponents.Levels.TryGetValue(entityId, out var level))
                entity.Attach(level);
            if (gameStateComponents.CollisionBodies.TryGetValue(entityId, out var collisionBody))
                entity.Attach(collisionBody);
            if (gameStateComponents.GravityFields.TryGetValue(entityId, out var gravityField))
                entity.Attach(gravityField);
            if (gameStateComponents.GravityPoints.TryGetValue(entityId, out var gravityPoint))
                entity.Attach(gravityPoint);
            if (gameStateComponents.PlayerInputs.TryGetValue(entityId, out var playerInput))
                entity.Attach(playerInput);
            if (gameStateComponents.PlayerStates.TryGetValue(entityId, out var playerState)) {
                if (playerState.PeerId == sharedContext.MyPeerId) {
                    // TODO: multiple players
                    playerState.LocalPlayerIndex = PlayerIndex.One;
                }

                entity.Attach(playerState);
            }

            if (gameStateComponents.Emitters.TryGetValue(entityId, out var emitter))
                entity.Attach(emitter);
            if (gameStateComponents.Lifetimes.TryGetValue(entityId, out var lifetime))
                entity.Attach(lifetime);
        }

        public void RespawnPlayer(byte playerNumber)
        {
            var entity = entitiesByPlayerNumber[playerNumber];
            var playerState = GetEntity(entity).Get<PlayerState>();
            entityFactory.CreatePlayer(CreateEntity(),
                GetStartPosition(playerNumber), playerNumber,
                playerState.LocalPlayerIndex, playerState.PeerId);
        }

        private void SetupEntities()
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(250, 500), 3000, 0.3f);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(700, 300), 5000, 0.5f);
            entityFactory.CreatePlayer(CreateEntity(), new Vector2(100, 100), GetFirstFreePlayerNumber(), PlayerIndex.One, default);
            // entityFactory.CreatePlayer(CreateEntity(), new Vector2(800, 700), PlayerIndex.Two);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(800, 800), 10000, 1f);
            // entityFactory.CreateBlock(CreateEntity(), new RectangleF(600, 600, 10, 200));
            var playerIndices = EnumHelper.GetValues<PlayerIndex>();
            foreach (var index in playerIndices.Skip(1)) {
                if (GamePad.GetState(index).IsConnected) {
                    var nextPlayerNumber = GetFirstFreePlayerNumber();
                    entityFactory.CreatePlayer(CreateEntity(), GetStartPosition(nextPlayerNumber), nextPlayerNumber, index, default);
                }
            }
        }

        protected override void OnEntityAdded(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => {
                entitiesByPlayerNumber[player.PlayerNumber] = entityId;
            });
        }

        protected override void OnEntityRemoved(int entityId)
        {
            playerMapper.TryGet(entityId).IfSome(player => {
                entitiesByPlayerNumber.Remove(player.PlayerNumber, out _);
            });
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            playerMapper = mapperService.GetMapper<PlayerState>();
        }

        public override void End()
        {
            var inputsByPlayerNumber = entitiesByPlayerNumber.Values.Select(GetEntity).Where(e => e.Get<PlayerState>().IsLocal).Select(p => {
                var input = p.Get<PlayerInput>();
                var state = p.Get<PlayerState>();
                return (input, state);
            }).ToDictionary(t => t.state.PlayerNumber, t => t.input);
            messageHub.Publish(new InputsUpdatedMessage(inputsByPlayerNumber));

            foreach (var message in newMessages) {
                var notificationHandler = notificationHandlers[message.GetType()];
                notificationHandler(this, new object[] { message });
            }

            newMessages.Clear();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            if (sharedContext.IsPaused)
                return;

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
