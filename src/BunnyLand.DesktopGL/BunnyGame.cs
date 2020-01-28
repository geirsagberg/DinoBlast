using System;
using BunnyLand.DesktopGL.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace BunnyLand.DesktopGL
{
    public class BunnyGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly GameSettings settings;
        private Vector2 blackHolePosition;
        private float blackHoleRotation;
        private SpriteBatch spriteBatch = null!;

        public BunnyGame(GameSettings settings)
        {
            this.settings = settings;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Textures = new Textures(Content);
        }

        public Textures Textures { get; }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            blackHolePosition = new Vector2(graphics.PreferredBackBufferWidth / 2f,
                graphics.PreferredBackBufferHeight / 2f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Textures.Load();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            blackHoleRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalMilliseconds * 0.001);

            var touchState = TouchPanel.GetState();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(Textures.blackhole, blackHolePosition, null, Color.White, blackHoleRotation,
                new Vector2(Textures.blackhole.Width / 2f, Textures.blackhole.Height / 2f), Vector2.One,
                SpriteEffects.FlipHorizontally,
                0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
