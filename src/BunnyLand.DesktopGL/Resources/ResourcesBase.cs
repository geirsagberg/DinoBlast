using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;

namespace BunnyLand.DesktopGL.Resources;

public abstract class ResourcesBase<T>
{
    protected readonly ContentManager ContentManager;

    protected ResourcesBase(ContentManager contentManager)
    {
        ContentManager = contentManager;
    }

    public void Load()
    {
        foreach (var propertyInfo in GetType().GetProperties().Where(p => p.PropertyType == typeof(T))) {
            var file = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (file != null) {
                var texture = ContentManager.Load<T>(file);
                propertyInfo.SetValue(this, texture);
            }
        }
    }
}