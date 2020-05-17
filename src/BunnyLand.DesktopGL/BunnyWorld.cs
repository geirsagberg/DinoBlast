using BunnyLand.DesktopGL.Models;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL
{
    public class BunnyWorld : World
    {
        private readonly SharedContext sharedContext;

        protected internal BunnyWorld(SharedContext sharedContext)
        {
            this.sharedContext = sharedContext;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var system in UpdateSystems) {
                if (system is IPausable && sharedContext.IsPaused)
                    continue;

                system.Update(gameTime);
            }

            // TODO: Can these be switched? Make sure systems do not evaluate same FrameCounter twice
            if (!sharedContext.IsPaused)
                sharedContext.FrameCounter++;
            if (sharedContext.IsPaused && sharedContext.ResumeAtGameTime < gameTime.TotalGameTime)
                sharedContext.IsPaused = false;
        }
    }
}
