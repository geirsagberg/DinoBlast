using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BunnyLand.Models.Weapons;
using Microsoft.Xna.Framework.Audio;

namespace BunnyLand.Models
{
    public class Projectile : PhysicalObject
    {
        public Player Owner { get; protected set; }
        public AmmoType Ammunition { get; protected set; }
        public float Damage { get { return Ammunition.Damage; } }
        public float BlastRadius { get { return Ammunition.BlastRadius; } }

        public Projectile(AmmoType ammunition, Vector2 position, float exitAngle, float exitSpeed, Player owner)
            : base(ammunition.ProjectileTexture, position)
        {
            Velocity = Utility.ToVector(exitAngle, exitSpeed);
            Owner = owner;
            Ammunition = ammunition;
            Scale = new Vector2(ammunition.Scale);
        }
    }
}
