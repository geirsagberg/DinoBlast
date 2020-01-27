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
using BunnyLand.Views;
using BunnyLand.Models;
using System.Collections;

namespace BunnyLand.Controllers
{
    /// <summary>
    /// The various actions that a player can perform. What each action actually does to the
    /// active model is determined by the methods ActionStarted, ActionStopped
    /// and ActionContinued in the active Model class.
    /// </summary>
    public enum PlayerAction
    {
        MoveLeft,
        MoveRight,
        AimUp,
        AimDown,
        Jump,
        Fire,
        NextWeapon,
        PreviousWeapon,
        Accept,
        Cancel,
        Clear,
        Dig
    }
}
