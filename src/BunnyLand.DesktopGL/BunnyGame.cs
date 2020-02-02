using System;
using System.Diagnostics;
using BunnyLand.DesktopGL.Resources;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace BunnyLand.DesktopGL
{
    public class BunnyGame : Core
    {
        public BunnyGame(GameSettings gameSettings) : base(gameSettings.Width, gameSettings.Height,
            gameSettings.FullScreen, windowTitle: "Bunnyland")
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            var scene = Scene.CreateWithDefaultRenderer(Color.CornflowerBlue);

            var textures = new Textures(scene.Content);
            var sw = Stopwatch.StartNew();
            textures.Load();
            Console.WriteLine("Loaded textures in: " + sw.Elapsed);

            var blackHole = scene.CreateEntity("blackHole");
            blackHole.AddComponent(new SpriteRenderer(textures.blackhole));
            blackHole.Transform.Position = Screen.Center;

            Scene = scene;
        }

        // private readonly GraphicsDeviceManager graphics;
        // private readonly GameSettings settings;
        // private Vector2 blackHolePosition;
        // private float blackHoleRotation;
        // private SpriteBatch spriteBatch = null!;
        //
        // public BunnyGame(GameSettings settings)
        // {
        //     this.settings = settings;
        //     graphics = new GraphicsDeviceManager(this);
        //     Content.RootDirectory = "Content";
        //     IsMouseVisible = true;
        //     Textures = new Textures(Content);
        //     SpriteFonts = new SpriteFonts(Content);
        //     SoundEffects = new SoundEffects(Content);
        //     Songs = new Songs(Content);
        // }
        //
        // private Textures Textures { get; }
        // private SpriteFonts SpriteFonts { get; }
        // private SoundEffects SoundEffects { get; }
        // private Songs Songs { get; }
        //
        // protected override void Initialize()
        // {
        //
        //     // TODO: Add your initialization logic here
        //     blackHolePosition = new Vector2(graphics.PreferredBackBufferWidth / 2f,
        //         graphics.PreferredBackBufferHeight / 2f);
        //
        //     base.Initialize();
        // }
        //
        // protected override void LoadContent()
        // {
        //     spriteBatch = new SpriteBatch(GraphicsDevice);
        //
        //     Textures.Load();
        //     SpriteFonts.Load();
        // }
        //
        // private string debugText = "";
        // private MouseState previousMouseState;
        // private Vector2 startDragPosition;
        //
        // protected override void Update(GameTime gameTime)
        // {
        //     if (!IsActive) return;
        //
        //     if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
        //         Keyboard.GetState().IsKeyDown(Keys.Escape))
        //         Exit();
        //
        //     // TODO: Add your update logic here
        //
        //     blackHoleRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalMilliseconds * 0.001);
        //
        //     var mouseState = Mouse.GetState();
        //
        //     if (previousMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed) {
        //         startDragPosition = mouseState.Position.ToVector2();
        //     } else if (previousMouseState.LeftButton == ButtonState.Pressed &&
        //         mouseState.LeftButton == ButtonState.Released)
        //         startDragPosition = default;
        //
        //     previousMouseState = mouseState;
        //
        //     debugText = JsonSerializer.Serialize(mouseState);
        //
        //     base.Update(gameTime);
        // }
        //
        // protected override void Draw(GameTime gameTime)
        // {
        //     GraphicsDevice.Clear(Color.CornflowerBlue);
        //
        //     // TODO: Add your drawing code here
        //     spriteBatch.Begin();
        //     spriteBatch.Draw(Textures.blackhole, blackHolePosition, null, Color.White, blackHoleRotation,
        //         new Vector2(Textures.blackhole.Width / 2f, Textures.blackhole.Height / 2f), Vector2.One,
        //         SpriteEffects.FlipHorizontally,
        //         0f);
        //     spriteBatch.DrawString(SpriteFonts.Verdana, debugText, Vector2.Zero, Color.Black);
        //     if (previousMouseState.LeftButton == ButtonState.Pressed)
        //         spriteBatch.DrawLine(Color.Chartreuse, startDragPosition, previousMouseState.Position.ToVector2());
        //     spriteBatch.End();
        //
        //     base.Draw(gameTime);
        // }
    }
}
