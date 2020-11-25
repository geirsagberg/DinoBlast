using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace MultiPong.DesktopGL.Systems
{
    public class PhysicsSystem : EntityProcessingSystem
    {
        private ComponentMapper<Physical> _physicalMapper = null!;

        public PhysicsSystem() : base(Aspect.All(typeof(Physical)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _physicalMapper = mapperService.GetMapper<Physical>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {

        }
        // TODO: Derived components? Nested components?

        // Every component type must be either present or not
        // One component can define dependencies on other components
        // Can check runtime that component dependencies are present
        // EXAMPLE:
        // Physical with shape
        // - RenderedPhysical, depends on Physical, uses its shape for rendering Solid color!
    }

    public record Physical(IShapeF Shape, Vector2 Movement, Vector2 Acceleration);
}
