using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Components
{
    public class SolidColor
    {
        public Color Color { get; }
        public RectangleF Bounds { get; }

        public SolidColor(Color color, RectangleF bounds)
        {
            Color = color;
            Bounds = bounds;
        }
    }
}
