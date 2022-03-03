using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Utils;

// Based on https://github.com/dmanning23/CollisionBuddy/blob/master/CollisionBuddy/CollisionBuddy.SharedProject/CollisionBuddy.cs, MIT License
public static class CollisionCheck
{
    /// <summary>
    ///     Check for collision between two circles
    ///     This also checks for tunneling, so that if the circles are going to pass bewteen each other
    /// </summary>
    public static bool CircleCircleCollision(CircleF first, CircleF second, CircleF firstOld, CircleF secondOld)
    {
        // calculate relative velocity and position
        var dvx = second.Position.X - secondOld.Position.X - (first.Position.X - firstOld.Position.X);
        var dvy = second.Position.Y - secondOld.Position.Y - (first.Position.Y - firstOld.Position.Y);
        var dx = secondOld.Position.X - firstOld.Position.X;
        var dy = secondOld.Position.Y - firstOld.Position.Y;

        // check if circles are already colliding
        var sqRadiiSum = first.Radius + second.Radius;
        sqRadiiSum *= sqRadiiSum;
        var pp = dx * dx + dy * dy - sqRadiiSum;
        if (pp < 0) {
            return true;
        }

        // check if the circles are moving away from each other and hence can’t collide
        var pv = dx * dvx + dy * dvy;
        if (pv >= 0) {
            return false;
        }

        // check if the circles can reach each other between the frames
        var vv = dvx * dvx + dvy * dvy;
        if (pv + vv <= 0 && vv + 2 * pv + pp >= 0) {
            return false;
        }

        // if we've gotten this far then it’s possible for intersection if the distance between
        // the circles is less than the radii sum when it’s at a minimum. Therefore find the time
        // when the distance is at a minimum and test this
        var tmin = -pv / vv;
        return pp + pv * tmin < 0;
    }

    /// <summary>
    ///     given two circles, find the closest points between them.
    /// </summary>
    public static void ClosestPoints(CircleF a, CircleF b, out Vector2 closestPointA, out Vector2 closestPointB)
    {
        //get the direction of the vector between the circles
        var lineDirection2 = a.Position - b.Position;
        lineDirection2.Normalize();
        var lineDirection1 = new Vector2(-lineDirection2.X, -lineDirection2.Y);

        //get the vector from the circle centers to the collision point
        var center2Collision1 = lineDirection1 * a.Radius;
        var center2Collision2 = lineDirection2 * b.Radius;

        //get the first collision point
        closestPointA = a.Position + center2Collision1;

        //get the second collision point
        closestPointB = b.Position + center2Collision2;
    }

    /// <summary>
    ///     given a circle and a line, check if they are colliding and where they are colliding at!
    ///     NOTE: this does not check for tunneling!  If the circles velocity is greater than it's radius, it can pass right
    ///     through the line
    /// </summary>
    /// <param name="a">the circle to check</param>
    /// <param name="b">the line to check</param>
    /// <param name="closestPointA">if a collision is occuring, the closet point on the circle to the line</param>
    /// <param name="closestPointB">if a collision is occuring, the closest point on the line to the circle</param>
    /// <returns>true if the circle and line are colliding, false if not</returns>
    public static bool CircleLineCollision(CircleF a, Ray2 b, ref Vector2 closestPointA, ref Vector2 closestPointB)
    {
        //get the vector from the circle center to the start of the line segment
        var cv = a.Position - b.Position;

        //project the CV onto the line segment
        var projL = Vector2.Dot(b.Direction, cv);

        if (projL < 0) {
            //the closest point on the line is the start point
            var fCirlclDot = Vector2.Dot(cv, cv);
            if (fCirlclDot > Math.Pow(a.Radius, 2)) {
                //There is no collision
                return false;
            } else {
                closestPointB = b.Position;

                //get the collision point on the circle
                cv.Normalize();
                var centerToEdge = cv * a.Radius;
                closestPointA = a.Position - centerToEdge;

                return true;
            }
        } else if (projL > b.Direction.Length()) {
            //get the vector of the line segment
            var lineVect = b.Direction;

            //The second point of the line segment is the closest
            var cv2 = cv - lineVect;
            var cv2Dot = Vector2.Dot(cv2, cv2);
            if (cv2Dot > Math.Pow(a.Radius, 2)) {
                //There is no collision
                return false;
            } else {
                closestPointB = b.Position + b.Direction;

                //get the collision point on the circle
                cv = a.Position - closestPointB;
                cv.Normalize();
                var centerToEdge = cv * a.Radius;
                closestPointA = a.Position - centerToEdge;

                return true;
            }
        } else {
            //The closest point is a midpoint on the line segemnt
            var vProj = b.Direction * projL;
            Vector2 closePoint = b.Position + vProj;

            //if the dot product of the vector from the closest point and itself is less than the radius squared, there is a collision
            var fClosestPoint = Vector2.Dot(closePoint - (Vector2) a.Position, closePoint - (Vector2) a.Position);
            if (fClosestPoint > Math.Pow(a.Radius, 2)) {
                return false;
            } else {
                closestPointB = closePoint;

                //get the collision point on the circle
                cv = a.Position - closePoint;
                cv.Normalize();
                var centerToEdge = cv * a.Radius;
                closestPointA = a.Position - centerToEdge;
                return true;
            }
        }
    }
}