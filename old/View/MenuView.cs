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
    /// This class implements a view of the main menu
    /// </summary>
    public class MenuView : AbstractView
    {
        MenuModel model;
        Color menuRegularColor = Color.White;
        Color menuSelectedColor = Color.Red;
        Color backgroundColor = Color.Thistle;

        public MenuView(Game game, MenuModel model)
            : base(game)
        {
            this.model = model;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw all the elements of the menu
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            //Draw menu background image
            Rectangle screenRect = new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight);
            spriteBatch.Draw(Sprites.MenuBackground, screenRect, Color.White);

            // Coordinates for the menu (the TextMenuComponent)
            int x = (int)(device.PresentationParameters.BackBufferWidth * 0.15);
            int y = (int)(device.PresentationParameters.BackBufferHeight * 0.3);

            // Clear the screen
            device.Clear(backgroundColor);

            // Loop through the menu items
            for (int i = 0; i < model.GetMenuItems().Count; i++)
            {
                // Set the appropriate font and color for the menu item
                SpriteFont font;
                Color color;
                if (i == model.Menu.SelectedIndex)
                {
                    font = Sprites.MenuSelectedFont;
                    color = menuSelectedColor;
                }
                else
                {
                    font = Sprites.MenuRegularFont;
                    color = menuRegularColor;
                }

                // Draw the menu item (with an added black shadow)
                spriteBatch.DrawString(font, model.GetMenuItems().ElementAt(i), new Vector2(x + 1, y), Color.Black);
                spriteBatch.DrawString(font, model.GetMenuItems().ElementAt(i), new Vector2(x, y), color);

                y += font.LineSpacing;
            }

            // Coordinates for the message
            x = (int)(device.PresentationParameters.BackBufferWidth * 0.05);
            y = (int)(device.PresentationParameters.BackBufferHeight * 0.9);

            // Draw the message
            spriteBatch.DrawString(Sprites.SpriteFont, model.Message, new Vector2(x, y), Color.BlueViolet);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}