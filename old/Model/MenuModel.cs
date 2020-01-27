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
using BunnyLand.Controllers;
using System.IO;


namespace BunnyLand.Models
{
    public enum MenuState
    {
        MainMenu,
        SettingsMenu,
        GameConfigMenu,
        PauseMenu
    }

    public enum MapType
    {
        Simple,
        Complex
    }

    public enum MapSize
    {
        Size_800x600,
        Size_1024x768,
        Size_1152x864,
        Size_1280x720,
        Size_1280x800,
        Size_1280x1024,
        Size_1366x768,
        Size_1440x900,
        Size_1600x900,
        Size_1600x1200,
        SIze_1600x1280,
        Size_1680x1050,
        Size_1920x1080,
        Size_1920x1200,
        Size_1920x1536,
        Size_2048x1536,
        Size_2560x1440
    }

    /// <summary>
    /// This class implements the model of the game's main menu
    /// </summary>
    public class MenuModel : AbstractModel
    {
        // Menu item descriptors for the main menu
        const string PLAY = "New Game";
        const string SETTINGS = "Settings";

        // Menu item descriptors for the settings menu
        const string RESOLUTION = "Resolution: ";
        const string FULL_SCREEN = "Fullscreen: ";
        const string ENTITY_LIMIT = "Entity limit: ";
        const string GORE_LEVEL = "Gore level: ";
        const string APPLY = "APPLY";

        // Menu item descriptors for the game configuration menu
        const string NUM_PLAYERS = "Number of players: ";
        const string MAP_TYPE = "Map type: ";
        const string MAP_SIZE = "Map size: ";
        const string NUM_PLANETS = "Number of planets: ";
        const string NUM_BLACKHOLES = "Number of black holes: ";
        const string KILL_LIMIT = "Kill limit: ";
        const string START = "START GAME";

        // Menu item descriptors for the pause menu
        const string RESUME = "Resume";
        const string END_GAME = "End Game";

        // Menu item descriptors that appear in more than one menu
        const string BACK = "BACK";
        const string QUIT = "Quit";

        private BunnyGame game;
        public TextMenuComponent Menu { get; set; }
        public string Message { get; set; }
        private MenuState menuState;
        string[] mainMenuItems = { PLAY, SETTINGS, QUIT };
        string[] settingsMenuItems = { RESOLUTION, FULL_SCREEN, ENTITY_LIMIT, GORE_LEVEL, "", APPLY, BACK};
        string[] gameConfigMenuItems = { NUM_PLAYERS, MAP_TYPE, MAP_SIZE, NUM_PLANETS, NUM_BLACKHOLES, KILL_LIMIT, "", START, BACK };
        string[] pauseMenuItems = { RESUME, END_GAME, QUIT };

        private float aspectRatio;  // Used when deciding size of terrain. (Aspect ratio of terrain should be same as for display.)

        bool startGame = false; // If this is true, the game will start in the next update cycle (allowing the loading message to be displayed in this cycle)

        // New settings that will be applied to Settings (static class) when pressing update in the settings menu,
        // initially set to the current settings from Settings.
        Resolution newResolution = Settings.Resolution;
        bool newFullScreen = Settings.FullScreen;
        Setting newEntityLimit = Settings.EntityLimit;
        Setting newGoreLevel = Settings.GoreLevel;

        // Configuration variables that will be used when starting a game. These variables are set by
        // the user in the game configuration menu.
        int numberPlayers;
        MapType mapType;
        MapSize mapSize;
        int numberPlanets;
        int numberBlackHoles;
        int killLimit;
        



