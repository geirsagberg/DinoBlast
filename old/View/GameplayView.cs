using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using BunnyLand.Models;


namespace BunnyLand.Views
{
    /// <summary>
    /// This class implements a view of the game in action
    /// </summary>
    public class GameplayView : AbstractView
    {

        Color backgroundColor = Color.CornflowerBlue;

        // The GameplayModel that is to be drawn
        public GameplayModel Model { get; set; }

        RenderTarget2D renderTarget;
        DepthStencilBuffer depthStencilBuffer;

        List<Line> lineList = new List<Line>();
        /// <summary>
        /// 
        /// </summary>
        List<StampSprite> stampSpriteList = new List<StampSprite>();

        public Camera2D Camera { get; set; }
        public Player FollowingPlayer { get; set; }

        public GameplayView(Game game, GameplayModel model)
            : base(game)
        {
            /*
             graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(GraphicsDeviceManager));
             device = graphics.GraphicsDevice;
             spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
             LineBatch.Init(device);
             */

            Camera = new Camera2D(800, 600); //default, will be changed later
            Model = model;
            Model.View = this;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            /*
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.ApplyChanges();
            */

            base.Initialize();
        }

        /// <summary>
        /// Called when the DrawableGameComponent needs to be drawn. Override this method with component-specific drawing code. Reference page contains links to related conceptual articles.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to Draw.</param>
        public override void Draw(GameTime gameTime)
        {
            // All drawing should happen here!

            DrawGameplayScene();

            /* Update camera */
            UpdateCamera();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Updates the camera position and rotation.
        /// </summary>
        private void UpdateCamera()
        {
            Camera.Zoom += Camera.ZoomVelocity;
            Camera.Rotation += Camera.RotationVelocity;
            //if (FollowingPlayer != null)
            if (Model.PlayerList.Count > 0)
            {
                // Find 
                Vector2 middle = Vector2.Zero;

                //Find center position between players
                foreach (Player p in Model.PlayerList)
                {
                    middle += p.Position;

                }
                middle /= Model.PlayerList.Count;
                Camera.Position = middle;

                //Find a rectangle containing all players
                Vector2 topLeft = middle;
                Vector2 bottomRight = middle;
                int margin = 150;
                foreach (Player p in Model.PlayerList)
                {
                    if (p.Position.X < topLeft.X)
                        topLeft.X = MathHelper.Max(p.Position.X - margin, 0);
                    if (p.Position.Y < topLeft.Y)
                        topLeft.Y = MathHelper.Max(p.Position.Y - margin, 0);

                    if (p.Position.X > bottomRight.X)
                        bottomRight.X = MathHelper.Min(p.Position.X + margin, Camera.LevelSize.X);
                    if (p.Position.Y > bottomRight.Y)
                        bottomRight.Y = MathHelper.Min(p.Position.Y + margin, Camera.LevelSize.Y);
                }

                Rectangle rectangle = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));

                if (BunnyGame.DebugMode)
                    DrawRectangle(rectangle, Color.Beige);

                if (!Camera.Viewport.Contains(rectangle))
                {
                    Camera.ZoomVelocity = -Camera.ZoomSpeed;
                }
                else
                    Camera.ZoomVelocity = +Camera.ZoomSpeed;
            }
            else
            {
                Camera.Position += Camera.Velocity;
                Camera.ZoomVelocity = 0;
            }
            Camera.RotationVelocity = 0;
            Camera.Velocity = Vector2.Zero;
        }

        /// <summary>
        /// Draws the gameplay scene.
        /// </summary>
        private void DrawGameplayScene()
        {

            // Render explosions to texture
            if (renderTarget != null && stampSpriteList.Count > 0)
            {
                device.SetRenderTarget(0, renderTarget);
                DepthStencilBuffer old = device.DepthStencilBuffer;
                device.DepthStencilBuffer = depthStencilBuffer;
                spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Deferred, SaveStateMode.None);
                DrawTerrain(Model.TerrainTexture);
                foreach (StampSprite e in stampSpriteList)
                {
                    spriteBatch.Draw(e.Texture, e.Position, Color.White);
                }
                stampSpriteList.Clear();
                spriteBatch.End();

                device.SetRenderTarget(0, null);
                device.DepthStencilBuffer = old;

                Model.TerrainTexture = renderTarget.GetTexture();
            }
            device.Clear(backgroundColor);

