using MessagePack;

namespace BunnyLand.DesktopGL.Serialization
{
    public class Serializer
    {
        private readonly MessagePackSerializerOptions options;

        public Serializer()
        {
            options = MessagePackSerializerOptions.Standard;
        }

        public byte[] Serialize<T>(T value) => MessagePackSerializer.Serialize(value, options);

        public T Deserialize<T>(in byte[] bytes) => MessagePackSerializer.Deserialize<T>(bytes, options);
    }
}
