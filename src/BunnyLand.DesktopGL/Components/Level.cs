using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class Level
    {
        public RectangleF Bounds { get; set; }

        public Level(RectangleF bounds)
        {
            Bounds = bounds;
        }
    }
}
