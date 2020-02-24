using System;
using System.Linq;
using BunnyLand.DesktopGL.Enums;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL.Extensions
{
    public static class MonoGameExtendedExtensions
    {
        public static float GetElapsedTicks(this GameTime gameTime, Variables variables) =>
            gameTime.GetElapsedSeconds() * variables.Global[GlobalVariable.GameSpeed] * 60f;

        public static Option<T> TryGet<T>(this ComponentMapper<T> mapper, int entityId) where T : class
        {
            return mapper.Has(entityId) ? Option<T>.Some(mapper.Get(entityId)) : Option<T>.None;
        }

        public static T? GetOrNull<T>(this ComponentMapper<T> mapper, int entityId) where T : class
        {
            return mapper.Has(entityId) ? mapper.Get(entityId) : null;
        }

        public static WorldBuilderService CreateWorld(this IServiceProvider serviceProvider) =>
            new WorldBuilderService(serviceProvider);

        public static T RegisterGameComponent<T>(this IServiceProvider serviceProvider) where T : IGameComponent
        {
            var component = serviceProvider.GetRequiredService<T>();
            var components = serviceProvider.GetRequiredService<Game>().Components;
            if (!components.Contains(component))
                components.Add(component);
            return component;
        }

        public static void Add(this SpriteSheetAnimationFactory animationFactory, string name, Range range,
            float frameDuration = 0.2f,
            bool isLooping = true, bool isReversed = false, bool isPingPong = false) => animationFactory.Add(name,
            DefineSpriteAnimation(range, frameDuration, isLooping, isReversed, isPingPong));

        public static SpriteSheetAnimationData DefineSpriteAnimation(Range range, float frameDuration = 0.2f,
            bool isLooping = true, bool isReversed = false, bool isPingPong = false) =>
            new SpriteSheetAnimationData(Enumerable.Range(range.Start.Value,
                    Math.Abs(range.End.Value - range.Start.Value)).ToArray(), frameDuration, isLooping, isReversed,
                isPingPong);

        public static void AddRange<T>(this Bag<T> bag, params T[] items)
        {
            foreach (var item in items) {
                bag.Add(item);
            }
        }
    }
}
