using System.Collections.Generic;
using BunnyLand.DesktopGL.Models;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL
{
    public class BunnyWorldBuilder
    {
        private readonly SharedContext sharedContext;

        public BunnyWorldBuilder(SharedContext sharedContext)
        {
            this.sharedContext = sharedContext;
        }

        private readonly List<ISystem> _systems = new List<ISystem>();

        public BunnyWorldBuilder AddSystem(ISystem system)
        {
            _systems.Add(system);
            return this;
        }

        public BunnyWorld Build()
        {
            var world = new BunnyWorld(sharedContext);

            foreach (var system in _systems)
                world.RegisterSystem(system);

            return world;
        }
    }
}