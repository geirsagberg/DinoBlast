using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace BunnyLand.DesktopGL
{
    public class BunnyGame : Game
    {
        private readonly GameSettings settings;
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch = null!;
        private Texture2D blackHoleTexture = null!;
        private Vector2 blackHolePosition;
        private float blackHoleRotation;

        public BunnyGame(GameSettings settings)
        {
            this.settings = settings;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

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

            // TODO: use this.Content to load your game content here
            blackHoleTexture = Content.Load<Texture2D>("black-hole");
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
            spriteBatch.Draw(blackHoleTexture, blackHolePosition, null, Color.White, blackHoleRotation,
                new Vector2(blackHoleTexture.Width / 2f, blackHoleTexture.Height / 2f), Vector2.One, SpriteEffects.FlipHorizontally,
                0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
