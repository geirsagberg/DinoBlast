    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.Models
{
    public static class PhysicsEngine
    {
        private static Vector2 _GravityField = new Vector2(0, 0.1f); //Default gravity field
        /// <summary>
        /// Gets or sets the gravity field vector.
        /// </summary>
        /// <value>The gravity field vector.</value>
        public static Vector2 GravityField
        {
            get { return _GravityField; }
            set { _GravityField = value; }
        }
        private static float _GravityConstant = 1f; //Default gravity constant
        /// <summary>
        /// Gets or sets the gravity constant.
        /// </summary>
        /// <value>The gravity constant.</value>
        public static float GravityConstant
        {
            get { return _GravityConstant; }
            set { _GravityConstant = value; }
        }
        /// <summary>
        /// Returns a Vector2 showing the gravitational pull of GravityPoint gp on PhysicalObject po
        /// </summary>
        /// <param name="po">The physical object being affected.</param>
        /// <param name="gp">The gravity point affecting the object.</param>
        /// <returns></returns>
        public static Vector2 CalculateGravity(PhysicalObject po, GravityPoint gp)
        {
            Vector2 delta = gp.Position - po.Position;
            float r = delta.Length();
            float gravPull = GravityConstant * gp.Mass / (r * r);
            float theta = (float)Math.Atan2(delta.Y, delta.X);
            Vector2 result = new Vector2();
            result.X = gravPull * (float)Math.Cos(theta);
            result.Y = gravPull * (float)Math.Sin(theta);
            return result;
        }

        /// <summary>
        /// Returns a unit Vector2 pointing the other way of the gravity field, i.e. up.
        /// </summary>
        /// <value>The normalized up vector.</value>
        public static Vector2 NormalizedUpVector
        {
            get
            {
                Vector2 up = -GravityField;
                up.Normalize();
                return up;
            }
        }
        /// <summary>
        /// Gets a unit vector pointing left, relative to the gravity field.
        /// </summary>
        /// <value>The normalized left vector.</value>
        public static Vector2 NormalizedLeftVector
        {
            get
            {
                Vector2 left = new Vector2(-GravityField.Y, GravityField.X);
                left.Normalize();
                return left;
            }
        }
        /// <summary>
        /// Gets a unit vector pointing right, relative to the gravity field.
        /// </summary>
        /// <value>The normalized right vector.</value>
        public static Vector2 NormalizedRightVector
        {
            get
            {
                Vector2 right = new Vector2(GravityField.Y, -GravityField.X);
                right.Normalize();
                return right;
            }
        }

        /// <summary>
        /// Gets a point of the rectangle that is at the border corresponding to the gravity field,
        /// i.e. bottom border for normal gravity.
        /// </summary>
        /// <param name="rectangle">The bounding box of an entity.</param>
        /// <returns></returns>
        public static Vector2 GetBottomPoint(Rectangle rectangle)
        {
            Vector2 point = Vector2.Zero;
            if (GravityField.X > 0)
                point.X = rectangle.Right;
            else if (GravityField.X < 0)
                point.X = rectangle.Left;
            else
                point.X = rectangle.Center.X;
            if (GravityField.Y > 0)
                point.Y = rectangle.Bottom;
            else if (GravityField.Y < 0)
                point.Y = rectangle.Top;
            else
                point.Y = rectangle.Center.Y;
            return point;
        }

        /// <summary>
        /// Gets a rectangle with the lower half of the given rectangle, relative to the gravity field.
        /// </summary>
        /// <param name="rectangle">The bottom half rectangle.</param>
        /// <returns></returns>
        public static Rectangle GetBottomHalf(Rectangle rectangle)
        {
            Rectangle bottomRectangle = new Rectangle();
             float angle = Utility.ToAngle(GravityField);
            if (angle < MathHelper.PiOver4 && angle >= -MathHelper.PiOver4) //get right rectangle
                bottomRectangle = new Rectangle(rectangle.X + rectangle.Width / 2, rectangle.Y, rectangle.Width / 2, rectangle.Height);
            else if (angle < 3 * MathHelper.PiOver4 && angle >= MathHelper.PiOver4) //get lower rectangle
                bottomRectangle = new Rectangle(rectangle.X, rectangle.Y + rectangle.Height / 2, rectangle.Width, rectangle.Height / 2);
            else if (angle < -3 * MathHelper.PiOver4 || angle >= 3 * MathHelper.PiOver4) //get left rectangle
                bottomRectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width / 2, rectangle.Height);
            else //get top rectangle
                bottomRectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height / 2);
            return bottomRectangle;
        }

        public static bool CollidesWithBottomBorder(PhysicalObject p, Terrain terrain)
        {
            if (GravityField.Y > 0 && GravityField.Y > GravityField.X) //gravity pointing down
                return p.BoundingBox.Bottom >= terrain.Height;
            else if (GravityField.Y < 0 && GravityField.Y < GravityField.X) //Gravity pointing up
                return p.BoundingBox.Top < 0;
            else if (GravityField.X > 0 && GravityField.X > GravityField.Y) //Gravity pointing right
                return p.BoundingBox.Right >= terrain.Width;
            else if (GravityField.X < 0 && GravityField.X < GravityField.Y) //gravity pointing left
                return p.BoundingBox.Left < 0;
            else
                return false;
        }
    }
}
