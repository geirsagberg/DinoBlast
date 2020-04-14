using System.Collections.Generic;
using BunnyLand.DesktopGL.Components;
using MessagePack;

namespace BunnyLand.DesktopGL.Serialization
{
    [MessagePackObject]
    public class SerializableComponents
    {
        [Key(0)] public List<Serializable> Serializables { get; }

        [Key(1)] public Dictionary<int, SerializableTransform> Transforms { get; }

        [Key(2)] public Dictionary<int, Movable> Movables { get; }

        public SerializableComponents(List<Serializable> serializables, Dictionary<int, SerializableTransform> transforms, Dictionary<int, Movable> movables)
        {
            Serializables = serializables;
            Transforms = transforms;
            Movables = movables;
        }
    }
}
