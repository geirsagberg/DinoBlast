using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL
{
    public class BunnyWorldBuilder
    {
        private readonly IServiceProvider serviceProvider;

        public BunnyWorldBuilder(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly List<ISystem> _systems = new List<ISystem>();

        public BunnyWorldBuilder AddSystem(ISystem system)
        {
            _systems.Add(system);
            return this;
        }

        public BunnyWorld Build()
        {
            var world = serviceProvider.GetRequiredService<BunnyWorld>();

            foreach (var system in _systems)
                world.RegisterSystem(system);

            return world;
        }
    }
}
