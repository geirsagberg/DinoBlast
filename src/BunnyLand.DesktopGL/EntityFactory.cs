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
using tainicom.Aether.Physics2D.Dynamics;
using World = MonoGame.Extended.Entities.World;
using AWorld = tainicom.Aether.Physics2D.Dynamics.World;

namespace BunnyLand.DesktopGL
{
    public class EntityFactory
    {
        private readonly Textures textures;
        private readonly World world;
        private readonly AWorld physicsWorld;

        public EntityFactory(World world, Textures textures, AWorld physicsWorld)
        {
            this.world = world;
            this.textures = textures;
            this.physicsWorld = physicsWorld;
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

        public Entity CreatePlayer(Vector2 position)
        {
            var entity = world.CreateEntity();
            var transform = new Transform2(position);
            entity.Attach(transform);
            var animatedSprite = GetPlayerSprite();
            entity.Attach(animatedSprite);
            entity.Attach(new CollisionBody(new CircleF(Point2.Zero, 15), transform, ColliderTypes.Player,
                ColliderTypes.Player | ColliderTypes.Projectile | ColliderTypes.Static));
            entity.Attach(new Player());
            entity.Attach(new Movable(transform));

            var aetherBody = physicsWorld.CreateCircle(15, 1f,
                new Vector2(position.X, position.Y), BodyType.Dynamic);
            aetherBody.SetRestitution(1f);
            // aetherBody.LinearDamping = 0.1f;
            entity.Attach(aetherBody);

            return entity;
        }

        public Entity CreatePlanet(Vector2 position, float mass, float scale = 1)
        {
            var entity = world.CreateEntity();
            var transform = new Transform2(position, scale: new Vector2(scale));
            entity.Attach(transform);
            var sprite = new Sprite(textures.redplanet);
            entity.Attach(sprite);
            var boundingRectangle = sprite.GetBoundingRectangle(position, 0, new Vector2(scale));
            var radius = boundingRectangle.Width / 2f;
            entity.Attach(new CollisionBody(new CircleF(Point2.Zero, radius), transform,
                ColliderTypes.Static, ColliderTypes.Player | ColliderTypes.Projectile));
            entity.Attach(new GravityPoint(transform, mass));

            var aetherBody = physicsWorld.CreateCircle(radius * scale, 1, position);
            entity.Attach(aetherBody);

            return entity;
        }

        public Entity CreateLevel(float width, float height)
        {
            var entity = world.CreateEntity();
            entity.Attach(new Level(new RectangleF(0, 0, width, height)));
            return entity;
        }

        public Entity CreateBlock(RectangleF rectangleF)
        {
            var entity = world.CreateEntity();
            var transform = new Transform2(rectangleF.Position);
            entity.Attach(transform);
            entity.Attach(new CollisionBody(rectangleF, transform, ColliderTypes.Static,
                ColliderTypes.Player | ColliderTypes.Projectile));
            entity.Attach(new SolidColor(Color.LightCoral, rectangleF));
            return entity;
        }
    }
}
