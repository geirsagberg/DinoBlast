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


namespace BunnyLand.Models
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TextMenuComponent : Microsoft.Xna.Framework.GameComponent
    {
        /* flytt til view
        // Attributes that control the appearance of the menu items
        private SpriteFont standardFont, selectedFont;
        private Color standardColor, selectedColor;
        */

        // Menu items
        public int SelectedIndex { get; set; }
        public List<String> menuItems;

        // Size of menu in pixels
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        ///  The actual menu component, containing the menu items
        /// </summary>
        /// <param name="game"></param>
        public TextMenuComponent(Game game)
            : base(game)
        {
            this.menuItems = new List<string>();
        }

        /// <summary>
        /// Set the menu items
        /// </summary>
        /// <param name="items"></param>
        public void SetMenuItems(string[] items)
        {
            menuItems.Clear();
            menuItems.AddRange(items);
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

        public void IncrementSelectedIndex()
        {
            SelectedIndex++;
            if (SelectedIndex > menuItems.Count - 1)
                SelectedIndex = 0;
            while (menuItems.ElementAt(SelectedIndex) == "")  // Skip empty items
            {
                IncrementSelectedIndex();
            }
        }

        public void DecrementSelectedIndex()
        {
            SelectedIndex--;
            if (SelectedIndex < 0)
                SelectedIndex = menuItems.Count - 1;
            while (menuItems.ElementAt(SelectedIndex) == "")  // Skip empty items
            {
                DecrementSelectedIndex();
            }
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
    }
}