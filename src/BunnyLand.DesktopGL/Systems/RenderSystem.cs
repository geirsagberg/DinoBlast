using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Resources;
using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace BunnyLand.DesktopGL.Systems;

public class RenderSystem : EntityDrawSystem
{
    private readonly BitmapFont font;

    private readonly ConcurrentDictionary<int, Sprite> spriteByEntity = new ConcurrentDictionary<int, Sprite>();

    private readonly LinkedList<int> fpsList = new LinkedList<int>();

    private readonly OrthographicCamera camera;
    private readonly SharedContext sharedContext;
    private readonly SpriteBatch spriteBatch;
    private readonly Textures textures;
    private readonly Variables variables;
    private ComponentMapper<CollisionBody> collisionMapper = null!;

    private ComponentMapper<Health> healthMapper = null!;
    private ComponentMapper<Level> levelMapper = null!;
    private ComponentMapper<Movable> movableMapper = null!;
    private ComponentMapper<PlayerInput> inputMapper = null!;
    private ComponentMapper<PlayerState> playerMapper = null!;
    private ComponentMapper<SolidColor> solidColorMapper = null!;
    private ComponentMapper<SpriteInfo> spriteMapper = null!;
    private ComponentMapper<Transform2> transformMapper = null!;

    public Option<Level> Level { get; set; }

    private GraphicsDevice GraphicsDevice => spriteBatch.GraphicsDevice;

    public Option<PlayerState> Player { get; set; }

    public RenderSystem(SpriteBatch spriteBatch, ContentManager contentManager, Variables variables, Textures textures, SharedContext sharedContext,
        KeyboardListener keyboardListener, OrthographicCamera camera) : base(
        Aspect
            .All(typeof(Transform2))
            .One(typeof(SpriteInfo), typeof(SolidColor)))
    {
        this.spriteBatch = spriteBatch;
        this.variables = variables;
        this.textures = textures;
        this.sharedContext = sharedContext;
        this.camera = camera;
        font = contentManager.Load<BitmapFont>("Fonts/bryndan-medium");

        keyboardListener.KeyPressed += (sender, args) => {
            if (args.Key == Keys.F12) {
                sharedContext.ShowDebugInfo = !sharedContext.ShowDebugInfo;
            }
        };
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        transformMapper = mapperService.GetMapper<Transform2>();
        spriteMapper = mapperService.GetMapper<SpriteInfo>();
        collisionMapper = mapperService.GetMapper<CollisionBody>();
        movableMapper = mapperService.GetMapper<Movable>();
        levelMapper = mapperService.GetMapper<Level>();
        playerMapper = mapperService.GetMapper<PlayerState>();
        solidColorMapper = mapperService.GetMapper<SolidColor>();
        healthMapper = mapperService.GetMapper<Health>();
        inputMapper = mapperService.GetMapper<PlayerInput>();
    }

    protected override void OnEntityAdded(int entityId)
    {
        levelMapper.TryGet(entityId).IfSome(level => Level = level);
        playerMapper.TryGet(entityId).IfSome(player => Player = player);
    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.GraphicsDevice.Clear(Color.Black);
        var elapsedSeconds = gameTime.GetElapsedSeconds();

        spriteBatch.Begin(transformMatrix: camera.GetViewMatrix());
        foreach (var entity in ActiveEntities) {
            DrawSprite(entity, elapsedSeconds);
            transformMapper.TryGet(entity).IfSome(transform => DrawAiming(entity, transform));

            DrawEntityDebugInfo(entity);
        }

        DrawGlobalDebugInfo(gameTime);

        spriteBatch.End();
    }

    private void DrawSprite(int entity, float elapsedSeconds)
    {
        spriteMapper.TryGet(entity).IfSome(spriteInfo => {
            var sprite = spriteByEntity.GetOrAdd(entity, (id, si) => CreateSprite(si), spriteInfo);
            if (sprite is AnimatedSprite animatedSprite) {
                animatedSprite.Update(elapsedSeconds);
                animatedSprite.Play("running");
            }

            RenderSprite(entity, sprite);
        });
    }

    private void DrawGlobalDebugInfo(GameTime gameTime)
    {
        if (!sharedContext.ShowDebugInfo) return;

        spriteBatch.DrawString(font, "AWSD: Move, Space: Boost, Shift: Toggle Brake/Glide, Ctrl: Shoot",
            Vector2.One, Color.White);
        Player.IfSome(player => spriteBatch.DrawString(font,
            "Brakes: " + (player.IsBraking ? "On" : "Off"), new Vector2(1, 30), Color.White));

        var smoothedFps = GetSmoothedFps(gameTime);

        spriteBatch.DrawString(font, $"FPS: {smoothedFps}", new Vector2(1, 60),
            Color.White);

        spriteBatch.DrawString(font, $"IsRunningSlowly: {gameTime.IsRunningSlowly}", new Vector2(1, 90),
            Color.White);

        spriteBatch.DrawString(font, $"CurrentFrame: {sharedContext.FrameCounter}", new Vector2(1, 120), Color.White);
    }

    private void DrawEntityDebugInfo(int entity)
    {
        if (!sharedContext.ShowDebugInfo) return;

        solidColorMapper.TryGet(entity).IfSome(solidColor => { spriteBatch.DrawRectangle(solidColor.Bounds, solidColor.Color); });
        transformMapper.TryGet(entity).IfSome(transform => {
            DrawCollisionBoundsAndInfo(entity, transform);
            DrawGravityPull(entity, transform);
        });
    }

