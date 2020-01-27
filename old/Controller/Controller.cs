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
using System.Collections;


namespace BunnyLand.Controllers
{
    /// <summary>
    /// This class implements the controller for the game in action
    /// </summary>
    public class Controller : GameComponent
    {
        public const int MaxInputs = 4;
        private Hashtable[] Controls = new Hashtable[MaxInputs];

        // The GameplayModel controlled by this GameplayController
        public AbstractModel Model { get; set; }

        // Keyboard state
        KeyboardState lastKeyboardState = new KeyboardState(), currentKeyboardState = new KeyboardState();
        GamePadState[] lastGamePadState = new GamePadState[MaxInputs], currentGamePadState = new GamePadState[MaxInputs];

        public Controller(Game game, AbstractModel model)
            : base(game)
        {
            Enabled = false;

            Model = model;
            for (int i = 0; i < MaxInputs; i++)
            {
                Controls[i] = new Hashtable();
            }
            //keyboard controls
            Controls[0][Keys.A] = PlayerAction.MoveLeft;
            Controls[0][Keys.D] = PlayerAction.MoveRight;
            Controls[0][Keys.W] = PlayerAction.AimUp;
            Controls[0][Keys.S] = PlayerAction.AimDown;
            Controls[0][Keys.C] = PlayerAction.Jump;
            Controls[0][Keys.V] = PlayerAction.Fire;
            Controls[0][Keys.E] = PlayerAction.NextWeapon;
            Controls[0][Keys.Q] = PlayerAction.PreviousWeapon;
            Controls[0][Keys.B] = PlayerAction.Dig;
            Controls[0][Keys.LeftControl] = PlayerAction.Accept;
            Controls[0][Keys.LeftShift] = PlayerAction.Cancel;

            Controls[1][Keys.Left] = PlayerAction.MoveLeft;
            Controls[1][Keys.Right] = PlayerAction.MoveRight;
            Controls[1][Keys.Up] = PlayerAction.AimUp;
            Controls[1][Keys.Down] = PlayerAction.AimDown;
            Controls[1][Keys.OemPeriod] = PlayerAction.Jump;
            Controls[1][Keys.OemComma] = PlayerAction.Fire;
            Controls[1][Keys.L] = PlayerAction.NextWeapon;
            Controls[1][Keys.K] = PlayerAction.PreviousWeapon;
            Controls[1][Keys.M] = PlayerAction.Dig;
            Controls[1][Keys.Enter] = PlayerAction.Accept;
            Controls[1][Keys.Escape] = PlayerAction.Cancel;

            // Gamepad controls
            foreach (Hashtable ht in Controls)
            {
                ht[Buttons.LeftThumbstickLeft] = PlayerAction.MoveLeft;
                ht[Buttons.LeftThumbstickRight] = PlayerAction.MoveRight;
                ht[Buttons.LeftThumbstickUp] = PlayerAction.AimUp;
                ht[Buttons.LeftThumbstickDown] = PlayerAction.AimDown;
                ht[Buttons.RightThumbstickLeft] = PlayerAction.MoveLeft;
                ht[Buttons.RightThumbstickRight] = PlayerAction.MoveRight;
                ht[Buttons.RightThumbstickUp] = PlayerAction.AimUp;
                ht[Buttons.RightThumbstickDown] = PlayerAction.AimDown;
                ht[Buttons.DPadLeft] = PlayerAction.MoveLeft;
                ht[Buttons.DPadRight] = PlayerAction.MoveRight;
                ht[Buttons.DPadUp] = PlayerAction.AimUp;
                ht[Buttons.DPadDown] = PlayerAction.AimDown;
                ht[Buttons.A] = ht[Buttons.LeftTrigger] = PlayerAction.Jump;
                ht[Buttons.RightTrigger] = PlayerAction.Fire;
                ht[Buttons.LeftShoulder] = PlayerAction.PreviousWeapon;
                ht[Buttons.RightShoulder] = PlayerAction.NextWeapon;
                ht[Buttons.Start] = PlayerAction.Accept;
                ht[Buttons.B] = PlayerAction.Cancel;
                ht[Buttons.X] = PlayerAction.Dig;
            }
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

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        private void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            for(int i = 0; i < MaxInputs; i++){
                lastGamePadState[i] = currentGamePadState[i];
                currentGamePadState[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.IndependentAxes);
            }

            #region Testing
            if (BunnyGame.DebugMode)
            {
                // For testing of new game initialization
                if (IsKeyPressed(Keys.N))
                    ((GameplayModel)Model).CreateNewGame(MapType.Simple, MapSize.Size_1024x768, 2, 10, 3, 2);
                else if (IsKeyPressed(Keys.M))
                    ((GameplayModel)Model).CreateNewGame(MapType.Complex, MapSize.Size_1280x800, 2, 10, 3, 2);
                /*
                else if (IsKeyPressed(Keys.B))
                {
                    Color[] colors = new Color[Sprites.TestMap.Width * Sprites.TestMap.Height];
                    Sprites.TestMap.GetData<Color>(colors);
                    ((GameplayModel)Model).NewGameTest(new CustomTerrain(Sprites.TestMap.Width, Sprites.TestMap.Height, colors));
                }
                 */

                // For testing of gravity field
                if (IsKeyPressed(Keys.D1))
                    PhysicsEngine.GravityField = new Vector2(0, 0.1f);
                if (IsKeyPressed(Keys.D2))
                    PhysicsEngine.GravityField = new Vector2(0.1f, 0);
                if (IsKeyPressed(Keys.D3))
                    PhysicsEngine.GravityField = new Vector2(0, -0.1f);
                if (IsKeyPressed(Keys.D4))
                    PhysicsEngine.GravityField = new Vector2(-0.1f, 0);
                if (IsKeyPressed(Keys.D5))
                    PhysicsEngine.GravityField = new Vector2(-0.1f, 0.1f);
                /*
                // For testing camera
                float cameraSpeed = 5f;
                float zoomSpeed = 0.01f;
                float rotationSpeed = MathHelper.Pi / 64;
                if (IsKeyHeld(Keys.A))
                    ((GameplayView)View).Camera.Velocity += new Vector2(-cameraSpeed, 0);
                if (IsKeyHeld(Keys.D))
                    ((GameplayView)View).Camera.Velocity += new Vector2(cameraSpeed, 0);
                if (IsKeyHeld(Keys.W))
                    ((GameplayView)View).Camera.Velocity += new Vector2(0, -cameraSpeed);
                if (IsKeyHeld(Keys.S))
                    ((GameplayView)View).Camera.Velocity += new Vector2(0, cameraSpeed);
                if (IsKeyHeld(Keys.E))
                    ((GameplayView)View).Camera.Zoom += zoomSpeed;
                if (IsKeyHeld(Keys.Q))
                    ((GameplayView)View).Camera.Zoom -= zoomSpeed;
                if (IsKeyHeld(Keys.Z))
                    ((GameplayView)View).Camera.RotationVelocity += rotationSpeed;
                if (IsKeyHeld(Keys.C))
                    ((GameplayView)View).Camera.RotationVelocity -= rotationSpeed;
                 */
            }
            #endregion

            for (int i = 0; i < MaxInputs; i++)
            {
                HandleGamePadInput((PlayerIndex)i);
                HandleKeyboardInput((PlayerIndex)i);
            }
        }

