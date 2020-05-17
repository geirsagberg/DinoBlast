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

        public FullGameState(int frameCounter, SerializableComponents components)
        {
            FrameCounter = frameCounter;
            Components = components;
        }

        public static FullGameState CreateFullGameState(IComponentMapperService componentManager, IEnumerable<int> entities,
            int frameCounter)
        {
            var serializableMapper = componentManager.GetMapper<Serializable>();
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

            var serializableIds = entities.Select(e => (entityId: e, serializableId: serializableMapper.Get(e).Id)).ToList();
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

            foreach (var (entityId, serializableId) in serializableIds) {
                if (transformMapper.Get(entityId) is { } transform2)
                    transforms[serializableId] = new SerializableTransform
                        { Position = transform2.Position, Rotation = transform2.Rotation, Scale = transform2.Scale };
                if (movableMapper.Get(entityId) is {} movable)
                    movables[serializableId] = movable;
                if (spriteInfoMapper.Get(entityId) is {} spriteInfo)
                    spriteInfos[serializableId] = spriteInfo;
                if (collisionBodyMapper.Get(entityId) is {} collisionBody)
                    collisionBodies[serializableId] = collisionBody;
                if (damagingMapper.Get(entityId) is {} damaging)
                    damagings[serializableId] = damaging;
                if (gravityFieldMapper.Get(entityId) is {} gravityField)
                    gravityFields[serializableId] = gravityField;
                if (gravityPointMapper.Get(entityId) is {} gravityPoint)
                    gravityPoints[serializableId] = gravityPoint;
                if (healthMapper.Get(entityId) is {} health)
                    healths[serializableId] = health;
                if (levelMapper.Get(entityId) is {} level)
                    levels[serializableId] = level;
                if (playerInputMapper.Get(entityId) is {} playerInput)
                    playerInputs[serializableId] = playerInput;
            }

            return new FullGameState(frameCounter, new SerializableComponents(serializableIds.Select(t => t.serializableId).ToHashSet(), transforms, movables,
                spriteInfos,
                collisionBodies, damagings, gravityFields, gravityPoints, healths, playerInputs, levels));
        }
    }
}
