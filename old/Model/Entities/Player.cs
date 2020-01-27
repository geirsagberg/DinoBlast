using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BunnyLand.Controllers;
using Microsoft.Xna.Framework;
using System.Collections;

namespace BunnyLand.Models
{
    public enum GroundTypes
    {
        Air,
        Ground,
        Planet
    }

    public class Player : PhysicalObject
    {
        public Profile Profile { get; set; }
        public int Team { get; set; }

        public Rectangle CollisionBox { get; private set; }
        public override bool Enabled
        {
            get { return _Enabled; }
            set
            {
                IsAffectedByGravity = value ? true : false;
                _Enabled = value;
            }
        }


        public Vector2 BodyCenter { get { return (Position + BottomPoint) / 2; } }
        private float _AimingAngle;
        public float AimingAngle
        {
            get
            {
                ////float orientationAngle = Utility.ToAngle(PhysicsEngine.NormalizedRightVector);
                return MathHelper.WrapAngle(IsFacingRight ? Angle + _AimingAngle : MathHelper.Pi - (-Angle + _AimingAngle));
            }
        } //Radians
        public float AimingSpeed { get; set; } //change aim by this much every frame
        private float _AimingMaxAngle; //Lower aiming angle bound
        private float _AimingMinAngle; //Upper aiming angle bound
        public float Health { get; set; }
        public float DefaultHealth { get; set; }
        public List<Weapon> Weapons { get; set; }
        public Weapon CurrentWeapon { get; set; }
        public PlayerIndex PlayerIndex { get; set; }
        private GroundTypes _IsStandingOn;
        /// <summary>
        /// Gets or sets the type of ground the player is standing on.
        /// Automatically sets IsAffectedByGravity accordingly.
        /// </summary>
        /// <value>The ground type.</value>
        public GroundTypes IsStandingOn
        {
            get { return _IsStandingOn; }
            set
            {
                _IsStandingOn = value;
                if (value == GroundTypes.Air)
                    IsAffectedByGravity = true;
                else
                {
                    IsAffectedByGravity = false;
                    Acceleration = Velocity = SelfVelocity = Vector2.Zero;
                }
            }
        }

        public Planet CurrentPlanet { get; set; }

        public float JumpSpeed { get; set; }
        public float JetPackThrust { get; set; }
        /// <summary>
        /// When moving on the ground, player can't climb higher walls than this, 
        /// and higher falls than this will lose contact with ground.
        /// </summary>
        /// <value>The max climbing distance.</value>
        public int MaxClimbingDistance { get; set; }
        public bool IsMovingLeft { get; set; }
        public bool IsMovingRight { get; set; }
        public bool IsAimingUp { get; set; }
        public bool IsAimingDown { get; set; }
        public bool IsJumping { get; set; }
        public bool IsFiring { get; set; }
        public int RespawnTimer { get; set; }


        public Player(Texture2D spriteSheet)
            : base(spriteSheet)
        {
            DefaultHealth = 200;
            MaxSelfSpeed = 2.0f;
            JumpSpeed = 5f;
            JetPackThrust = 0.1f;
            MaxClimbingDistance = 5;
            _AimingAngle = 0f;
            AimingSpeed = (float)Math.PI / 64f;
            _AimingMaxAngle = (float)Math.PI / 2f;
            _AimingMinAngle = -_AimingMaxAngle;
            Weapons = new List<Weapon>();
            IsFiring = false;

            CollisionBox = new Rectangle(12, 25, 11, 24);

            AnimationFrames = Sprites.GetSprites(21, 1, spriteSheet);
            Origin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);

            animationPeriod = 3;
        }

        public Player()
            : this(Sprites.BunnySpritesheet)
        {


        }

        public Player(Texture2D spritesheet, Profile profile)
            : this(spritesheet)
        {
            Profile = profile;
        }

        public void ChangeAimingAngle(float angle)
        {
            _AimingAngle = MathHelper.Clamp(MathHelper.WrapAngle(_AimingAngle + angle), _AimingMinAngle, _AimingMaxAngle);
        }

        public override void Animate()
        {
            animationPeriodCounter++;
            if (animationPeriodCounter == animationPeriod)
            {
                animationPeriodCounter = 0;
                if ((IsMovingLeft ^ !IsMovingRight) && IsStandingOn != GroundTypes.Air)
                {
                    currentframe = 0;
                }
                else if ((IsMovingLeft || IsMovingRight) && IsStandingOn != GroundTypes.Air)
                {
                    if (currentframe < 1 || currentframe > 9)
                        currentframe = 1;
                    else
                    {
                        currentframe++;
                        if (currentframe > 9)
                            currentframe = 1;
                    }
                }
                else if (IsStandingOn == GroundTypes.Air && !IsJumping)
                {
                    if (currentframe < 9 || currentframe > 12)
                        currentframe = 9;
                    else if (currentframe < 12)
                    {
                        currentframe++;
                    }
                }
                else if (IsStandingOn == GroundTypes.Air && IsJumping)
                    if (currentframe < 13 || currentframe > 20)
                        currentframe = 13;
                    else
                    {
                        currentframe++;
                        if (currentframe > 20)
                            currentframe = 13;
                    }
            }
        }

        public void StartRespawnTimer(int timer)
        {
            RespawnTimer = timer;
        }

        public void ResetRespawnTimer()
        {
            RespawnTimer = 0;
        }
    }
}
