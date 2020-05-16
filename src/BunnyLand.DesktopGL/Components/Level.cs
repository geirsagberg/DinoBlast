using MessagePack;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class Level : ISerializableComponent
    {
        [Key(0)]
        public RectangleF Bounds { get; }

        public Level(RectangleF bounds)
        {
            Bounds = bounds;
        }
    }
}
