using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace MultiPong.DesktopGL.Systems
{
    public class RenderSystem : EntityDrawSystem
    {
        public RenderSystem() : base(aspect.All())
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            mapperService.GetMapper<>()
        }

        public override void Draw(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }
    }
}
