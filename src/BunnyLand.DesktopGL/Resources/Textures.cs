using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.DesktopGL.Resources
{
    public partial class Textures : ResourcesBase
    {
        public Textures(ContentManager contentManager) : base(contentManager)
        {
        }

        public override void Load()
        {
            foreach (var propertyInfo in GetType().GetProperties().Where(p => p.PropertyType == typeof(Texture2D))) {
                var file = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (file != null) {
                    var texture = ContentManager.Load<Texture2D>(file);
                    propertyInfo.SetValue(this, texture);
                }
            }
        }
    }
}
