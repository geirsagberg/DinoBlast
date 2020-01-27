using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using BunnyLand.Models.Weapons;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;

namespace BunnyLand.Models
{
    public abstract class Weapon
    {
        public SoundEffect SFX_FireWeapon { get; set; }

        public Texture2D WeaponTexture { get; set; }


        /// <summary>
        /// Used for converting to and from strings.
        /// </summary>
        /// <value>The type of the weapon texture.</value>
        public WeaponTextureTypes WeaponTextureType { get; protected set; }
        public Rectangle SourceRect { get; protected set; }

        public Vector2 Origin { get { return new Vector2(SourceRect.Width / 2, 4); } }

        private static float _GlobalReloadTimeMultiplier = 1f;
        public static float GlobalReloadTimeMultiplier
        {
            get { return Weapon._GlobalReloadTimeMultiplier; }
            set
            {
                if (value < 0)
                    throw new IndexOutOfRangeException("Reload multiplier must be positive!");
                Weapon._GlobalReloadTimeMultiplier = value;
            }
        }

        private int _ReloadTime;
        /// <summary>
        /// Number of frames it takes to reload.
        /// </summary>
        /// <value>The reload time.</value>
        public int ReloadTime
        {
            get { return (int)(_ReloadTime * _GlobalReloadTimeMultiplier); }
            protected set { _ReloadTime = value; }
        }
        /// <summary>
        /// The frames left till the weapon is reloaded.
        /// </summary>
        /// <value>The time left to reload.</value>
        public int ReloadTimeCounter { get; set; }
        /// <summary>
        /// Number of frames to wait between each shot.
        /// </summary>
        /// <value>The rate of fire.</value>
        public int RateOfFire { get; protected set; }
        /// <summary>
        /// The frames left till a new round is fired.
        /// </summary>
        /// <value>The rate of fire counter.</value>
        public int RateOfFireCounter { get; set; }
        public int AmmoCapacity { get; protected set; }
        public int CurrentAmmo { get; set; }
        public bool IsAutomatic { get; protected set; }
        public bool IsReloading { get; set; }
        public string Name { get; protected set; }
        private float _ProjectileExitSpeed;
        public float ProjectileExitSpeed
        {
            get { return _ProjectileExitSpeed; }
            set { _ProjectileExitSpeed = Math.Abs(value); }
        }
        public AmmoType Ammunition { get; protected set; }

        public Weapon(string name, int reloadTime, int rateOfFire, int ammoCapacity, AmmoType ammunition, bool isAutomatic, float exitSpeed, Texture2D weaponSpritesheet, SoundEffect sfx_fireWeapon)
        {
            SFX_FireWeapon = sfx_fireWeapon;
            ReloadTime = reloadTime;
            ProjectileExitSpeed = exitSpeed;
            Ammunition = ammunition;
            RateOfFire = rateOfFire;
            AmmoCapacity = ammoCapacity;
            IsAutomatic = isAutomatic;
            IsReloading = false;
            CurrentAmmo = ammoCapacity;
            ReloadTimeCounter = 0;
            RateOfFireCounter = 0;
            Name = name;
            WeaponTexture = weaponSpritesheet;
        }

        public static void WriteWeaponsToIniFile(List<Weapon> weapons)
        {
            TextWriter writer = new StreamWriter("weapons.ini");
            writer.WriteLine("# name,			reloadTime,	rateOfFire,	ammoCapacity,	damage,		blastRadius,	isExplosive,	isAutomatic,	projectileSpeed,	exitSpeedVariance,	nofProjectiles,	spreadAngle,	scale,	soundEffect,		weaponTextureType");
            foreach (Weapon w in weapons)
            {
                writer.WriteLine(w.ToString());
            }
            writer.Close();
        }

        public abstract Projectile[] Fire(float angle, Player owner);
    }
}
