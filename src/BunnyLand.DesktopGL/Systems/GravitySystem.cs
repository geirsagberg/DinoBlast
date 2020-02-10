using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class GravitySystem : EntityProcessingSystem
    {
        private readonly Bag<GravityPoint> points = new Bag<GravityPoint>();
        private ComponentMapper<GravityPoint> gravityPointMapper;
        private ComponentMapper<Movable> movableMapper;

        public GravitySystem() : base(Aspect.All(typeof(Movable)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            movableMapper = mapperService.GetMapper<Movable>();
            gravityPointMapper = mapperService.GetMapper<GravityPoint>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            gravityPointMapper.TryGet(entityId).IfSome(points.Add);
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var movable = movableMapper.Get(entityId);

            movable.GravityPull = points.Aggregate(Vector2.Zero, (current, p) =>
                current + (p.Position - movable.Position).NormalizedOrZero() * p.GravityMass /
                (p.Position - movable.Position).LengthSquared()) * movable.GravityMultiplier;
        }
    }
}
