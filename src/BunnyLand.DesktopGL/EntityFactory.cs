using System;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Sprites;
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

        private AnimatedSprite GetPlayerSprite()
        {
            var atlas = TextureAtlas.Create("bunny", textures.PlayerAnimation, 35, 50);
            var animationFactory = new SpriteSheetAnimationFactory(atlas);
            animationFactory.Add("idle", ..0);
            animationFactory.Add("running", 1..9);
            var animatedSprite = new AnimatedSprite(animationFactory, "idle");
            return animatedSprite;
        }

        public Entity CreatePlayer(Vector2 vector2)
        {
            var entity = world.CreateEntity();
            entity.Attach(new Transform2(vector2));
            entity.Attach(GetPlayerSprite());
            entity.Attach(new CollisionBody(new Size2(20, 30)));
            return entity;
        }

        public Entity CreatePlanet(Vector2 position)
        {
            var entity = world.CreateEntity();
            entity.Attach(new Transform2(position));
            entity.Attach(new Sprite(textures.redplanet));
            entity.Attach(new CollisionBody());
            return entity;
        }
    }
}
