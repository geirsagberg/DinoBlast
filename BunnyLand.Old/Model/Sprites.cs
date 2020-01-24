using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Storage;

namespace BunnyLand.Models
{
    public static class Sprites
    {
        public static Texture2D[] BodyParts = new Texture2D[10]; 
        public static Texture2D BlackHoleSprite { get; set; }
        public static Texture2D BackgroundTex { get; set; }
        public static Texture2D BulletSprite { get; set; }
        public static Texture2D BunnySpritesheet { get; set; }
        public static Texture2D TestMap { get; set; }
        public static SpriteFont SpriteFont { get; set; }
        public static SpriteFont MenuRegularFont { get; set; }
        public static SpriteFont MenuSelectedFont { get; set; }
        public static Texture2D BloodSprite { get; set; }
        public static Texture2D BloodSprite2 { get; set; }
        public static Texture2D ExplosionSpritesheet { get; set; }
        public static Texture2D RedPlanetSprite { get; set; }
        public static Texture2D EarthPlanetSprite { get; set; }
        public static Texture2D BodySprite { get; set; }
        public static Texture2D Ear1Sprite { get; set; }
        public static Texture2D Ear2Sprite { get; set; }
        public static Texture2D Eye1Sprite { get; set; }
        public static Texture2D Eye2Sprite { get; set; }
        public static Texture2D FootSprite { get; set; }
        public static Texture2D HandSprite { get; set; }
        public static Texture2D HeadSprite { get; set; }
        public static Texture2D MenuBackground { get; set; }
        public static Texture2D HandgunWeaponSpritesheet { get; set; }

        //

        // Planet textures
        public static List<Texture2D> Planets { get; set; }

        // Backgrounds
        public static List<Texture2D> BackGrounds_16_10;
        public static List<Texture2D> BackGrounds_16_9;
        public static List<Texture2D> BackGrounds_4_3;
        public static List<Texture2D> BackGrounds_5_4;

        public static List<Texture2D> TerrainBackgrounds;

        /// <summary>
        /// Returns an array of rectangles describing all the sprites of the spritesheet.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="Spritesheet">The spritesheet.</param>
        /// <returns></returns>
        public static Rectangle[] GetSprites(int columns, int rows, Texture2D Spritesheet)
        {
            int spriteWidth = Spritesheet.Width / columns;
            int spriteHeight = Spritesheet.Height / rows;
            Rectangle[] sprites = new Rectangle[columns * rows];
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    sprites[y * columns + x] = new Rectangle(spriteWidth * x, spriteHeight * y, spriteWidth, spriteHeight);
                }
            }
            return sprites;
        }

        public static void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content, bool loadHighresTextures)
        {
            
            Sprites.BlackHoleSprite = Content.Load<Texture2D>("Textures/black-hole");
            Sprites.BulletSprite = Content.Load<Texture2D>("Textures/bullet");
            Sprites.BunnySpritesheet = Content.Load<Texture2D>("Textures/PlayerAnimation");
            Sprites.SpriteFont = Content.Load<SpriteFont>("Fonts/SpriteFont1");
            Sprites.MenuRegularFont = Content.Load<SpriteFont>("Fonts/menuRegular");
            Sprites.MenuSelectedFont = Content.Load<SpriteFont>("Fonts/menuSelected");
            Sprites.TestMap = Content.Load<Texture2D>("Textures/totoro");
            Sprites.ExplosionSpritesheet = Content.Load<Texture2D>("Textures/Explotion");
            Sprites.BloodSprite = Content.Load<Texture2D>("Textures/blood");
            Sprites.BloodSprite2 = Content.Load<Texture2D>("Textures/blood2");
            Sprites.BodyParts[0] = Content.Load<Texture2D>("Textures/Bodyparts/Body");
            Sprites.BodyParts[1] = Content.Load<Texture2D>("Textures/Bodyparts/Ear1");
            Sprites.BodyParts[2] = Content.Load<Texture2D>("Textures/Bodyparts/Ear2");
            Sprites.BodyParts[3] = Content.Load<Texture2D>("Textures/Bodyparts/Eye1");
            Sprites.BodyParts[4] = Content.Load<Texture2D>("Textures/Bodyparts/Eye2");
            Sprites.BodyParts[5] = Sprites.BodyParts[6] = Content.Load<Texture2D>("Textures/Bodyparts/Foot");
            Sprites.BodyParts[7] = Sprites.BodyParts[8] = Content.Load<Texture2D>("Textures/Bodyparts/Hand");
            Sprites.BodyParts[9] = Content.Load<Texture2D>("Textures/Bodyparts/Head");
            Sprites.MenuBackground = Content.Load<Texture2D>("Textures/Menu/cutebunnies");
            Sprites.HandgunWeaponSpritesheet = Content.Load<Texture2D>("Textures/Handweapon");

            // Load planet textures
            Planets = loadAllTexturesInFolder(Content, "Textures/Planets");

            // Load game backgrounds
            BackGrounds_4_3 = loadAllTexturesInFolder(Content, "Textures/GameBackgrounds/4-3");
            BackGrounds_5_4 = loadAllTexturesInFolder(Content, "Textures/GameBackgrounds/5-4");
            BackGrounds_16_9 = loadAllTexturesInFolder(Content, "Textures/GameBackgrounds/16-9");
            BackGrounds_16_10 = loadAllTexturesInFolder(Content, "Textures/GameBackgrounds/16-10");
            
            // Load terrain backgrounds
            if(loadHighresTextures)
                TerrainBackgrounds = loadAllTexturesInFolder(Content, "Textures/Terrain");
        }

        /// <summary>
        /// Loads all the textures in the given folder and returns them in a list
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static List<Texture2D> loadAllTexturesInFolder(Microsoft.Xna.Framework.Content.ContentManager Content, string folder)
        {
            List<Texture2D> textures = new List<Texture2D>();
            string[] files = Directory.GetFiles(Path.Combine(StorageContainer.TitleLocation, Path.Combine(Content.RootDirectory, folder)), "*.xnb");
            foreach (string file in files)
            {
                string assetName = Path.Combine(folder, Path.GetFileNameWithoutExtension(file));
                textures.Add(Content.Load<Texture2D>(assetName));
            }
            return textures;
        }
    }
}

