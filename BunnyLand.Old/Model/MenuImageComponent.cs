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


namespace BunnyLand.Model
{
    /// <summary>
    /// This is an image that appears in the menu
    /// </summary>
    public class MenuImageComponent
    {
        public Texture2D Image { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; }

        public MenuImageComponent(Game game, Texture2D image, int xPos, int yPos)
        {
           /* Image = image;
            Position = new Vector2(xPos, yPos);
            Rotation = 0;
            Width = Image.Width;
            Height = Image.Height;*/
        }
    }
}