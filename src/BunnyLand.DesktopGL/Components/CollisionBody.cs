using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class CollisionBody
    {
        public Size2 Size { get; set; }

        public CollisionBody(Size2 size)
        {
            Size = size;
        }
    }
}