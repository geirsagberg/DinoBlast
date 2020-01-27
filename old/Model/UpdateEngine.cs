using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BunnyLand.Models;
using BunnyLand.Models.Entities;

namespace BunnyLand.Models
{
    public class UpdateEngine
    {
        private GameplayModel Model { get; set; }

        public UpdateEngine(GameplayModel model)
        {
            Model = model;
        }

        #region UpdateLogic
        /* Calculates gravity, and updates all objects position and angle by their acceleration and angle */
        public void UpdateWorld()
        {
            //Calculate physics and move objects, etc.
            UpdateExplosions();
            UpdateAnimations();
            UpdateAngle();
            UpdateGravity();
            UpdatePosition();
        }

        private void UpdateExplosions()
        {
            for (int i = 0; i < Model.ExplosionList.Count; i++)
            {
                Explosion e = Model.ExplosionList.ElementAt(i);
                if (e.FramesLeftToLive > 0)
                {
                    e.FramesLeftToLive--;
                }
                else
                {
                    e.Mass = 0;
                    if (e.AnimationFinished())
                    {
                        Model.Remove(e);
                    }
                }
            }
        }

        private void UpdateAnimations()
        {
            foreach (Entity e in Model.EntityList)
            {
                e.Animate();
            }
        }
        /* Update angle of entity with rotation speed */
        private void UpdateAngle()
        {
            foreach (Entity e in Model.EntityList)
            {
                e.Angle = MathHelper.WrapAngle(e.Angle - e.Rotation);
            }
        }
        /* Update velocity and position of entities */
        private void UpdatePosition()
        {
            UpdatePhysicalObjects();
            UpdatePlayers();
        }
        /* Iterates through all entites and updates Velocity and Position */
        private void UpdatePhysicalObjects()
        {
            foreach (PhysicalObject po in Model.PhysicalObjectList)
            {
                po.Velocity += po.TotalAcceleration;
                po.Position += po.TotalVelocity;
            }
        }
        /* Iterates through all players and updates movement and aiming angle */
        private void UpdatePlayers()
        {
            if (!Model.GameOver)
            {
                foreach (Player p in Model.PlayerList)
                {
                    if (p.Enabled)
                    {
                        if (p.IsJumping)
                        {
                            if (p.IsStandingOn == GroundTypes.Ground)
                            {
                                p.IsStandingOn = GroundTypes.Air;
                                p.Velocity += PhysicsEngine.NormalizedUpVector * p.JumpSpeed;
                                p.CanCollideWithTerrain = false;
                                p.IsJumping = false;
                            }
                            else if (p.IsStandingOn == GroundTypes.Planet)
                            {
                                p.IsStandingOn = GroundTypes.Air;
                                Vector2 delta = p.Position - p.CurrentPlanet.Position;
                                float angle = Utility.ToAngle(delta);
                                p.Velocity += Utility.ToVector(angle, p.JumpSpeed);
                                p.IsJumping = false;
                            }
                            else if (p.IsStandingOn == GroundTypes.Air)
                            {
                                p.SelfAcceleration = PhysicsEngine.NormalizedUpVector * p.JetPackThrust;
                            }
                        }
                        else
                        {
                            p.SelfAcceleration = Vector2.Zero;
                        }

                        /* Update aiming */
                        if (p.IsAimingUp)
                            p.ChangeAimingAngle(-p.AimingSpeed);
                        if (p.IsAimingDown)
                            p.ChangeAimingAngle(p.AimingSpeed);

                        /* Update firing */
                        if (p.CurrentWeapon != null)
                        {
                            if (p.CurrentWeapon.IsReloading)
                            {
                                p.CurrentWeapon.ReloadTimeCounter--;
                                if (p.CurrentWeapon.ReloadTimeCounter <= 0)
                                {
                                    p.CurrentWeapon.IsReloading = false;
                                    p.CurrentWeapon.CurrentAmmo = p.CurrentWeapon.AmmoCapacity;
                                }
                            }
                            else
                            {

                                if (p.CurrentWeapon.RateOfFireCounter > 0)
                                    p.CurrentWeapon.RateOfFireCounter--;
                                else if (p.IsFiring)
                                {
                                    p.CurrentWeapon.RateOfFireCounter = p.CurrentWeapon.RateOfFire;
                                    Model.PlayerFiredWeapon(p);
                                    if (!p.CurrentWeapon.IsAutomatic)
                                        p.IsFiring = false;
                                }
                            }
                        }

                        /* Update movement */
                        if (p.IsMovingLeft ^ !p.IsMovingRight) // Either both or neither (logical XNOR)
                            p.SelfVelocity = Vector2.Zero;
                        else if (p.IsMovingLeft)
                        {
                            p.SelfVelocity = Utility.ToVector(p.Angle + MathHelper.Pi, p.MaxSelfSpeed);
                        }
                        else if (p.IsMovingRight)
                        {
                            p.SelfVelocity = Utility.ToVector(p.Angle, p.MaxSelfSpeed);
                        }


                        // Make player land on his feet
                        float rightAngle = Utility.ToAngle(PhysicsEngine.NormalizedRightVector);
                        if (p.IsStandingOn == GroundTypes.Air && p.Angle != rightAngle)
                        {
                            if (Math.Abs(p.Angle - rightAngle) < MathHelper.Pi / 16f)
                            {
                                p.Angle = rightAngle;
                            }
                            else
                            { //Rotate player clockwise till he lines up with the gravityfield
                                p.Angle = MathHelper.WrapAngle(p.Angle + MathHelper.Pi / 32f);
                            }
                        }
                    }
                    else if (p.RespawnTimer > 0)
                        p.RespawnTimer--;
                    else
                    {
                        p.Enabled = true;
                        Model.PlacePlayer(p);
                    }
                }
            }
        }

        /* Iterates over all physical objects and calculates the acceleration changes
         * caused by gravity points and constant gravity */
        private void UpdateGravity()
        {
            Vector2 v;
            foreach (PhysicalObject po in Model.PhysicalObjectList)
            {
                if (po.IsAffectedByGravity)
                {
                    Vector2 resultVector = Vector2.Zero;
                    foreach (GravityPoint gp in Model.GravityPointList)
                    {
                        v = PhysicsEngine.CalculateGravity(po, gp);
                        resultVector += v;
                        if (BunnyGame.DebugMode) //Draw vector as green line
                        {
                            Model.View.DrawVector(po.Position, v, 1000, Color.Green);
                        }
                    }
                    resultVector += PhysicsEngine.GravityField; //constant gravity acceleration
                    po.Acceleration = resultVector;
                    if (BunnyGame.DebugMode) //Draw result vector as yellow line
                    {
                        Model.View.DrawVector(po.Position, PhysicsEngine.GravityField, 1000, Color.Blue);
                        Model.View.DrawVector(po.Position, resultVector, 1000, Color.Yellow);
                    }
                }
            }
        }
        #endregion

    }
}
