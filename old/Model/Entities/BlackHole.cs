using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BunnyLand.Models
{
    public class BlackHole : GravityPoint
    {

        /// <summary>
        /// Gets or sets the radius at which objects are sucked into the black hole.
        /// </summary>
        /// <value>The disappearance radius.</value>
        public float DisappearanceRadius { get { return Mass / 50; } }

        public BlackHole(float mass)
            : base(Sprites.BlackHoleSprite)
        {
            Rotation = MathHelper.Pi / (mass/10);  // Heavy black holes rotate more slowly
            Mass = mass;
            Scale = new Vector2(mass / 3000);   // Heavy black holes are larger
        }

        /// <summary>
        /// Creates a black hole with random mass, within the limits of the given parameters.
        /// </summary>
        public BlackHole(float minMass, float maxMass)
            : base(Sprites.BlackHoleSprite)
        {
            Mass = Utility.RandomFloat(minMass, maxMass);
            Rotation = MathHelper.Pi / (Mass / 10);  // Heavy black holes rotate more slowly
            Scale = new Vector2(Mass / 3000);   // Heavy black holes are larger
        }
    }
}
