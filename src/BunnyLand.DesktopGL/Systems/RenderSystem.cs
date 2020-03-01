using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;

namespace BunnyLand.DesktopGL.Systems
{
    public class RenderSystem : EntityDrawSystem
    {
        private readonly BitmapFont font;

        private readonly LinkedList<int> fpsList = new LinkedList<int>();
        private readonly SpriteBatch spriteBatch;
        private readonly Variables variables;
        private ComponentMapper<AnimatedSprite> animatedSpriteMapper;
        private ComponentMapper<CollisionBody> collisionMapper;
        private ComponentMapper<Level> levelMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Player> playerMapper;
        private ComponentMapper<SolidColor> solidColorMapper;
        private ComponentMapper<Sprite> spriteMapper;
        private ComponentMapper<Transform2> transformMapper;

        public Option<Level> Level { get; set; }

        private GraphicsDevice GraphicsDevice => spriteBatch.GraphicsDevice;

        public Option<Player> Player { get; set; }

        public RenderSystem(SpriteBatch spriteBatch, ContentManager contentManager, Variables variables) : base(Aspect
            .All(typeof(Transform2))
            .One(typeof(AnimatedSprite), typeof(Sprite), typeof(SolidColor)))
        {
            this.spriteBatch = spriteBatch;
            this.variables = variables;
            font = contentManager.Load<BitmapFont>("Fonts/bryndan-medium");
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
            solidColorMapper = mapperService.GetMapper<SolidColor>();
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
                animatedSpriteMapper.TryGet(entity).IfSome(animatedSprite => {
                    animatedSprite.Update(elapsedSeconds);
                    RenderSprite(entity, animatedSprite);
                });
                spriteMapper.TryGet(entity).IfSome(sprite => RenderSprite(entity, sprite));
                solidColorMapper.TryGet(entity).IfSome(solidColor => {
                    spriteBatch.DrawRectangle(solidColor.Bounds, solidColor.Color);
                });
                transformMapper.TryGet(entity).IfSome(transform => {
                    DrawCollisionBoundsAndInfo(entity, transform);
                    DrawGravityPull(entity, transform);
                });
            }

            spriteBatch.DrawString(font, "AWSD: Move, Space: Boost, Shift: Toggle Brake/Glide, Ctrl: Shoot",
                Vector2.One, Color.White);
            Player.IfSome(player => spriteBatch.DrawString(font,
                "Brakes: " + (player.IsBraking ? "On" : "Off"), new Vector2(1, 30), Color.White));

            var smoothedFps = GetSmoothedFps(gameTime);

            spriteBatch.DrawString(font, $"FPS: {smoothedFps}", new Vector2(1, 60),
                Color.White);

            spriteBatch.DrawString(font, $"IsRunningSlowly: {gameTime.IsRunningSlowly}", new Vector2(1, 90), Color.White);

            spriteBatch.End();
        }

        private void RenderSprite(int entity, Sprite sprite)
        {
            var transform = transformMapper.Get(entity);

            spriteBatch.Draw(sprite, transform);
            // spriteBatch.DrawRectangle(sprite.GetBoundingRectangle(transform), Color.Beige);

            DrawLevelWrapping(sprite, transform);
        }

        private int GetSmoothedFps(GameTime gameTime)
        {
            var fps = (int) (Math.Abs(gameTime.ElapsedGameTime.TotalSeconds) < 0.0000001
                ? 0
                : 1 / gameTime.ElapsedGameTime.TotalSeconds);

            fpsList.AddLast(fps);
            if (fpsList.Count > 3)
                fpsList.RemoveFirst();

            var smoothedFps = fpsList.Sum() / fpsList.Count;
            return smoothedFps;
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
            collisionMapper.TryGet(entity).IfSome(body => {
                var color = body.Collisions.Any() ? Color.Red : Color.Aqua;
                if (body.Bounds is CircleF circle) {
                    spriteBatch.DrawCircle(circle, 32, color);
                } else if (body.Bounds is RectangleF rectangle) {
                    spriteBatch.DrawRectangle(rectangle, color);
                }

                if (body.CollisionBounds != default) {
                    spriteBatch.DrawRectangle(body.CollisionBounds, Color.Chocolate);
                }

                foreach (var collision in body.Collisions) {
                    spriteBatch.DrawLine(transform.WorldPosition, transform.WorldPosition + collision.penetrationVector,
                        Color.Aquamarine);
                }

                // body.CollisionInfo.IfSome(info => {
                //     spriteBatch.DrawLine(transform.WorldPosition, transform.WorldPosition + info.PenetrationVector,
                //         Color.Aquamarine);
                // });
            });
        }

        private void DrawGravityPull(int entity, Transform2 transform)
        {
            movableMapper.TryGet(entity).IfSome(movable => spriteBatch.DrawLine(transform.WorldPosition,
                transform.WorldPosition + movable.GravityPull * variables.Global[GlobalVariable.DebugVectorMultiplier], Color.Azure));
        }
    }
}
