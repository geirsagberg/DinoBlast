using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BunnyLand.Models.Entities
{
    public class Explosion : GravityPoint
    {
        /// <summary>
        /// After this many frames, the explosion will no longer affect stuff.
        /// </summary>
        /// <value>The frames left to live.</value>
        public int FramesLeftToLive { get; set; }

        public Explosion(float mass)
            : base(Sprites.ExplosionSpritesheet)
        {
            AnimationFrames = Sprites.GetSprites(5, 5, Sprites.ExplosionSpritesheet);
            Mass = mass;
            FramesLeftToLive = 1;
            Origin = Size / 2;
        }

        public bool AnimationFinished()
        {
            if (currentframe == AnimationFrames.Count() - 1)
            {
                return true;
            }
            return false;
        }
    }
}
