using System.Collections.Generic;
using MessagePack;

namespace BunnyLand.DesktopGL.Serialization
{
    [MessagePackObject]
    public class SerializableGenericComponents
    {
        [Key(0)] public Dictionary<int, List<ISerializableComponent>> ComponentsByEntityId { get; }

        public SerializableGenericComponents(Dictionary<int, List<ISerializableComponent>> componentsByEntityId)
        {
            ComponentsByEntityId = componentsByEntityId;
        }
    }
}
