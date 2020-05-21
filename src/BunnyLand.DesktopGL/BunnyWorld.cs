using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL
{
    public class BunnyWorld : World
    {
        private readonly SharedContext sharedContext;
        private readonly DebugLogger debugLogger;

        public BunnyWorld(SharedContext sharedContext, DebugLogger debugLogger)
        {
            this.sharedContext = sharedContext;
            this.debugLogger = debugLogger;
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

            debugLogger.Flush();
        }
    }
}
