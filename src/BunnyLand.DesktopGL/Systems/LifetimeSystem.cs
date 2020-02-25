using System;
using BunnyLand.DesktopGL.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class LifetimeSystem : EntityProcessingSystem
    {
        private ComponentMapper<Lifetime> lifetimeMapper;

        public LifetimeSystem() : base(Aspect.All(typeof(Lifetime)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            lifetimeMapper = mapperService.GetMapper<Lifetime>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var lifetime = lifetimeMapper.Get(entityId);
            if (lifetime.CreatedAt + lifetime.LifeSpan < gameTime.TotalGameTime) {
                DestroyEntity(entityId);
            }
        }
    }
}
