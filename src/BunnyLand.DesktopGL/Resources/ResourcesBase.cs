using System;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace BunnyLand.DesktopGL.Resources
{
    public abstract class ResourcesBase : IDisposable
    {
        protected readonly ContentManager ContentManager;

        protected ResourcesBase(ContentManager contentManager)
        {
            ContentManager = contentManager;
        }

        public void Dispose()
        {
            foreach (IDisposable? disposable in GetType().GetProperties()
                .Where(p => typeof(IDisposable).IsAssignableFrom(p.PropertyType)).Select(p => p.GetValue(this))) {
                disposable?.Dispose();
            }
        }

        public abstract void Load();
    }
}
