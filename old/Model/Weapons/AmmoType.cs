using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.Models.Weapons
{
    public class AmmoType
    {
        public float Damage { get; set; }
        public float BlastRadius { get; set; }
        public bool IsExplosive { get; set; }
        public float Scale { get; set; }
        public Texture2D ProjectileTexture { get; protected set; }

        public AmmoType(Texture2D texture, float damage, float blastRadius, float scale, bool isExplosive)
        {
            ProjectileTexture = texture;
            Damage = damage;
            BlastRadius = blastRadius;
            IsExplosive = isExplosive;
            Scale = scale;

        }
    }
}
