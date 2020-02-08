using System;
using LanguageExt;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class MonoGameExtendedExtensions
    {
        public static Option<T> MaybeGet<T>(this ComponentMapper<T> mapper, int entityId) where T : class
        {
            return mapper.Has(entityId) ? Option<T>.Some(mapper.Get(entityId)) : Option<T>.None;
        }

        public static T? GetOrNull<T>(this ComponentMapper<T> mapper, int entityId) where T : class
        {
            return mapper.Has(entityId) ? mapper.Get(entityId) : null;
        }

        public static WorldBuilderService BuildWorld(this IServiceProvider serviceProvider) =>
            new WorldBuilderService(serviceProvider);
    }
}
