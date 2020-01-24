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
using BunnyLand.Views;


namespace BunnyLand.Models
{
    /// <summary>
    /// This is the base class for all game models.
    /// </summary>
    public abstract class AbstractModel : Microsoft.Xna.Framework.GameComponent
    {
        public AbstractModel(Game game)
            : base(game)
        {
            Enabled = false;
        }

        /// <summary>
        /// Stop the model from updating itself
        /// </summary>
        public void Disable()
        {
            Enabled = false;
        }

        /// <summary>
        /// Make the model start/continue to update itself
        /// </summary>
        public void Enable()
        {
            Enabled = true;
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

        #region ActionHandling

        /// <summary>
        /// The controller calls this method to indicate that a particular action was started
        /// by a particular player. Each model (GameplayModel, various MenuModels) will have
        /// it's own specific implementation of this method.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="action"></param>
        public abstract void ActionStarted(PlayerIndex i, PlayerAction action);

        /// <summary>
        /// The controller calls this method to indicate that a particular action was stopped
        /// by a particular player. Each model (GameplayModel, various MenuModels) will have
        /// it's own specific implementation of this method.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="action"></param>
        public abstract void ActionStopped(PlayerIndex i, PlayerAction action);

        public abstract void ActionContinued(PlayerIndex i, PlayerAction action);

        #endregion

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}