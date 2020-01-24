using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BunnyLand.Models;

namespace BunnyLand.Models
{
    public class CollisionEngine
    {


        private GameplayModel model;

        public CollisionEngine(GameplayModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Checks collisions for all physical objects and updates game state accordingly.
        /// </summary>
        public void CheckCollisions()
        {
            CalculateProjectileCollisions();
            CalculateBloodCollisions();
            CalculatePlayerCollisions();
        }

        /// <summary>
        /// Calculates collisions between player and terrain.
        /// Does different checks depending on if the player is on the ground, 
        /// in the air or on a planet.
        /// </summary>
        private void CalculatePlayerCollisions()
        {
            foreach (Player p in model.PlayerList)
            {
                if (p.Enabled)
                {
                    if (BunnyGame.DebugMode)
                        model.View.DrawRectangle(p.BoundingBox, Color.Blue);
                    //CheckBorderCollision(p);
                    if (p.IsStandingOn == GroundTypes.Ground)
                    {
                        CheckGroundCollision(p);
                    }
                    else if (p.IsStandingOn == GroundTypes.Air)
                    {
                        CheckAirCollision(p);
                    }
                    else if (p.IsStandingOn == GroundTypes.Planet)
                    {
                        CheckPlanetCollision(p);
                    }
                    if (CollidesWithBlackHole(p))
                    {
                        model.PlayerMeetsBlackHole(p);
                    }
                }
            }
        }

        private void CheckPlanetCollision(Player p)
        {
            Planet planet = CollidesWithPlanet(p);
            if (planet != null)
            {
                p.CurrentPlanet = planet;
            }
            /* If on planet, set correct distance and angle */
            Vector2 delta = p.Position - p.CurrentPlanet.Position;
            float angle = Utility.ToAngle(delta);
            Vector2 rightVector = new Vector2(-delta.Y, delta.X);
            p.Position = p.CurrentPlanet.Position + Utility.ToVector(angle, p.CurrentPlanet.Radius + p.Origin.Y);
            p.Angle = Utility.ToAngle(rightVector);

            if (CollidesWithTerrain(p))
            {
                PlacePlayerOnGround(p);
            }
            if (CollidesWithBorders(p))
            {
                PlacePlayerInsideLevel(p);
            }

        }

        private bool CollidesWithTerrain(PhysicalObject po)
        {
            return IntersectPixels(po.WorldTransform, po.SourceRect.Width, po.SourceRect.Height, po.ColorData, model.Terrain);
        }

        private bool CollidesWithBlackHole(PhysicalObject po)
        {
            foreach (BlackHole bh in model.BlackHoleList)
            {
                if (Vector2.Distance(bh.Position, po.Position) <= bh.DisappearanceRadius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the player hits the ground or a planet.
        /// </summary>
        /// <param name="p">The player.</param>
        private void CheckAirCollision(Player p)
        {
            if (CollidesWithBorders(p))
            {
                PlacePlayerInsideLevel(p);
                if (PhysicsEngine.CollidesWithBottomBorder(p, model.Terrain))
                    p.IsStandingOn = GroundTypes.Ground;
            }

            p.Position += p.TotalVelocity; //Check if next position collides with terrain
            bool collision = CollisionEngine.IntersectPixels(p.WorldTransform, (int)p.Size.X, (int)p.Size.Y, p.ColorData, model.Terrain);
            //bool collision = CollisionEngine.BoundingBoxTerrainCollision(p.BoundingBox, model.Terrain);
            if (collision) //check player vs terrain
            {
                Rectangle bottomHalf = PhysicsEngine.GetBottomHalf(p.BoundingBox);
                if (BunnyGame.DebugMode)
                    model.View.DrawRectangle(bottomHalf, Color.Green);
                bool bottomCollision = CollisionEngine.BoundingBoxTerrainCollision(bottomHalf, model.Terrain);
                p.Position -= p.TotalVelocity;
                if (bottomCollision)
                { //bottom half collides with terrain
                    PlacePlayerOnGround(p);
                }
                else
                { //top half collides with terrain
                    p.SelfVelocity = -p.SelfVelocity;
                    p.Velocity = -p.Velocity;
                }
            }
            else
            { //Check player vs. planets
                Planet planet = CollidesWithPlanet(p);
                if (planet != null)
                {
                    PlacePlayerOnPlanet(p, planet);
                    collision = true;
                }
                if (!collision)
                    p.Position -= p.TotalVelocity;
            }

        }

        public void PlacePlayerOnGround(Player p)
        {
            p.IsStandingOn = GroundTypes.Ground;
            p.Angle = Utility.ToAngle(PhysicsEngine.NormalizedRightVector);
            if (CollidesWithBorders(p))
                PlacePlayerInsideLevel(p);
            while (!model.Terrain.IsPositionClear(p.BottomPoint))
            {
                p.Position += PhysicsEngine.NormalizedUpVector;
            }
        }

        public void PlacePlayerOnPlanet(Player p, Planet planet)
        {
            p.IsStandingOn = GroundTypes.Planet;
            p.CurrentPlanet = planet;
        }

        /// <summary>
        /// Moves the player along the ground and checks if it hits a wall or a cliff
        /// </summary>
        /// <param name="p">The player.</param>
        private void CheckGroundCollision(Player p)
        {
            Vector2 bottomPoint = PhysicsEngine.GetBottomPoint(p.BoundingBox);
            Vector2 nextStep = bottomPoint + p.SelfVelocity; //coordinates of the presumed next step
            Vector2 depthCheck; // For checking if a slope is too high or low

            Planet planet = CollidesWithPlanet(p);
            if (planet != null) //Player collides with planet
            {
                PlacePlayerOnPlanet(p, planet);
            }
            else if (CollidesWithBorders(p))
            {
                PlacePlayerInsideLevel(p);
            }
            else if (model.Terrain.IsPositionClear(nextStep)) //Terrain is either straight or sloped downwards
            {
                depthCheck = -PhysicsEngine.NormalizedUpVector * p.MaxClimbingDistance;
                if (model.Terrain.IsPositionClear(nextStep + depthCheck))
                {
                    // We are free-falling!
                    p.IsStandingOn = GroundTypes.Air;
                }
                else
                {
                    depthCheck = -PhysicsEngine.NormalizedUpVector;
                    while (model.Terrain.IsPositionClear(nextStep + depthCheck))
                    {
                        nextStep += depthCheck;
                    }
                }
            }
            else //Terrain is climbing
            {
                depthCheck = PhysicsEngine.NormalizedUpVector * p.MaxClimbingDistance;
                if (model.Terrain.IsPositionClear(nextStep + depthCheck))
                {
                    //Climbing slope
                    depthCheck = PhysicsEngine.NormalizedUpVector;
                    while (!model.Terrain.IsPositionClear(nextStep + depthCheck))
                    {
                        nextStep += depthCheck;
                    }
                }
                else
                {
                    //Slope too high!
                    nextStep = bottomPoint;
                }
            }
            p.SelfVelocity = nextStep - bottomPoint;
        }

        private void PlacePlayerInsideLevel(Player p)
        {
            if (p.BoundingBox.Left < 0)
            {
                p.Position -= new Vector2(p.BoundingBox.Left, 0);
            }
            else if (p.BoundingBox.Right >= model.Terrain.Width)
            {
                p.Position -= new Vector2(p.BoundingBox.Right - model.Terrain.Width, 0);
            }
            if (p.BoundingBox.Top < 0)
            {
                p.Position -= new Vector2(0, p.BoundingBox.Top);
            }
            else if (p.BoundingBox.Bottom >= model.Terrain.Height)
            {
                p.Position -= new Vector2(0, p.BoundingBox.Bottom - model.Terrain.Height);
            }
        }

        private void CalculateBloodCollisions()
        {
            for (int i = 0; i < model.BloodParticleList.Count; i++)
            {
                BloodParticle bp = model.BloodParticleList.ElementAt(i);
                //Check blood vs terrain
                if (!model.Terrain.IsPositionInsideLevel(bp.Position))
                {
                    model.Remove(bp);
                }
                else if (!model.Terrain.IsPositionClear(bp.Position)) // hits terrain
                {
                    model.AddSpriteToTerrain((int)bp.Position.X, (int)bp.Position.Y, bp.Spritesheet, bp.Scale.X);
                    model.Remove(bp);
                }
                    /*
                else if (CollidesWithBlackHole(bp))
                {
                    model.BlackHoleHit(bp);
                }
                     */
            }
        }

        /// <summary>
        /// Checks if projectiles' center point will collide with terrain or players, 
        /// and notifies model if it does.
        /// </summary>
        private void CalculateProjectileCollisions() //TODO: Modify method to handle bouncing projectiles, terrain-adding projectiles, etc.
        {
            int margin = 200; //Projectile can go this far out of screen before disappearing
            for (int i = 0; i < model.ProjectileList.Count; i++)
            {
                Projectile proj = model.ProjectileList.ElementAt(i);
                bool collisionFound = false;
                Vector2 originalPos = proj.Position;
                Vector2 nextPos = proj.Position + proj.TotalVelocity;
                Vector2 stepSize = Vector2.Normalize(proj.TotalVelocity);
                Vector2 collisionPoint;
                do
                {
                    //Check projectile vs. terrain, if entity limit is reached use cheaper collision detection
                    bool collision = model.IsEntityLimitReached ? !model.Terrain.IsRectangleClear(proj.BoundingBox) : CollisionEngine.IntersectPixels(proj.WorldTransform, (int)proj.SourceRect.Width, (int)proj.SourceRect.Height, proj.ColorData, Matrix.Identity, model.Terrain.Width, model.Terrain.Height, model.Terrain.GetTextureColor(), out collisionPoint);
                    //if(!model.Terrain.IsPositionClear(nextStep))
                    if (collision)
                    {
                        model.TerrainHit(proj, proj.Position);
                        collisionFound = true;
                    }// Check projectile vs. black hole
                    else if (CollidesWithBlackHole(proj))
                    {
                        model.BlackHoleHit(proj);
                        collisionFound = true;
                    }
                    else if (CollidesWithPlanet(proj) != null)
                    {
                        model.PlanetHit(proj);
                        collisionFound = true;
                    }
                    else
                    {//Check projectile vs. player
                        foreach (Player p in model.PlayerList)
                        {
                            if (p.Enabled)
                            {
                                //collision = model.IsEntityLimitReached ? p.BoundingBox.Contains((int)proj.Position.X, (int)proj.Position.Y) : CollisionEngine.IntersectPixels(proj.WorldTransform, (int)proj.Size.X, (int)proj.Size.Y, proj.ColorData, p.WorldTransform, (int)p.Size.X, (int)p.Size.Y, p.ColorData);
                                collision = p.BoundingBox.Contains(proj.BoundingBox);
                                if (collision)
                                {
                                    model.PlayerHit(proj, p, proj.Position);
                                    collisionFound = true;
                                }
                            }
                        }
                        if (!collisionFound)
                            proj.Position += stepSize;
                    }
                } while (!collisionFound && Vector2.Distance(proj.Position, nextPos) < 1);

                /* Explode projectile if it hits bottom */
                if (!collisionFound && proj.Ammunition.IsExplosive && PhysicsEngine.CollidesWithBottomBorder(proj, model.Terrain))
                {
                    model.TerrainHit(proj, proj.Position);
                }

                Rectangle extendedBoundingBox = new Rectangle(0, 0, model.Terrain.Width, model.Terrain.Height);
                extendedBoundingBox.Inflate(margin, margin);

                if (!collisionFound && !extendedBoundingBox.Contains(Utility.ToPoint(proj.Position)))
                {
                    //Projectile outside level bounds
                    model.ProjectileOutsideLevelBounds(proj);
                    collisionFound = true;
                }
                proj.Position = originalPos;

            }
        }

        private bool CollidesWithBorders(PhysicalObject po)
        {
            return !new Rectangle(0, 0, model.Terrain.Width, model.Terrain.Height).Contains(po.BoundingBox);
        }

        private Planet CollidesWithPlanet(PhysicalObject po)
        {
            foreach (Planet planet in model.PlanetList)
            {
                if (Vector2.Distance(po.Position, planet.Position) < planet.Radius)
                {
                    return planet;
                }
            }
            return null;
        }

        #region Static Methods
        /// <summary>
        /// Determines if there is overlap of the non-transparent pixels
        /// between two sprites.
        /// </summary>
        /// <param name="rectangleA">Bounding rectangle of the first sprite</param>
        /// <param name="dataA">Pixel data of the first sprite</param>
        /// <param name="rectangleB">Bouding rectangle of the second sprite</param>
        /// <param name="dataB">Pixel data of the second sprite</param>
        /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
        public static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                                  Rectangle rectangleB, Color[] dataB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }

        public static bool IntersectPixels(Matrix transformA, int widthA, int heightA, Color[] dataA, Terrain terrain)
        {
            Vector2 collisionPoint;
            return IntersectPixels(transformA, widthA, heightA, dataA, terrain, out collisionPoint);
        }

        public static bool IntersectPixels(Matrix transformA, int widthA, int heightA, Color[] dataA, Terrain terrain, out Vector2 collisionPoint)
        {
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformA);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformA);

            // Calculate the top left corner of A in B's local space
            // This variable will be reused to keep track of the start of each row
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformA);

            // For each row of pixels in A
            for (int yA = 0; yA < heightA; yA++)
            {
                // Start at the beginning of the row
                Vector2 posInB = yPosInB;

                // For each pixel in this row
                for (int xA = 0; xA < widthA; xA++)
                {
                    // Round to the nearest pixel
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    // If the pixel lies within the bounds of B
                    if (0 <= xB && xB < terrain.Width &&
                        0 <= yB && yB < terrain.Height)
                    {
                        // Get the colors of the overlapping pixels
                        Color colorA = dataA[xA + yA * widthA];

                        // If both pixels are not completely transparent,
                        if (colorA.A != 0 && !terrain.IsPositionClear(new Vector2(xB, yB)))
                        {
                            // then an intersection has been found
                            collisionPoint = Vector2.Transform(new Vector2(xA, yA), transformA);
                            return true;
                        }
                    }

                    // Move to the next pixel in the row
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // No intersection found
            collisionPoint = new Vector2(-1, -1);
            return false;
        }

        /// <summary>
        /// Determines if there is overlap of the non-transparent pixels between two
        /// sprites.
        /// </summary>
        /// <param name="transformA">World transform of the first sprite.</param>
        /// <param name="widthA">Width of the first sprite's texture.</param>
        /// <param name="heightA">Height of the first sprite's texture.</param>
        /// <param name="dataA">Pixel color data of the first sprite.</param>
        /// <param name="transformB">World transform of the second sprite.</param>
        /// <param name="widthB">Width of the second sprite's texture.</param>
        /// <param name="heightB">Height of the second sprite's texture.</param>
        /// <param name="dataB">Pixel color data of the second sprite.</param>
        /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
        public static bool IntersectPixels(Matrix transformA, int widthA, int heightA, Color[] dataA,
                                                  Matrix transformB, int widthB, int heightB, Color[] dataB)
        {
            Vector2 collisionPoint;
            return IntersectPixels(transformA, widthA, heightA, dataA, transformB, widthB, heightB, dataB, out collisionPoint);
        }

        /// <summary>
        /// Determines if there is overlap of the non-transparent pixels between two
        /// sprites.
        /// </summary>
        /// <param name="transformA">World transform of the first sprite.</param>
        /// <param name="widthA">Width of the first sprite's texture.</param>
        /// <param name="heightA">Height of the first sprite's texture.</param>
        /// <param name="dataA">Pixel color data of the first sprite.</param>
        /// <param name="transformB">World transform of the second sprite.</param>
        /// <param name="widthB">Width of the second sprite's texture.</param>
        /// <param name="heightB">Height of the second sprite's texture.</param>
        /// <param name="dataB">Pixel color data of the second sprite.</param>
        /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
        public static bool IntersectPixels(
                                   Matrix transformA, int widthA, int heightA, Color[] dataA,
                                   Matrix transformB, int widthB, int heightB, Color[] dataB, out Vector2 collisionPoint)
        {
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            Matrix transformAToB = transformA * Matrix.Invert(transformB);

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Calculate the top left corner of A in B's local space
            // This variable will be reused to keep track of the start of each row
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            // For each row of pixels in A
            for (int yA = 0; yA < heightA; yA++)
            {
                // Start at the beginning of the row
                Vector2 posInB = yPosInB;

                // For each pixel in this row
                for (int xA = 0; xA < widthA; xA++)
                {
                    // Round to the nearest pixel
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    // If the pixel lies within the bounds of B
                    if (0 <= xB && xB < widthB &&
                        0 <= yB && yB < heightB)
                    {
                        // Get the colors of the overlapping pixels
                        Color colorA = dataA[xA + yA * widthA];
                        Color colorB = dataB[xB + yB * widthB];

                        // If both pixels are not completely transparent,
                        if (colorA.A != 0 && colorB.A != 0)
                        {
                            // then an intersection has been found
                            collisionPoint = Vector2.Transform(new Vector2(xA, yA), transformA);
                            return true;
                        }
                    }

                    // Move to the next pixel in the row
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // No intersection found
            collisionPoint = new Vector2(-1, -1);
            return false;
        }

        /// <summary>
        /// Calculates an axis aligned rectangle which fully contains an arbitrarily
        /// transformed axis aligned rectangle.
        /// </summary>
        /// <param name="rectangle">Original bounding rectangle.</param>
        /// <param name="transform">World transform of the rectangle.</param>
        /// <returns>A new rectangle which contains the trasnformed rectangle.</returns>
        public static Rectangle CalculateBoundingRectangle(Rectangle rectangle,
                                                                  Matrix transform)
        {
            // Get all four corners in local space
            Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            // Find the minimum and maximum extents of the rectangle in world space
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return that as a rectangle
            return new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        /// <summary>
        /// Returns true if the rectangle collides with terrain,
        /// or false if there is no collision.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="terrain">The terrain.</param>
        /// <returns></returns>
        public static bool BoundingBoxTerrainCollision(Rectangle rectangle, Terrain terrain)
        {
            TerrainTypes pixel;
            int globalX, globalY;
            for (int x = 0; x < rectangle.Width; x++)
            {
                for (int y = 0; y < rectangle.Height; y++)
                {
                    globalX = x + (int)rectangle.X;
                    globalY = y + (int)rectangle.Y;
                    if (globalX >= 0 && globalX < terrain.Width && globalY >= 0 && globalY < terrain.Height)
                    {
                        pixel = terrain.GetPixelType(globalX, globalY);
                        if (pixel != TerrainTypes.None)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
