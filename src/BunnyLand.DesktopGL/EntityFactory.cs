using System;
using System.Linq;
using BunnyLand.DesktopGL.Resources;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Entities;
using MonoGame.Extended.TextureAtlases;

namespace BunnyLand.DesktopGL
{
    public class EntityFactory
    {
        private readonly Textures textures;
        private readonly World world;

        public EntityFactory(World world, Textures textures)
        {
            this.world = world;
            this.textures = textures;
        }

        public Entity CreatePlayer()
        {
            var entity = world.CreateEntity();
            entity.Attach(new Transform2());
            var atlas = TextureAtlas.Create("bunny", textures.PlayerAnimation, 35, 50);
            var animationFactory = new SpriteSheetAnimationFactory(atlas);
            animationFactory.Add("idle", DefineSpriteAnimation(..0));
            animationFactory.Add("running", DefineSpriteAnimation(1..9));
            entity.Attach(new AnimatedSprite(animationFactory, "idle"));

            return entity;
        }

        public static SpriteSheetAnimationData DefineSpriteAnimation(Range range, float frameDuration = 0.2f,
            bool isLooping = true, bool isReversed = false, bool isPingPong = false) =>
            new SpriteSheetAnimationData(Enumerable.Range(range.Start.Value,
                    Math.Abs(range.End.Value - range.Start.Value)).ToArray(), frameDuration, isLooping, isReversed,
                isPingPong);
    }
}
