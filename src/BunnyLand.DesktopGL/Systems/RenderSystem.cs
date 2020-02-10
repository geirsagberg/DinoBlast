using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
using LanguageExt;
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
        private ComponentMapper<Level> levelMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Player> playerMapper;
        private ComponentMapper<Sprite> spriteMapper;
        private ComponentMapper<Transform2> transformMapper;

        public Option<Level> Level { get; set; }

        private GraphicsDevice GraphicsDevice => spriteBatch.GraphicsDevice;

        public Option<Player> Player { get; set; }

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
            levelMapper = mapperService.GetMapper<Level>();
            playerMapper = mapperService.GetMapper<Player>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            levelMapper.TryGet(entityId).IfSome(level => Level = level);
            playerMapper.TryGet(entityId).IfSome(player => Player = player);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            var elapsedSeconds = gameTime.GetElapsedSeconds();

            spriteBatch.Begin();
            foreach (var entity in ActiveEntities) {
                var sprite = animatedSpriteMapper.Get(entity) ?? spriteMapper.Get(entity);
                if (sprite is AnimatedSprite animatedSprite) {
                    animatedSprite.Update(elapsedSeconds);
                }

                var transform = transformMapper.Get(entity);

                spriteBatch.Draw(sprite, transform);
                // spriteBatch.DrawRectangle(sprite.GetBoundingRectangle(transform), Color.Beige);

                DrawLevelWrapping(sprite, transform);
                DrawCollisionBoundsAndInfo(entity, transform);
                DrawGravityPull(entity, transform);
            }

            spriteBatch.DrawString(spriteFonts.Verdana, "AWSD: Move, Space: Boost, Shift: Toggle Brake/Glide",
                Vector2.One, Color.White);
            Player.IfSome(player => spriteBatch.DrawString(spriteFonts.Verdana,
                "Brakes: " + (player.IsBraking ? "On" : "Off"), new Vector2(1, 30), Color.White));


            spriteBatch.End();
        }

        private void DrawLevelWrapping(Sprite sprite, Transform2 transform)
        {
            Level.IfSome(level => {
                var bounds = sprite.GetBoundingRectangle(transform);

                if (level.Bounds.Contains(bounds.TopLeft) ^ level.Bounds.Contains(bounds.BottomRight)) {
                    if (bounds.Top < 0) {
                        spriteBatch.Draw(sprite, transform.Position + level.Bounds.HeightVector(),
                            transform.Rotation, transform.Scale);
                    } else if (bounds.Bottom >= level.Bounds.Bottom) {
                        spriteBatch.Draw(sprite, transform.Position - level.Bounds.HeightVector(),
                            transform.Rotation, transform.Scale);
                    }

                    if (bounds.Left < 0) {
                        spriteBatch.Draw(sprite, transform.Position + level.Bounds.WidthVector(),
                            transform.Rotation, transform.Scale);
                    } else if (bounds.Right >= level.Bounds.Right) {
                        spriteBatch.Draw(sprite, transform.Position - level.Bounds.WidthVector(),
                            transform.Rotation, transform.Scale);
                    }
                }
            });
        }

        private void DrawCollisionBoundsAndInfo(int entity, Transform2 transform)
        {
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
        }

        private void DrawGravityPull(int entity, Transform2 transform)
        {
            movableMapper.TryGet(entity).IfSome(movable => spriteBatch.DrawLine(transform.WorldPosition,
                transform.WorldPosition + movable.GravityPull * 1000, Color.Azure));
        }
    }
}
