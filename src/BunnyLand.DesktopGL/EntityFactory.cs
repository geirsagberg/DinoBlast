using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
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
        private readonly Variables variables;

        public EntityFactory(Textures textures, Variables variables)
        {
            this.textures = textures;
            this.variables = variables;
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

        public Entity CreatePlayer(Entity entity, Vector2 position)
        {
            var transform = new Transform2(position);
            entity.Attach(transform);
            var animatedSprite = GetPlayerSprite();
            entity.Attach(animatedSprite);
            entity.Attach(new CollisionBody(new CircleF(Point2.Zero, 15), transform, ColliderTypes.Player,
                ColliderTypes.Player | ColliderTypes.Projectile | ColliderTypes.Static));
            entity.Attach(new Player());
            entity.Attach(new Movable(transform));

            var emitter = new Emitter {
                EmitInterval = TimeSpan.FromSeconds(0.1)
            };
            entity.Attach(emitter);

            return entity;
        }

        public Entity CreatePlanet(Entity entity, Vector2 position, float mass, float scale = 1)
        {
            var transform = new Transform2(position, scale: new Vector2(scale));
            entity.Attach(transform);
            var sprite = new Sprite(textures.redplanet);
            entity.Attach(sprite);
            var boundingRectangle = sprite.GetBoundingRectangle(position, 0, new Vector2(scale));
            entity.Attach(new CollisionBody(new CircleF(Point2.Zero, boundingRectangle.Width / 2f), transform,
                ColliderTypes.Static, ColliderTypes.Player | ColliderTypes.Projectile));
            entity.Attach(new GravityPoint(transform, mass));
            return entity;
        }

        public Entity CreateLevel(Entity entity, float width, float height)
        {
            entity.Attach(new Level(new RectangleF(0, 0, width, height)));
            return entity;
        }

        public Entity CreateBlock(Entity entity, RectangleF rectangleF)
        {
            var transform = new Transform2(rectangleF.Position);
            entity.Attach(transform);
            entity.Attach(new CollisionBody(rectangleF, transform, ColliderTypes.Static,
                ColliderTypes.Player | ColliderTypes.Projectile));
            entity.Attach(new SolidColor(Color.LightCoral, rectangleF));
            return entity;
        }

        public Entity CreateBullet(Entity entity, Vector2 position, Vector2 velocity, TimeSpan totalGametime,
            TimeSpan lifeSpan)
        {
            var transform = new Transform2(position);
            entity.Attach(transform);
            var movable = new Movable(transform) {
                Velocity = velocity * variables.Global[GlobalVariable.BulletSpeed]
            };
            entity.Attach(movable);
            var collisionBody = new CollisionBody(new CircleF(Point2.Zero, 1), transform,
                ColliderTypes.Projectile, ColliderTypes.Player | ColliderTypes.Static);
            entity.Attach(collisionBody);
            var projectile = new Projectile(movable, collisionBody);
            entity.Attach(projectile);

            var sprite = new Sprite(textures.bullet);
            entity.Attach(sprite);
            var lifetime = new Lifetime(totalGametime, lifeSpan);
            entity.Attach(lifetime);
            return entity;
        }
    }
}
