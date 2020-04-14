using BunnyLand.DesktopGL.Messages;
using LiteNetLib.Utils;

namespace BunnyLand.DesktopGL.Serialization
{
    public class FullGameState : INetSerializable
    {
        private readonly Serializer serializer;

        public SerializableComponents Components { get; set; }

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
    }
}