            /* Draw background image */
            Rectangle screenRect = new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth,
                                              device.PresentationParameters.BackBufferHeight);
            spriteBatch.Begin();
            spriteBatch.Draw(Model.BackGround, screenRect, Color.White);
            spriteBatch.End();

            /* Draw terrain, entities and lines */
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, Camera.Transform);
            DrawTerrain(Model.TerrainTexture);
            DrawEntities(Model.EntityList);
            DrawPlayerInfo(Model.PlayerList);
            DrawLines();
            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
            DrawScores(Model.PlayerList);

            if (Model.GameOver)
            {
                //Draw end game and win notice 
                Color color = ColorFromPlayerIndex(Model.GetWinner());
                string gameOver = "Game Over! Player " + Model.GetWinner() + " wins!";
                string quit = "Press ESC to go back to the menu.";
                Vector2 gameOverSize = Sprites.MenuRegularFont.MeasureString(gameOver);
                Vector2 quitSize = Sprites.MenuRegularFont.MeasureString(quit);
                spriteBatch.DrawString(Sprites.MenuRegularFont, gameOver, Camera.Position - new Vector2(gameOverSize.X / 2, gameOverSize.Y), color);
                spriteBatch.DrawString(Sprites.MenuRegularFont, quit, Camera.Position - new Vector2(quitSize.X / 2, 0), color);
            }

            spriteBatch.End();

            /* Update camera */
            UpdateCamera();

        }

        /// <summary>
        /// Draws the scoreboard
        /// </summary>
        /// <param name="list"></param>
        private void DrawScores(List<Player> list)
        {
            Vector2 scoreBoardPosition = new Vector2(10);
            int lineDistance = 20;
            int killsDistance = 120;
            int deathsDistance = 170;
            string s = "Player:        Kills: Deaths:\n";
            spriteBatch.DrawString(Sprites.SpriteFont, s, scoreBoardPosition, Color.White);
            scoreBoardPosition.Y += lineDistance;
            foreach (Player p in list)
            {
                Color color = ColorFromPlayerIndex(p.PlayerIndex);

                spriteBatch.DrawString(Sprites.SpriteFont, "Player " + p.PlayerIndex + ":", scoreBoardPosition, color);
                spriteBatch.DrawString(Sprites.SpriteFont, ((Scores)Model.ScoreBoard[p]).Kills.ToString(), scoreBoardPosition + new Vector2(killsDistance, 0), color);
                spriteBatch.DrawString(Sprites.SpriteFont, ((Scores)Model.ScoreBoard[p]).Deaths.ToString(), scoreBoardPosition + new Vector2(deathsDistance, 0), color);
                scoreBoardPosition.Y += lineDistance;
            }
        }

        /// <summary>
        /// Draws the terrain.
        /// </summary>
        /// <param name="terrainTexture">The terrain texture.</param>
        private void DrawTerrain(Texture2D terrainTexture)
        {
            if (terrainTexture != null)
            {
                spriteBatch.Draw(terrainTexture, Vector2.Zero, Color.White);
                //spriteBatch.Draw(terrainTexture, new Vector2(Camera.Viewport.Location.X, Camera.Viewport.Location.Y), Camera.Viewport, Color.White);
            }
        }

        /// <summary>
        /// Draws all entities in the game.
        /// </summary>
        /// <param name="list">The list of entities.</param>
        private void DrawEntities(List<Entity> list)
        {
            foreach (Entity e in list)
            {
                if (e.Enabled && e.BoundingBox.Intersects(Camera.Viewport))
                    spriteBatch.Draw(e.Spritesheet, e.Position, e.SourceRect, e.Color, e.Angle, e.Origin, e.Scale, e.SpriteEffect, 0.5f);
            }
        }

        /// <summary>
        /// Draws aiming lines and other vectors
        /// </summary>
        private void DrawLines()
        {
            foreach (Line l in lineList)
                l.Draw(spriteBatch, Vector2.Zero);
            lineList.Clear();
        }

        /// <summary>
        /// Assigns a color to each player (used for drawing the player's health and score)
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        private Color ColorFromPlayerIndex(PlayerIndex pi)
        {
            Color color;
            if (pi == PlayerIndex.One)
                color = Color.Red;
            else if (pi == PlayerIndex.Two)
                color = Color.Blue;
            else if (pi == PlayerIndex.Three)
                color = Color.Green;
            else
                color = Color.Yellow;

            return color;
        }

        private void DrawPlayerInfo(List<Player> players)
        {

            foreach (Player p in players)
            {


                if (p.Enabled)
                {
                    //draw health
                    spriteBatch.DrawString(Sprites.SpriteFont, ((int)p.Health).ToString(), p.Position - new Vector2(15, 35), ColorFromPlayerIndex(p.PlayerIndex));

                    float ammoRatio = (float)p.CurrentWeapon.CurrentAmmo / (float)p.CurrentWeapon.AmmoCapacity;
                    /*
                    //draw ammo
                    Vector2 ammoCapStart = p.Position + new Vector2(-p.Size.X / 2, p.Size.Y / 2);
                    Vector2 ammoCapLength = new Vector2(0, -p.Size.Y * ammoRatio);
                    DrawVector(ammoCapStart, ammoCapLength, 1, Color.Red);
                     */

                    //Draw aiming and ammo
                    Vector2 aimingVector = Utility.ToVector(p.AimingAngle, 30);
                    DrawVector(p.BodyCenter + aimingVector, ammoRatio * Utility.ToVector(p.AimingAngle, 70), 1, Color.White);

                    //draw weapon
                    if (p.CurrentWeapon != null)
                    {
                        Weapon weapon = p.CurrentWeapon;
                        SpriteEffects spriteEffect = p.IsFacingRight ? SpriteEffects.None : SpriteEffects.FlipVertically;
                        Vector2 origin = p.IsFacingRight ? weapon.Origin : new Vector2(weapon.Origin.X, weapon.SourceRect.Height - weapon.Origin.Y);
                        spriteBatch.Draw(weapon.WeaponTexture, p.BodyCenter, weapon.SourceRect, Color.White, p.AimingAngle, origin, 1f, spriteEffect, 0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a vector at the given position with the given magnification and color.
        /// </summary>
        /// <param name="position">The starting position.</param>
        /// <param name="vector">The vector.</param>
        /// <param name="magnification">The magnification.</param>
        /// <param name="color">The color.</param>
        public void DrawVector(Vector2 position, Vector2 vector, float magnification, Color color)
        {
            lineList.Add(new Line(position, vector * magnification, color));
        }

        /// <summary>
        /// Draws the given rectangle to screen in the next frame, with the given color.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="color">The color.</param>
        public void DrawRectangle(Rectangle rectangle, Color color)
        {
            lineList.Add(new Line(new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.Width, 0), color));
            lineList.Add(new Line(new Vector2(rectangle.X, rectangle.Y), new Vector2(0, rectangle.Height), color));
            lineList.Add(new Line(new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.Width, 0), color));
            lineList.Add(new Line(new Vector2(rectangle.X + rectangle.Width, rectangle.Y), new Vector2(0, rectangle.Height), color));
        }

        public void DrawRectangle(Rectangle rectangle, Matrix transform, Color color)
        {
            lineList.Add(new Line(Vector2.Transform(new Vector2(rectangle.X, rectangle.Y), transform), new Vector2(rectangle.Width, 0), color));
            lineList.Add(new Line(Vector2.Transform(new Vector2(rectangle.X, rectangle.Y), transform), new Vector2(0, rectangle.Height), color));
            lineList.Add(new Line(Vector2.Transform(new Vector2(rectangle.X, rectangle.Y + rectangle.Height), transform), new Vector2(rectangle.Width, 0), color));
            lineList.Add(new Line(Vector2.Transform(new Vector2(rectangle.X + rectangle.Width, rectangle.Y), transform), new Vector2(0, rectangle.Height), color));
        }

        /// <summary>
        /// Adds an explosion to the list of explosions to be drawn in next frame.
        /// </summary>
        /// <param name="color">The colordata of the explosion.</param>
        /// <param name="posX">The leftmost X coordinate.</param>
        /// <param name="posY">The topmost Y coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void DrawToTerrain(Color[] color, int posX, int posY, int width, int height)
        {
            StampSprite explosion = new StampSprite(new Texture2D(device, width, height), posX, posY);
            explosion.Texture.SetData<Color>(color);
            stampSpriteList.Add(explosion);
        }

        // A line with a starting position and a direction vector.
        private struct Line
        {
            Vector2 point1;
            Vector2 point2;
            Color color;

            public Line(Vector2 position, Vector2 vector, Color color)
            {
                this.point1 = position;
                this.point2 = position + vector;
                this.color = color;
            }

            public void Draw(SpriteBatch spriteBatch, Vector2 offset)
            {
                LineBatch.DrawLine(spriteBatch, color, point1 + offset, point2 + offset);
            }
        }

        /// <summary>
        /// Sprite class for drawing to texture.
        /// </summary>
        private class StampSprite
        {
            Texture2D texture;
            public Texture2D Texture
            {
                get { return texture; }
            }
            Vector2 position;
            public Vector2 Position
            {
                get { return position; }
            }

            public StampSprite(Texture2D explosion, int posX, int posY)
            {
                this.texture = explosion;
                position = new Vector2(posX, posY);
            }
        }
        /// <summary>
        /// Initializes the render target.
        /// </summary>
        public void InitializeRenderTarget()
        {
            //Update camera with level boundaries
            Camera.LevelSize = new Vector2(Model.Terrain.Width, Model.Terrain.Height);

            renderTarget = new RenderTarget2D(device, Model.Terrain.Width, Model.Terrain.Height, 1, SurfaceFormat.Color);
            device.SetRenderTarget(0, renderTarget);

            DepthStencilBuffer old = GraphicsDevice.DepthStencilBuffer;
            // Set our custom depth buffer
            device.DepthStencilBuffer = depthStencilBuffer = CreateDepthStencil(renderTarget);

            spriteBatch.Begin();
            DrawTerrain(Model.TerrainTexture);
            spriteBatch.End();

            device.SetRenderTarget(0, null);
            device.DepthStencilBuffer = old;
        }

        /// <summary>
        /// Set resolution from the previously stored settings. If that fails, a safe resolution is set.
        /// </summary>
        public void setInitialResolution()
        {
            int[] resolution = Utility.ResolutionToInts(Settings.Resolution);
            if (!InitGraphicsMode(resolution[0], resolution[1], Settings.FullScreen))
            {
                Settings.Resolution = Resolution.Res_800x600;
                Settings.FullScreen = false;
                resolution = Utility.ResolutionToInts(Settings.Resolution);
                InitGraphicsMode(resolution[0], resolution[1], Settings.FullScreen);
                Settings.writeSettingsToIniFile();
            }
        }

        /// <summary>
        /// Creates the depth stencil, for usage with a RenderTarget of a size different from screen size.
        /// </summary>
        /// <param name="target">The rendertarget.</param>
        /// <returns></returns>
        public static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target)
        {
            return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                target.Height, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }

        /// <summary>
        /// Adjust camera to fit resolution
        /// </summary>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        /// <param name="bFullScreen"></param>
        /// <returns></returns>
        public override bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            if (base.InitGraphicsMode(iWidth, iHeight, bFullScreen))
            {
                Camera.ScreenSize = new Vector2(iWidth, iHeight);
                return true;
            }
            else
                return false;
        }
    }
}