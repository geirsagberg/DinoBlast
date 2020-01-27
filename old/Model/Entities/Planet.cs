using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BunnyLand.Models
{
    public class Planet : GravityPoint
    {
        public float Radius { get { return Size.X / 2; } }
        public float MassDensity = 0.001f;
        private Random random;

        public Planet(Texture2D spriteSheet)
            : base(spriteSheet)
        {
            CalculateMass();
        }

        public Planet(Texture2D spriteSheet, float density, float scale)
            : base(spriteSheet)
        {
            MassDensity = density;
            Scale = new Vector2(scale);
            CalculateMass();
        }

        /// <summary>
        /// This constructor creates a planet with a texture chosen at random from the available planet textures.
        /// Density and scale are randomized within the limits of the given parameters.
        /// </summary>
        /// <param name="minDensity"></param>
        /// <param name="maxDensity"></param>
        /// <param name="minScale"></param>
        /// <param name="maxScale"></param>
        public Planet(float minDensity, float maxDensity, float minScale, float maxScale)
            : base(Sprites.Planets.ElementAt(Utility.Random.Next(Sprites.Planets.Count)))
        {
            //random = new Random();
            //int index = random.Next(Sprites.Planets.Count);
            //Spritesheet = Sprites.Planets.ElementAt(index);
            MassDensity = Utility.RandomFloat(minDensity, maxDensity);
            Scale = new Vector2(Utility.RandomFloat(minScale, maxScale));
            CalculateMass();
        }

        public void CalculateMass()
        {
            Mass = MassDensity * (4f / 3f) * MathHelper.Pi * (float)Math.Pow(Radius, 2);
        }
    }
}
