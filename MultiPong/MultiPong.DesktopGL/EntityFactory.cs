using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MultiPong.DesktopGL.Systems;

namespace MultiPong.DesktopGL
{
    public static class EntityFactory
    {
        public static Entity CreatePaddle(this World world, short playerNumber)
        {
            var entity = world.CreateEntity();

            var x = playerNumber switch {
                0 => 20,
                _ => 100
            };

            var rectangleF = new RectangleF(x, 50, 20, 100);
            entity.Attach(new Physical(rectangleF, Vector2.Zero, Vector2.Zero));
            return entity;
        }
    }
}