        /// <summary>
        /// Handles the game pad input for a player.
        /// </summary>
        /// <param name="p">The player.</param>
        private void HandleGamePadInput(PlayerIndex pi)
        {
            int i = (int)pi;
            foreach (Buttons b in Controls[i].Keys.OfType<Buttons>())
            {
                if (IsButtonPressed(b, i))
                    Model.ActionStarted(pi, (PlayerAction)(Controls[i][b]));
                else if (IsButtonReleased(b, i))
                    Model.ActionStopped(pi, (PlayerAction)(Controls[i][b]));
                else if (IsButtonHeld(b,i))
                    Model.ActionContinued(pi, (PlayerAction)(Controls[i][b]));
            }
        }

        /// <summary>
        /// Handles the keyboard input for a given player.
        /// </summary>
        /// <param name="p">The player.</param>
        private void HandleKeyboardInput(PlayerIndex pi)
        {
            int i = (int)pi;
            foreach (Keys k in Controls[i].Keys.OfType<Keys>())
            {
                if (IsKeyPressed(k))
                    Model.ActionStarted(pi, (PlayerAction)(Controls[i][k]));
                else if (IsKeyReleased(k))
                    Model.ActionStopped(pi, (PlayerAction)(Controls[i][k]));
                else if (IsKeyHeld(k))
                    Model.ActionContinued(pi, (PlayerAction)(Controls[i][k]));
            }
        }
        /* Returns true if button was just pressed */
        private bool IsButtonPressed(Buttons b, int playerIndex)
        {
            return lastGamePadState[playerIndex].IsButtonUp(b) && currentGamePadState[playerIndex].IsButtonDown(b);
        }
        /* Returns true if button is being held down */
        private bool IsButtonHeld(Buttons b, int playerIndex)
        {
            return lastGamePadState[playerIndex].IsButtonDown(b) && currentGamePadState[playerIndex].IsButtonDown(b);
        }
        /* Returns true if button was just released */
        private bool IsButtonReleased(Buttons b, int playerIndex)
        {
            return lastGamePadState[playerIndex].IsButtonDown(b) && currentGamePadState[playerIndex].IsButtonUp(b);
        }

        /* Returns true if button was just pressed */
        private bool IsKeyPressed(Keys k)
        {
            return lastKeyboardState.IsKeyUp(k) && currentKeyboardState.IsKeyDown(k);
        }
        /* Returns true if button is being held down */
        private bool IsKeyHeld(Keys k)
        {
            return lastKeyboardState.IsKeyDown(k) && currentKeyboardState.IsKeyDown(k);
        }
        /* Returns true if button was just released */
        private bool IsKeyReleased(Keys k)
        {
            return lastKeyboardState.IsKeyDown(k) && currentKeyboardState.IsKeyUp(k);
        }
    }
}