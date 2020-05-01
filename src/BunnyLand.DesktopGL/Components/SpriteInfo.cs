using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class SpriteInfo
    {
        public SpriteType SpriteType { get; set; }
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
