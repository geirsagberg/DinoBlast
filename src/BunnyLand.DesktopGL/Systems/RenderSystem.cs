using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;

namespace BunnyLand.DesktopGL.Systems
{
    public class RenderSystem : EntityDrawSystem
    {
        private readonly SpriteBatch spriteBatch;
        private readonly SpriteFonts spriteFonts;
        private ComponentMapper<AnimatedSprite> animatedSpriteMapper;
        private ComponentMapper<CollisionBody> collisionMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Sprite> spriteMapper;
        private ComponentMapper<Transform2> transformMapper;

        public RenderSystem(SpriteBatch spriteBatch, SpriteFonts spriteFonts) : base(Aspect.All(typeof(Transform2))
            .One(typeof(AnimatedSprite), typeof(Sprite)))
        {
            this.spriteBatch = spriteBatch;
            this.spriteFonts = spriteFonts;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            animatedSpriteMapper = mapperService.GetMapper<AnimatedSprite>();
            spriteMapper = mapperService.GetMapper<Sprite>();
            collisionMapper = mapperService.GetMapper<CollisionBody>();
            movableMapper = mapperService.GetMapper<Movable>();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            var elapsedSeconds = gameTime.GetElapsedSeconds();

            spriteBatch.Begin();
            foreach (var entity in ActiveEntities) {
                var sprite = animatedSpriteMapper.GetOrNull(entity)
                    ?? spriteMapper.Get(entity);
                if (sprite is AnimatedSprite animatedSprite) {
                    animatedSprite.Update(elapsedSeconds);
                }

                var transform = transformMapper.Get(entity);

                spriteBatch.Draw(sprite, transform);

                var collisionBody = collisionMapper.Get(entity);
                if (collisionBody != null) {
                    if (collisionBody.Bounds is CircleF circle) {
                        spriteBatch.DrawCircle(circle, 32, Color.Aqua);
                    }

                    collisionBody.CollisionInfo.IfSome(info => {
                        spriteBatch.DrawLine(transform.WorldPosition, transform.WorldPosition + info.PenetrationVector,
                            Color.Aquamarine);
                    });
                }

                movableMapper.MaybeGet(entity).IfSome(movable => spriteBatch.DrawLine(transform.WorldPosition,
                    transform.WorldPosition + movable.GravityPull * 1000, Color.Azure));
            }

            spriteBatch.DrawString(spriteFonts.Verdana, "AWSD: Move, Space: Boost", Vector2.One, Color.White);

            spriteBatch.End();
        }
    }
}
