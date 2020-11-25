using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace MultiPong.DesktopGL.Systems
{
    public class RenderSystem : EntityDrawSystem
    {
        private ComponentMapper<Physical> _physicalMapper;

        public RenderSystem() : base(Aspect.All())
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _physicalMapper = mapperService.GetMapper<Physical>();
        }

        public override void Draw(GameTime gameTime)
        {

        }
    }
}
