using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class LifetimeSystem : EntityProcessingSystem
    {
        private readonly Variables variables;
        private ComponentMapper<Lifetime> lifetimeMapper;

        public LifetimeSystem(Variables variables) : base(Aspect.All(typeof(Lifetime)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            lifetimeMapper = mapperService.GetMapper<Lifetime>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {

            var lifetime = lifetimeMapper.Get(entityId);
            lifetime.LifeSpanLeft -= gameTime.GetElapsedTimeSpan(variables);
            if (lifetime.LifeSpanLeft < TimeSpan.Zero) {
                DestroyEntity(entityId);
            }
        }
    }
}
