using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.Models
{
    public class BloodParticle : PhysicalObject
    {
        public BloodParticle()
            : base(Sprites.BloodSprite)
        {
            if (Utility.RandomGenerator.NextDouble() > 0.5)
                Spritesheet = Sprites.BloodSprite2;
        }

        public BloodParticle(Texture2D t) : base(t) { }
 
    }
}
