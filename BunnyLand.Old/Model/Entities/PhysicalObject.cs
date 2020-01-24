using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.Models
{
    public abstract class PhysicalObject : Entity
    {
        public bool IsAffectedByGravity { get; set; }
        public bool CollidesWithOtherObjects { get; protected set; }
        public bool CanCollideWithTerrain { get; set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 SelfAcceleration { get; set; }
        public Vector2 TotalAcceleration { get { return Acceleration + SelfAcceleration; } }
        public Vector2 Velocity { get; set; }
        /// <summary>
        /// Gets or sets the velocity caused by the object moving by itself, e.g. walking.
        /// </summary>
        /// <value>The self velocity.</value>
        public Vector2 SelfVelocity { get; set; }
        public Vector2 TotalVelocity { get { return Velocity + SelfVelocity; } }
        public float MaxSelfSpeed { get; set; } //Max self-movement speed in pixels per frame

        public PhysicalObject(Texture2D spritesheet, Vector2 position)
            : base(spritesheet, position)
        {
            CollidesWithOtherObjects = true;
            CanCollideWithTerrain = true;
            IsAffectedByGravity = true;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            SelfVelocity = Vector2.Zero;
            SelfAcceleration = Vector2.Zero;
            MaxSelfSpeed = 0f;
        }

        public PhysicalObject(Texture2D spritesheet)
            : this(spritesheet, Vector2.Zero)
        {
            
        }
    }
}
