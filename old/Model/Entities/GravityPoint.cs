using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.Models
{
    public abstract class GravityPoint : Entity
    {
        public float Mass { get; set; }

        public GravityPoint(Texture2D spriteSheet)
            : base(spriteSheet)
        {

        }

    }
}
