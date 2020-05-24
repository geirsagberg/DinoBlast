using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using MessagePack;
using MonoGame.Extended;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Serialization
{
    [MessagePackObject]
    public class FullGameState
    {
        [Key(0)] public int FrameCounter { get; }

        [Key(1)] public SerializableComponents Components { get; }

        [Key(2)] public DateTime UtcNow { get; }

        [Key(3)] public DateTime ResumeAtUtc { get; }

        [Key(4)] public int YourPeerId { get; }

        public FullGameState(int frameCounter, SerializableComponents components, DateTime utcNow, DateTime resumeAtUtc, int yourPeerId)
        {
            FrameCounter = frameCounter;
            Components = components;
            UtcNow = utcNow;
            ResumeAtUtc = resumeAtUtc;
            YourPeerId = yourPeerId;
        }

        public static FullGameState CreateFullGameState(IComponentMapperService componentManager, IEnumerable<int> entities,
            int frameCounter, DateTime utcNow, DateTime resumeAt, int yourPeerId)
        {
            var transformMapper = componentManager.GetMapper<Transform2>();
            var movableMapper = componentManager.GetMapper<Movable>();
            var spriteInfoMapper = componentManager.GetMapper<SpriteInfo>();
            var collisionBodyMapper = componentManager.GetMapper<CollisionBody>();
            var damagingMapper = componentManager.GetMapper<Damaging>();
            var gravityFieldMapper = componentManager.GetMapper<GravityField>();
            var gravityPointMapper = componentManager.GetMapper<GravityPoint>();
            var healthMapper = componentManager.GetMapper<Health>();
            var levelMapper = componentManager.GetMapper<Level>();
            var playerInputMapper = componentManager.GetMapper<PlayerInput>();
            var playerStateMapper = componentManager.GetMapper<PlayerState>();
            var emitterMapper = componentManager.GetMapper<Emitter>();
            var lifetimeMapper = componentManager.GetMapper<Lifetime>();

            var transforms = new Dictionary<int, SerializableTransform>();
            var movables = new Dictionary<int, Movable>();
            var spriteInfos = new Dictionary<int, SpriteInfo>();
            var collisionBodies = new Dictionary<int, CollisionBody>();
            var damagings = new Dictionary<int, Damaging>();
            var gravityFields = new Dictionary<int, GravityField>();
            var gravityPoints = new Dictionary<int, GravityPoint>();
            var healths = new Dictionary<int, Health>();
            var levels = new Dictionary<int, Level>();
            var playerInputs = new Dictionary<int, PlayerInput>();
            var playerStates = new Dictionary<int, PlayerState>();
            var emitters = new Dictionary<int, Emitter>();
            var lifetimes = new Dictionary<int, Lifetime>();

            var entityIds = entities.ToHashSet();

            foreach (var entityId in entityIds) {
                if (transformMapper.Get(entityId) is { } transform2)
                    transforms[entityId] = new SerializableTransform
                        { Position = transform2.Position, Rotation = transform2.Rotation, Scale = transform2.Scale };
                if (movableMapper.Get(entityId) is {} movable)
                    movables[entityId] = movable;
                if (spriteInfoMapper.Get(entityId) is {} spriteInfo)
                    spriteInfos[entityId] = spriteInfo;
                if (collisionBodyMapper.Get(entityId) is {} collisionBody)
                    collisionBodies[entityId] = collisionBody;
                if (damagingMapper.Get(entityId) is {} damaging)
                    damagings[entityId] = damaging;
                if (gravityFieldMapper.Get(entityId) is {} gravityField)
                    gravityFields[entityId] = gravityField;
                if (gravityPointMapper.Get(entityId) is {} gravityPoint)
                    gravityPoints[entityId] = gravityPoint;
                if (healthMapper.Get(entityId) is {} health)
                    healths[entityId] = health;
                if (levelMapper.Get(entityId) is {} level)
                    levels[entityId] = level;
                if (playerInputMapper.Get(entityId) is {} playerInput)
                    playerInputs[entityId] = playerInput;
                if (playerStateMapper.Get(entityId) is {} playerState)
                    playerStates[entityId] = playerState;
                if (emitterMapper.Get(entityId) is {} emitter)
                    emitters[entityId] = emitter;
                if (lifetimeMapper.Get(entityId) is {} lifetime)
                    lifetimes[entityId] = lifetime;
            }

            var serializableComponents = new SerializableComponents(entityIds, transforms, movables,
                spriteInfos,
                collisionBodies, damagings, gravityFields, gravityPoints, healths, playerInputs, levels, playerStates, emitters, lifetimes);
            return new FullGameState(frameCounter, serializableComponents, utcNow, resumeAt, yourPeerId);
        }
    }
}