        public MenuModel(Game game)
            : base(game)
        {
            this.game = (BunnyGame)game;

             // Create menu (initially main menu)
            Menu = new TextMenuComponent(game);
            menuState = MenuState.MainMenu;
            Menu.SetMenuItems(mainMenuItems);

            // Set the message to the empty string initially
            Message = "";

            // Initialize content of settings menu
            string resolution = Utility.ResolutionToString(Settings.Resolution);
            string fullScreen;
            if (Settings.FullScreen) { fullScreen = "On"; } else { fullScreen = "Off"; }
            string entityLimit = Settings.EntityLimit.ToString();
            string goreLevel = Settings.GoreLevel.ToString();
            settingsMenuItems[0] += resolution;
            settingsMenuItems[1] += fullScreen;
            settingsMenuItems[2] += entityLimit;
            settingsMenuItems[3] += goreLevel;



            // Calculate initial aspect ratio:
            aspectRatio = Utility.GetAspectRatio();
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


#region Action handling (user input)
        public override void ActionStarted(PlayerIndex pi, BunnyLand.Controllers.PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.AimUp:
                    Menu.DecrementSelectedIndex();
                    break;
                case PlayerAction.AimDown:
                    Menu.IncrementSelectedIndex();
                    break;
                case PlayerAction.Jump:
                    ActivateMenuItem();
                    break;
                case PlayerAction.Accept:
                    ActivateMenuItem();
                    break;
                case PlayerAction.Cancel:
                    switch (menuState)
                    {
                        case MenuState.MainMenu:
                            game.Exit();
                            break;
                        case MenuState.SettingsMenu:
                            setMenuState(MenuState.MainMenu);
                            break;
                        case MenuState.GameConfigMenu:
                            setMenuState(MenuState.MainMenu);
                            break;
                        case MenuState.PauseMenu:
                            game.GoToGameplay();
                            break;
                    }
                    break;
            }
        }

        public override void ActionStopped(PlayerIndex pi, BunnyLand.Controllers.PlayerAction action)
        {
            // TODO: Implement method
        }

        public override void ActionContinued(PlayerIndex pi, BunnyLand.Controllers.PlayerAction action)
        {
            // TODO: Implement method
        }
#endregion

