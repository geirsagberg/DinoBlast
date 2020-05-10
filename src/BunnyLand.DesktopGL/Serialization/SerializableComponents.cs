using System.Collections.Generic;
using System.Runtime.Serialization;
using BunnyLand.DesktopGL.Components;
using MessagePack;

namespace BunnyLand.DesktopGL.Serialization
{
    [MessagePackObject]
    public class SerializableComponents
    {
        [Key(0)] public HashSet<int> SerializableIds { get; }

        [Key(1)] public Dictionary<int, SerializableTransform> Transforms { get; }

        [Key(2)] public Dictionary<int, Movable> Movables { get; }

        [Key(3)] public Dictionary<int, SpriteInfo> SpriteInfos { get; }

        public SerializableComponents(HashSet<int> serializableIds, Dictionary<int, SerializableTransform> transforms,
            Dictionary<int, Movable> movables, Dictionary<int, SpriteInfo> spriteInfos)
        {
            SerializableIds = serializableIds;
            Transforms = transforms;
            Movables = movables;
            SpriteInfos = spriteInfos;
        }
    }

    [MessagePackObject()]
    public class SerializableGenericComponents
    {
        public SerializableGenericComponents(Dictionary<int, List<object>> componentsByEntityId)
        {
            ComponentsByEntityId = componentsByEntityId;
        }

        [Key(0)]
        public Dictionary<int, List<object>> ComponentsByEntityId { get; }
    }
}
