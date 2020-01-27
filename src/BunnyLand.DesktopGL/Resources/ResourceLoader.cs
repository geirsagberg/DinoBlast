using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.DesktopGL.Resources
{
    public class ResourceLoader : IDisposable
    {
        private readonly ContentManager contentManager;

        public ResourceLoader(ContentManager contentManager)
        {
            this.contentManager = contentManager;
        }

        public Textures LoadTextures()
        {
            return new Textures {
                BlackHole = contentManager.Load<Texture2D>("blackHole")
            };
        }

        public void Dispose()
        {
            contentManager.Dispose();
        }
    }

    public class Textures
    {


        public Texture2D BlackHole { get; set; }
    }
}