        /// <summary>
        /// This method is triggered when a user activates an item in the menu. Based on which state the menu
        /// is in (main menu, settings etc), this method calls the method that handles the activation of menu items
        /// for that state.
        /// </summary>
        public void ActivateMenuItem()
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    ActivateMainMenuItem();
                    break;
                case MenuState.SettingsMenu:
                    ActivateSettingsMenuItem();
                    break;
                case MenuState.GameConfigMenu:
                    ActivateGameConfigMenuItem();
                    break;
                case MenuState.PauseMenu:
                    ActivatePauseMenuItem();
                    break;
            }
        }

        /// <summary>
        /// Handles the activation of items in the main menu
        /// </summary>
        private void ActivateMainMenuItem()
        {
            switch (Menu.SelectedIndex)
            {
                case 0:
                    setMenuState(MenuState.GameConfigMenu);
                    break;
                case 1:
                    setMenuState(MenuState.SettingsMenu);
                    break;
                case 2:
                    game.Exit();
                    break;
            }
        }

        /// <summary>
        /// Handles the activation of items in the settings menu
        /// </summary>
        private void ActivateSettingsMenuItem()
        {
            switch (Menu.SelectedIndex)
            {
                case 0: // Change resolution setting
                    Array resolutions = Enum.GetValues(typeof(Resolution));
                    newResolution = (Resolution)GetNextSetting(newResolution, resolutions);
                    settingsMenuItems[0] = RESOLUTION + Utility.ResolutionToString(newResolution);
                    Menu.SetMenuItems(settingsMenuItems);
                    Message = "";
                    break;
                case 1: // Toggle fullscreen setting
                    newFullScreen = !newFullScreen;
                    if (newFullScreen)
                        settingsMenuItems[1] = FULL_SCREEN + "On";
                    else
                        settingsMenuItems[1] = FULL_SCREEN + "Off";
                    Menu.SetMenuItems(settingsMenuItems);
                    Message = "";
                    break;
                case 2: // Change entity limit setting
                    Array values = Enum.GetValues(typeof(Setting));
                    newEntityLimit = (Setting)GetNextSetting(newEntityLimit, values);
                    settingsMenuItems[2] = ENTITY_LIMIT + newEntityLimit.ToString();
                    Menu.SetMenuItems(settingsMenuItems);
                    Message = "";
                    break;
                case 3: // Change gore level setting
                    values = Enum.GetValues(typeof(Setting));
                    newGoreLevel = (Setting)GetNextSetting(newGoreLevel, values);
                    settingsMenuItems[3] = GORE_LEVEL + newGoreLevel.ToString();
                    Menu.SetMenuItems(settingsMenuItems);
                    Message = "";
                    break;
                case 4: // Will never happen. This is an empty menu entry, which is always skipped by IncrementSelectedIndex and DecrementSelectedIndex
                    break;
                case 5: // Apply all settings and store them in settings.ini
                    Resolution oldResolution = Settings.Resolution;
                    Settings.Resolution = newResolution;
                    Settings.FullScreen = newFullScreen;
                    Settings.EntityLimit = newEntityLimit;
                    Settings.GoreLevel = newGoreLevel;
                    int[] resolution = Utility.ResolutionToInts(Settings.Resolution);
                    if (game.GameplayView.InitGraphicsMode(resolution[0], resolution[1], Settings.FullScreen))
                    {
                        Message = "Settings saved.";
                    }
                    else
                    {
                        Settings.Resolution = oldResolution;
                        settingsMenuItems[0] = RESOLUTION + Utility.ResolutionToString(Settings.Resolution);
                        Menu.SetMenuItems(settingsMenuItems);
                        Message = "Your display does not support the selected resolution.\nPlease try another.";
                    }
                    aspectRatio = Utility.GetAspectRatio(); // We need to recalculate the aspect ratio, in case the resolution was changed.
                    Settings.writeSettingsToIniFile();
                    break;
                case 6: // Back to main menu or pause menu
                    switch (menuState)
                    {
                        case MenuState.SettingsMenu:
                            setMenuState(MenuState.MainMenu);
                            break;
                    }
                    Message = "";
                    break;
            }
        }

        /// <summary>
        /// Handles the activation of items in the game configuration menu
        /// </summary>
        private void ActivateGameConfigMenuItem()
        {
            switch (Menu.SelectedIndex)
            {
                case 0: // Change number of players (can be 2, 3 or 4)
                    numberPlayers++;
                    if (numberPlayers > 4) { numberPlayers = 2; }
                    gameConfigMenuItems[0] = NUM_PLAYERS + numberPlayers;
                    Menu.SetMenuItems(gameConfigMenuItems);
                    break;
                case 1: // Change map type
                    Array types = Enum.GetValues(typeof(MapType));
                    mapType = (MapType)GetNextSetting(mapType, types);
                    gameConfigMenuItems[1] = MAP_TYPE + mapType;
                    Menu.SetMenuItems(gameConfigMenuItems);
                    break;
                case 2: // Change map size
                    Array sizes = Enum.GetValues(typeof(MapSize));
                    float mapAspectRatio;

                    // Search forward through the possible map sizes until we find one that has the same
                    // aspect ratio as the current screen resolution setting.
                    string size;
                    do
                    {
                        mapSize = (MapSize)GetNextSetting(mapSize, sizes);
                        size = Utility.MapSizeToString(mapSize);
                        string[] dimensions = size.Split('x');
                        int width = Int16.Parse(dimensions[0]);
                        int height = Int16.Parse(dimensions[1]);
                        mapAspectRatio = (float)width / height;
                    } while (Math.Abs(mapAspectRatio - aspectRatio) > 0.01);

                    gameConfigMenuItems[2] = MAP_SIZE + size;
                    Menu.SetMenuItems(gameConfigMenuItems);
                    break;
                case 3: // Change the number of planets (can be from 0 to 10)
                    numberPlanets++;
                    if (numberPlanets > 10) { numberPlanets = 0; }
                    gameConfigMenuItems[3] = NUM_PLANETS + numberPlanets;
                    Menu.SetMenuItems(gameConfigMenuItems);
                    break;
                case 4: // Change the number of black holes (can be from 0 to 10)
                    numberBlackHoles++;
                    if (numberBlackHoles > 10) { numberBlackHoles = 0; }
                    gameConfigMenuItems[4] = NUM_BLACKHOLES + numberBlackHoles;
                    Menu.SetMenuItems(gameConfigMenuItems);
                    break;
                case 5: // Change the kill limit (in intervals of 10 from 10 to 200)
                    killLimit += 10;
                    if (killLimit > 100) { killLimit = 10; }
                    gameConfigMenuItems[5] = KILL_LIMIT + killLimit;
                    Menu.SetMenuItems(gameConfigMenuItems);
                    Console.WriteLine(killLimit);
                    break;
                case 6: // Will never happen. This is an empty menu entry, which is always skipped by IncrementSelectedIndex and DecrementSelectedIndex
                    break;
                case 7: // Start game
                    Message = "Loading map...";
                    startGame = true;
                    break;
                case 8: // Back to main menu
                    setMenuState(MenuState.MainMenu);
                    break;
            }
        }

        public void setMenuState(MenuState state)
        {
            menuState = state;
            switch (state)
            {
                case MenuState.MainMenu:
                    Menu.SetMenuItems(mainMenuItems);
                    break;
                case MenuState.SettingsMenu:
                    Menu.SetMenuItems(settingsMenuItems);
                    break;
                case MenuState.GameConfigMenu:
                    // Before entering game config menu, set game config variables (and content of game
                    // configuration menu) to default values.
                    // Map size is set by default to the same dimensions as the current screen resolution.
                    numberPlayers = 2;
                    mapType = MapType.Simple;
                    string size = Settings.Resolution.ToString().Split('_')[1]; // Settings.Resolution.ToString() looks like: Res_1024x768
                    mapSize = (MapSize)Enum.Parse(typeof(MapSize), "Size_" + size);
                    numberPlanets = 3;
                    numberBlackHoles = 2;
                    killLimit = 10;
                    gameConfigMenuItems[0] = NUM_PLAYERS + numberPlayers;
                    gameConfigMenuItems[1] = MAP_TYPE + mapType;
                    gameConfigMenuItems[2] = MAP_SIZE + size;
                    gameConfigMenuItems[3] = NUM_PLANETS + numberPlanets;
                    gameConfigMenuItems[4] = NUM_BLACKHOLES + numberBlackHoles;
                    gameConfigMenuItems[5] = KILL_LIMIT + killLimit;

                    Menu.SetMenuItems(gameConfigMenuItems);
                    break;
                case MenuState.PauseMenu:
                    Menu.SetMenuItems(pauseMenuItems);
                    break;
            }
            Menu.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the activation of items in the pause menu
        /// </summary>
        private void ActivatePauseMenuItem()
        {
            switch (Menu.SelectedIndex)
            {
                case 0:
                    game.GoToGameplay();
                    break;
                case 1:
                    setMenuState(MenuState.MainMenu);
                    break;
                case 2:
                    game.Exit();
                    break;
            }
        }

        public List<string> GetMenuItems()
        {
            return Menu.menuItems;
        }

        /// <summary>
        /// Returns the next setting, which is the one that immediately follows currentSetting in values,
        /// or the first setting in values if currentSetting is the last.
        /// </summary>
        /// <param name="newSetting"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private Object GetNextSetting(Enum currentSetting, Array values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values.GetValue(i).Equals(currentSetting))
                {
                    if (i == values.Length - 1)
                    {
                        return values.GetValue(0);
                    }
                    else
                        return values.GetValue(i + 1);
                }
            }
            return currentSetting;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (startGame)
            {
                startGame = false;
                Message = "";
                game.CreateNewGame(mapType, mapSize, numberPlayers, killLimit, numberPlanets, numberBlackHoles);
                game.GoToGameplay();
            }
        }
    }
}