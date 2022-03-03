using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;

namespace BunnyLand.DesktopGL.Resources;

public class ResourceLoader
{
    private readonly ContentManager contentManager;

    public ResourceLoader(ContentManager contentManager)
    {
        this.contentManager = contentManager;
    }

    public void Load<T>(object obj)
    {
        foreach (var propertyInfo in obj.GetType().GetProperties().Where(p => p.PropertyType == typeof(T))) {
            var file = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (file != null) {
                var texture = contentManager.Load<T>(file);
                propertyInfo.SetValue(obj, texture);
            }
        }
    }
}