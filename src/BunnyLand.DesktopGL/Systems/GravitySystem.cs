using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class GravitySystem : EntityProcessingSystem
    {
        private readonly Variables variables;
        private readonly Bag<GravityPoint> points = new Bag<GravityPoint>();
        private ComponentMapper<GravityPoint> gravityPointMapper;
        private ComponentMapper<Movable> movableMapper;

        public GravitySystem(Variables variables) : base(Aspect.All(typeof(Movable)))
        {
            this.variables = variables;
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

        protected override void OnEntityRemoved(int entityId)
        {
            gravityPointMapper.TryGet(entityId).IfSome(point => points.Remove(point));
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var movable = movableMapper.Get(entityId);

            // Add up all the gravitational forces acting on the movable
            var resultingGravityPull = points.Aggregate(Vector2.Zero,
                (current, point) => current + CalculateGravityPull(point, movable));
            movable.GravityPull = resultingGravityPull * movable.GravityMultiplier * variables.Global[GlobalVariable.GravityMultiplier];
        }

        private static Vector2 CalculateGravityPull(GravityPoint point, Movable movable) =>
            (point.Position - movable.Position).NormalizedOrZero() * point.GravityMass /
            (point.Position - movable.Position).LengthSquared();
    }
}
