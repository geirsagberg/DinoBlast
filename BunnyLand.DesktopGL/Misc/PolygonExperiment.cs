using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.DesktopGL.Misc
{
    public static class PolygonExperiment
    {
        public static (BasicEffect effect, PolygonThingy polygon) CreateColoredPolygon(GraphicsDevice device,
            bool variant1)
        {
            BasicEffect basicEffect = new BasicEffect(device);
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;

            var polygon = new PolygonThingy(variant1);

            return (basicEffect, polygon);
        }
    }
}
