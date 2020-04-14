using MessagePack;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Serialization
{
    [MessagePackObject]
    public class SerializableTransform
    {
        [Key(0)] public Vector2 Position { get; set; }

        [Key(1)] public float Rotation { get; set; }

        [Key(2)] public Vector2 Scale { get; set; }
    }
}