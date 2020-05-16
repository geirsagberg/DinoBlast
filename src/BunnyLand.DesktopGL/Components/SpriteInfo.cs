using MessagePack;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class SpriteInfo
    {
        [Key(0)]
        public SpriteType SpriteType { get; set; }
        [Key(1)]
        public Size Size { get; set; }

        public SpriteInfo(SpriteType spriteType, Size size)
        {
            SpriteType = spriteType;
            Size = size;
        }

        public SpriteInfo()
        {
        }
    }

    public enum SpriteType
    {
        Bunny,
        Anki,
        Planet1,
        Bullet
    }
}