    private void DrawAiming(int entity, Transform2 transform)
    {
        inputMapper.TryGet(entity).IfSome(playerInput => {
            spriteBatch.DrawLine(transform.Position,
                transform.Position + playerInput.DirectionalInputs.AimDirection * 100, Color.White);
        });
    }

    private void RenderSprite(int entity, Sprite sprite)
    {
        var transform = transformMapper.Get(entity);

        // playerMapper.TryGet(entity).IfSome(player => {
        //     sprite.Color = player.PlayerNumber switch {
        //         1 => new Color(128, 128, 255),
        //         2 => new Color(255, 128, 128),
        //         3 => Color.Yellow,
        //         4 => Color.Green,
        //         _ => Color.White
        //     };
        //     healthMapper.TryGet(entity).IfSome(health => { transform.Scale = Vector2.One * health.CurrentHealth / health.MaxHealth; });
        // });

        spriteBatch.Draw(sprite, transform);
        // spriteBatch.DrawRectangle(sprite.GetBoundingRectangle(transform), Color.Beige);

        movableMapper.TryGet(entity).IfSome(movable => {
            if (movable.LevelBoundsBehavior == LevelBoundsBehavior.Wrap) {
                DrawLevelWrapping(sprite, transform);
            }
        });
    }

    protected override void OnEntityRemoved(int entityId)
    {
        spriteByEntity.Remove(entityId, out _);
    }

    private Sprite CreateSprite(SpriteInfo spriteInfo) => spriteInfo.SpriteType switch {
        SpriteType.Bunny => GetPlayerSprite(),
        SpriteType.Anki => new Sprite(textures.miniAnki),
        SpriteType.Planet1 => new Sprite(textures.redplanet),
        SpriteType.Bullet => new Sprite(textures.bullet),
        SpriteType.Dino1 => GetDinoSprite(),
        _ => throw new ArgumentException("Unknown spriteType")
    };

    private AnimatedSprite GetDinoSprite()
    {
        var atlas = TextureAtlas.Create("dino", textures.dino1, 127, 90);
        var spritesheet = new SpriteSheet {
            TextureAtlas = atlas
        };
        spritesheet.Cycles.Add("idle", new SpriteSheetAnimationCycle {
            Frames = new List<SpriteSheetAnimationFrame> {
                new SpriteSheetAnimationFrame(0)
            }
        });
        spritesheet.Cycles.Add("running", new SpriteSheetAnimationCycle {
            Frames = Enumerable.Range(0, 12).Select(i => new SpriteSheetAnimationFrame(i)).ToList(),
            FrameDuration = 1f / 12
        });
        var animatedSprite = new AnimatedSprite(spritesheet);
        return animatedSprite;
    }

    private AnimatedSprite GetPlayerSprite()
    {
        var atlas = TextureAtlas.Create("bunny", textures.PlayerAnimation, 35, 50);
        var spriteSheet = new SpriteSheet {
            TextureAtlas = atlas
        };
        spriteSheet.Cycles.Add("idle", new SpriteSheetAnimationCycle {
            Frames = new List<SpriteSheetAnimationFrame> {
                new SpriteSheetAnimationFrame(0)
            }
        });
        spriteSheet.Cycles.Add("running",
            new SpriteSheetAnimationCycle {
                Frames = Enumerable.Range(1, 8).Select(i => new SpriteSheetAnimationFrame(i)).ToList(),
            });
        var animatedSprite = new AnimatedSprite(spriteSheet);
        return animatedSprite;
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

            var overlappingTop = bounds.Top < 0;
            var overlappingBottom = bounds.Bottom >= level.Bounds.Bottom;

            if (overlappingTop) {
                spriteBatch.Draw(sprite, transform.Position + level.Bounds.HeightVector(),
                    transform.Rotation, transform.Scale);
            } else if (overlappingBottom) {
                spriteBatch.Draw(sprite, transform.Position - level.Bounds.HeightVector(),
                    transform.Rotation, transform.Scale);
            }

            var overlappingLeft = bounds.Left < 0;
            var overlappingRight = bounds.Right >= level.Bounds.Right;

            if (overlappingLeft) {
                spriteBatch.Draw(sprite, transform.Position + level.Bounds.WidthVector(),
                    transform.Rotation, transform.Scale);
            } else if (overlappingRight) {
                spriteBatch.Draw(sprite, transform.Position - level.Bounds.WidthVector(),
                    transform.Rotation, transform.Scale);
            }

            if (overlappingTop && overlappingLeft) {
                spriteBatch.Draw(sprite, transform.Position + level.Bounds.BottomRight,
                    transform.Rotation, transform.Scale);
            } else if (overlappingTop && overlappingRight) {
                spriteBatch.Draw(sprite,
                    transform.Position + level.Bounds.HeightVector() - level.Bounds.WidthVector(),
                    transform.Rotation, transform.Scale);
            } else if (overlappingBottom && overlappingLeft) {
                spriteBatch.Draw(sprite,
                    transform.Position + level.Bounds.WidthVector() - level.Bounds.HeightVector(),
                    transform.Rotation, transform.Scale);
            } else if (overlappingBottom && overlappingRight) {
                spriteBatch.Draw(sprite, transform.Position - (Vector2) level.Bounds.BottomRight,
                    transform.Rotation, transform.Scale);
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
            transform.WorldPosition + movable.GravityPull * variables.Global[GlobalVariable.DebugVectorMultiplier],
            Color.Azure));
    }
}