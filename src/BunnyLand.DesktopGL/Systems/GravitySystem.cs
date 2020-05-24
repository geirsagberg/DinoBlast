using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class GravitySystem : EntityProcessingSystem, IPausable
    {
        private readonly HashSet<int> gravityPointEntities = new HashSet<int>();
        private readonly SharedContext sharedContext;
        private readonly Variables variables;
        private ComponentMapper<GravityPoint> gravityPointMapper = null!;
        private ComponentMapper<Movable> movableMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;

        public GravitySystem(Variables variables, SharedContext sharedContext) : base(Aspect.All(typeof(Movable), typeof(Transform2)))
        {

            this.variables = variables;
            this.sharedContext = sharedContext;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            movableMapper = mapperService.GetMapper<Movable>();
            gravityPointMapper = mapperService.GetMapper<GravityPoint>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            if (gravityPointMapper.Has(entityId) && transformMapper.Has(entityId))
                gravityPointEntities.Add(entityId);
        }

        protected override void OnEntityRemoved(int entityId)
        {
            gravityPointEntities.Remove(entityId);
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var movable = movableMapper.Get(entityId);
            var transform = transformMapper.Get(entityId);

            // Add up all the gravitational forces acting on the movable
            var resultingGravityPull = gravityPointEntities.Aggregate(Vector2.Zero,
                (current, point) => current + CalculateGravityPull(point, transform));
            movable.GravityPull = resultingGravityPull * movable.GravityMultiplier * variables.Global[GlobalVariable.GravityMultiplier];
        }

        private Vector2 CalculateGravityPull(int point, Transform2 transform)
        {
            var distance = transformMapper.Get(point).Position - transform.Position;
            var gravityMass = gravityPointMapper.Get(point).GravityMass;
            return distance.NormalizedOrZero() * gravityMass / distance.LengthSquared();
        }
    }
}
