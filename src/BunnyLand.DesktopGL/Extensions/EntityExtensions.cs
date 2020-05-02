using LanguageExt;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class EntityExtensions
    {
        public static Option<T> TryGet<T>(this Entity entity) where T : class => entity.Get<T>() is {} existing ? existing : Option<T>.None;
    }
}
