using System;
using BunnyLand.DesktopGL.Components;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL
{
    public class EntityFactory
    {
        public Entity CreatePlayer(Entity entity, Vector2 position, PlayerIndex playerIndex, bool isLocal = true, Option<int> peerId = default)
        {
            entity.Attach(new Serializable(entity.Id));
            var transform = new Transform2(position);
            entity.Attach(transform);

            var sprite = new SpriteInfo(SpriteType.Bunny, new Size(35, 50));
            entity.Attach(sprite);

            entity.Attach(new CollisionBody(new CircleF(Point2.Zero, 15), position, ColliderTypes.Player,
                ColliderTypes.Player | ColliderTypes.Projectile | ColliderTypes.Static));
            entity.Attach(new Player(playerIndex) { IsLocal = isLocal, PeerId = peerId});
            entity.Attach(new Movable());
            entity.Attach(new Health(100));

            var emitter = new Emitter {
                EmitInterval = TimeSpan.FromSeconds(0.1)
            };
            entity.Attach(emitter);

            return entity;
        }

        public Entity CreatePlanet(Entity entity, Vector2 position, float mass, float scale = 1)
        {
            entity.Attach(new Serializable(entity.Id));
            var transform = new Transform2(position, scale: new Vector2(scale));
            entity.Attach(transform);

            const int planetSize = 400;

            var sprite = new SpriteInfo(SpriteType.Planet1, new Size(planetSize, planetSize));
            entity.Attach(sprite);

            entity.Attach(new CollisionBody(new CircleF(Point2.Zero, planetSize * scale / 2f), position,
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
            entity.Attach(new CollisionBody(rectangleF, rectangleF.Position, ColliderTypes.Static,
                ColliderTypes.Player | ColliderTypes.Projectile));
            entity.Attach(new SolidColor(Color.LightCoral, rectangleF));
            return entity;
        }

        public Entity CreateBullet(Entity entity, Vector2 position, Vector2 velocity,
            TimeSpan lifeSpan)
        {
            entity.Attach(new Serializable(entity.Id));
            var transform = new Transform2(position);
            entity.Attach(transform);
            var movable = new Movable {
                Velocity = velocity,
                GravityMultiplier = 0.3f,
                WrapAround = false
            };
            entity.Attach(movable);
            var collisionBody = new CollisionBody(new CircleF(Point2.Zero, 1), position,
                ColliderTypes.Projectile, ColliderTypes.Player | ColliderTypes.Static);
            entity.Attach(collisionBody);

            var sprite = new SpriteInfo(SpriteType.Bullet, new Size(2, 2));
            entity.Attach(sprite);

            var lifetime = new Lifetime(lifeSpan);
            entity.Attach(lifetime);
            entity.Attach(new Damaging(30));
            return entity;
        }
    }
}
