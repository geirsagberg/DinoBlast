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
using BunnyLand.Views;
using BunnyLand.Controllers;
using System.IO;

namespace BunnyLand
{
    /// <summary>
    /// This is the main type for the game
    /// </summary>
    public class BunnyGame : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        public static bool DebugMode = false; //set true to draw vectors and display debug output

        // Model
        public MenuModel MenuModel { get; set; }
        public GameplayModel GameplayModel { get; set; }

        // View
        public MenuView MenuView { get; set; }
        public GameplayView GameplayView { get; set; }

        // Controller
        public Controller Controller { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BunnyGame()
        {
            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;

            // Initialize a GraphicsDeviceManager and add it as a Service
            graphics = new GraphicsDeviceManager(this);
            Services.AddService(typeof(GraphicsDeviceManager), graphics);

            // Read settings from settings.ini
            List<string> settingValues = new List<string>();
            List<string> settingsFromFile;
            try { settingsFromFile = Utility.ReadFromFile("settings.ini"); } catch (FileNotFoundException e) {
                settingsFromFile = new List<string>();
            }
            if (settingsFromFile.Count < 4) // Settings read from file are wrong. Use standard settings and create new settings.ini file
            {
                Settings.Resolution = Resolution.Res_800x600;
                Settings.FullScreen = false;
                Settings.EntityLimit = Setting.Medium;
                Settings.GoreLevel = Setting.Medium;
                Settings.writeSettingsToIniFile();
            } else {
                for (int i = 0; i < settingsFromFile.Count; i++) {
                    settingValues.Add(settingsFromFile.ElementAt(i).Split('=')[1].Trim());
                }

                // Apply settings
                Settings.Resolution = (Resolution) Enum.Parse(typeof(Resolution), settingValues[0], true);
                Settings.FullScreen = Boolean.Parse(settingValues[1]);
                Settings.EntityLimit = (Setting) Enum.Parse(typeof(Setting), settingValues[2], true);
                Settings.GoreLevel = (Setting) Enum.Parse(typeof(Setting), settingValues[3], true);
            }

            // Initialize model
            MenuModel = new MenuModel(this);
            GameplayModel = new GameplayModel(this);

            // Initialize view
            MenuView = new MenuView(this, MenuModel);
            GameplayView = new GameplayView(this, GameplayModel);

            // Initialize controller
            Controller = new Controller(this, MenuModel);

            // Add model components
            Components.Add(MenuModel);
            Components.Add(GameplayModel);

            // Add view components
            Components.Add(MenuView);
            Components.Add(GameplayView);

            // Add controller component
            Components.Add(Controller);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            // Start game in main menu
            MenuModel.Enable();
            MenuView.Show();

            // Enable controller
            Controller.Enable();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // Create the SpriteBatch and add it as a service
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            // Initialize graphics device
            MenuView.InitializeGraphicsDevice();
            GameplayView.InitializeGraphicsDevice();

            // If the running grphics device supports it, high resolution textures will be used for the
            // terrain. (Otherwise the terrain will get a random color with some grain for added detail.)
            bool loadHighresTextures = GraphicsDevice.GraphicsDeviceCapabilities.MaxTextureWidth >= 2560;

            // Load sprites
            Sprites.LoadContent(Content, loadHighresTextures);

            // Load audio
            Audio.LoadContent(Content);

            // Set resolution to previously stored setting
            GameplayView.setInitialResolution();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Go to the menu. Which menu is shown, depends on the menuState parameter.
        /// </summary>
        /// <param name="menuState"></param>
        public void GoToMenu(MenuState menuState)
        {
            GameplayModel.Disable();
            GameplayView.Hide();
            MenuModel.Enable();
            MenuView.Show();
            MenuModel.setMenuState(menuState);
            Audio.stopMusic();
            Controller.Model = MenuModel;
        }

        /// <summary>
        /// Switches to gameplay mode
        /// </summary>
        public void GoToGameplay()
        {
            MenuModel.Disable();
            MenuView.Hide();
            GameplayModel.Enable();
            GameplayView.Show();
            Audio.startMusic();
            Controller.Model = GameplayModel;
        }

        /// <summary>
        /// Creates a new game
        /// </summary>
        /// <param name="mapType">Type of map (simple or complex)</param>
        /// <param name="mapSize">Size of map (various preset dimensions</param>
        /// <param name="numberPlayers">Number of players (2-4)</param>
        /// <param name="killLimit">Number of kills needed to win the game</param>
        /// <param name="numberPlanets">Number of randomly generated planets in the map</param>
        /// <param name="numberBlackHoles">Number of randomly generated black holes in the map</param>
        public void CreateNewGame(MapType mapType, MapSize mapSize, int numberPlayers, int killLimit, int numberPlanets, int numberBlackHoles)
        {
            GameplayModel.CreateNewGame(mapType, mapSize, numberPlayers, killLimit, numberPlanets, numberBlackHoles);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            base.Draw(gameTime);
        }

    }
}
