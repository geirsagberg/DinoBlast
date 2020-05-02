using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using LiteNetLib.Utils;
using MonoGame.Extended;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Serialization
{
    public class FullGameState : INetSerializable
    {
        private readonly Serializer serializer;

        public SerializableComponents? Components { get; set; }

        public FullGameState(Serializer serializer)
        {
            this.serializer = serializer;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(serializer.Serialize(Components));
        }

        public void Deserialize(NetDataReader reader)
        {
            Components = serializer.Deserialize<SerializableComponents>(reader.GetRemainingBytes());
        }

        public static FullGameState CreateFullGameState(Serializer serializer, IEnumerable<int> entities,
            ComponentMapper<Serializable> serializableMapper, ComponentMapper<Transform2> transformMapper, ComponentMapper<Movable> movableMapper,
            ComponentMapper<SpriteInfo> spriteInfoMapper)
        {
            var serializables = new List<Serializable>();
            var transforms = new Dictionary<int, Transform2>();
            var movables = new Dictionary<int, Movable>();
            var spriteInfos = new Dictionary<int, SpriteInfo>();

            foreach (var entity in entities) {
                serializables.Add(serializableMapper.Get(entity));
                transformMapper.TryGet(entity).IfSome(transform => transforms[entity] = transform);
                movableMapper.TryGet(entity).IfSome(movable => movables[entity] = movable);
                spriteInfoMapper.TryGet(entity).IfSome(spriteInfo => spriteInfos[entity] = spriteInfo);
            }

            var serializableTransforms = transforms.ToDictionary(kvp => kvp.Key,
                kvp => new SerializableTransform { Position = kvp.Value.Position, Rotation = kvp.Value.Rotation, Scale = kvp.Value.Scale });

            return new FullGameState(serializer) {
                Components = new SerializableComponents(serializables.Select(s => s.Id).ToHashSet(), serializableTransforms, movables, spriteInfos)
            };
        }
    }
}
