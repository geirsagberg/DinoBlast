using System;
using Microsoft.Extensions.DependencyInjection;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Extensions
{
    public class WorldBuilderService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly BunnyWorldBuilder worldBuilder;

        public WorldBuilderService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            worldBuilder = serviceProvider.GetRequiredService<BunnyWorldBuilder>();
        }

        public WorldBuilderService AddSystemService<T>() where T : ISystem
        {
            var service = serviceProvider.GetRequiredService<T>();
            worldBuilder.AddSystem(service);
            return this;
        }

        public World Build() => worldBuilder.Build();
    }
}
