using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Globalization;

namespace BunnyLand.Models.Weapons
{
    public enum WeaponTextureTypes
    {
        MP5,
        AG3,
        M16,
        shotgun,
        SMG,
        pistol
    }

    public class HandgunWeapon : Weapon
    {
        private int _NofProjectiles;
        public int NofProjectiles
        {
            get { return _NofProjectiles; }
            set { _NofProjectiles = Math.Max(1, value); }
        }
        public float SpreadAngle { get; set; }
        public float ExitSpeedVariance { get; set; }


        public HandgunWeapon(string name, int reloadTime, int rateOfFire, int ammoCapacity, AmmoType ammunition, bool isAutomatic, float projectileSpeed, float exitSpeedVariance, int nofProjectiles, float spreadAngle, WeaponTextureTypes weaponTextureType, SoundEffect sfx_FireWeapon)
            : base(name, reloadTime, rateOfFire, ammoCapacity, ammunition, isAutomatic, projectileSpeed, Sprites.HandgunWeaponSpritesheet, sfx_FireWeapon)
        {
            WeaponTextureType = weaponTextureType;
            SourceRect = Sprites.GetSprites(1, 6, Sprites.HandgunWeaponSpritesheet)[(int)weaponTextureType];
            NofProjectiles = nofProjectiles;
            SpreadAngle = spreadAngle;
            ExitSpeedVariance = exitSpeedVariance;
        }

        public override Projectile[] Fire(float angle, Player owner)
        {
            Projectile[] projectiles = new Projectile[NofProjectiles];
            Random rand = new Random();
            Vector2 exitPosition = owner.BodyCenter;
            for (int i = 0; i < NofProjectiles; i++)
            {
                float exitAngle = angle + (2 * (float)rand.NextDouble() - 1) * SpreadAngle;
                float exitSpeed = owner.TotalVelocity.Length() + ProjectileExitSpeed + (2 * (float)rand.NextDouble() - 1) * ExitSpeedVariance;
                projectiles[i] = new Projectile(Ammunition, exitPosition, exitAngle, exitSpeed, owner);
            }
            return projectiles;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name + "|");
            sb.Append(ReloadTime + "|");
            sb.Append(RateOfFire + "|");
            sb.Append(AmmoCapacity + "|");
            sb.Append(Ammunition.Damage.ToString(CultureInfo.InvariantCulture) + "|");
            sb.Append(Ammunition.BlastRadius.ToString(CultureInfo.InvariantCulture) + "|");
            sb.Append(Ammunition.IsExplosive + "|");
            sb.Append(IsAutomatic + "|");
            sb.Append(ProjectileExitSpeed.ToString(CultureInfo.InvariantCulture) + "|");
            sb.Append(ExitSpeedVariance.ToString(CultureInfo.InvariantCulture) + "|");
            sb.Append(NofProjectiles + "|");
            sb.Append(SpreadAngle.ToString(CultureInfo.InvariantCulture) + "|");
            sb.Append(Ammunition.Scale.ToString(CultureInfo.InvariantCulture) + "|");
            sb.Append(WeaponTextureType.ToString() + "|");
            sb.Append(WeaponTextureType.ToString());

            return sb.ToString();
        }

        public static Weapon WeaponFromString(string s)
        {
            string[] strings = s.Split('|');
            foreach (string str in strings)
                str.Trim();
            string name = strings[0];
            int reloadTime;
            if (!int.TryParse(strings[1], NumberStyles.Any, CultureInfo.InvariantCulture, out reloadTime))
                throw new FormatException("reload time must be integer!");
            int rateOfFire;
            if (!int.TryParse(strings[2], NumberStyles.Any, CultureInfo.InvariantCulture, out rateOfFire))
                throw new FormatException("rate of fire must be integer!");
            int ammoCapacity;
            if (!int.TryParse(strings[3], NumberStyles.Any, CultureInfo.InvariantCulture, out ammoCapacity))
                throw new FormatException("ammo capacity must be integer!");
            float damage;
            if (!float.TryParse(strings[4], NumberStyles.Any, CultureInfo.InvariantCulture, out damage))
                throw new FormatException("damage must be float!");
            float blastRadius;
            if (!float.TryParse(strings[5], NumberStyles.Any, CultureInfo.InvariantCulture, out blastRadius))
                throw new FormatException("blast radius must be float!");
            bool isExplosive;
            if (!bool.TryParse(strings[6], out isExplosive))
                throw new FormatException("isExplosive must be boolean!");
            bool isAutomatic;
            if (!bool.TryParse(strings[7], out isAutomatic))
                throw new FormatException("isAutomatic must be boolean!");
            float projectileSpeed;
            if (!float.TryParse(strings[8], NumberStyles.Any, CultureInfo.InvariantCulture, out projectileSpeed))
                throw new FormatException("projectile speed must be float!");
            float exitSpeedVariance;
            if (!float.TryParse(strings[9], NumberStyles.Any, CultureInfo.InvariantCulture, out exitSpeedVariance))
                throw new FormatException("exit speed variance must be float!");
            int nofProjectiles;
            if (!int.TryParse(strings[10], NumberStyles.Any, CultureInfo.InvariantCulture, out nofProjectiles))
                throw new FormatException("nofProjectiles must be integer!");
            float spreadAngle;
            if (!float.TryParse(strings[11], NumberStyles.Any, CultureInfo.InvariantCulture, out spreadAngle))
                throw new FormatException("spreadAngle must be float!");
            float scale;
            if (!float.TryParse(strings[12], NumberStyles.Any, CultureInfo.InvariantCulture, out scale))
                throw new FormatException("scale must be float!");
            string sfx = strings[13];
            SoundEffect sfx_FireWeapon;
            //Workaround use contains
            if (sfx.Contains("shotgun")) 
            {
                sfx_FireWeapon = Audio.sfx_shotgun1;
            }
            else if (sfx.Contains("pistol"))
            {
                sfx_FireWeapon = Audio.sfx_shotgun1;
            }
            else if (sfx.Contains("smg"))
            {
                sfx_FireWeapon = Audio.sfx_mgun1;
            }
            else if (sfx.Contains("grenadeLauncher"))
            {
                sfx_FireWeapon = Audio.sfx_shotgun1;
            }
            else
            {
                sfx_FireWeapon = Audio.sfx_shotgun1;
            }
            WeaponTextureTypes weaponTextureType = (WeaponTextureTypes)Enum.Parse(typeof(WeaponTextureTypes), strings[14], true);



            AmmoType ammunition = new AmmoType(Sprites.BulletSprite, damage, blastRadius, scale, isExplosive);
            HandgunWeapon hw = new HandgunWeapon(name, reloadTime, rateOfFire, ammoCapacity, ammunition,
                isAutomatic, projectileSpeed, exitSpeedVariance, nofProjectiles, spreadAngle, weaponTextureType, sfx_FireWeapon);
            return hw;
        }
    }
}
