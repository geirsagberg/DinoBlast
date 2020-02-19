using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Systems
{
    // Based on https://github.com/dmanning23/CollisionBuddy/blob/master/CollisionBuddy/CollisionBuddy.SharedProject/CollisionBuddy.cs, MIT License
    // public static class CollisionCheck
    // {
    //     /// <summary>
    //     /// Check for collision between two circles
    //     /// This also checks for tunneling, so that if the circles are going to pass bewteen each other
    //     /// </summary>
    //     /// <param name="first">teh first circle</param>
    //     /// <param name="second">the second circle</param>
    //     /// <returns>These cricles are either colliding, or going to collide</returns>
    //     public static bool CircleCircleCollision(CircleF first, CircleF second, CircleF firstOld, CircleF secondOld)
    //     {
    //         // calculate relative velocity and position
    //         float dvx = (second.Position.X - secondOld.Position.X) - (first.Position.X - firstOld.Position.X);
    //         float dvy = (second.Position.Y - secondOld.Position.Y) - (first.Position.Y - firstOld.Position.Y);
    //         float dx = secondOld.Position.X - firstOld.Position.X;
    //         float dy = secondOld.Position.Y - firstOld.Position.Y;
    //
    //         // check if circles are already colliding
    //         float sqRadiiSum = first.Radius + second.Radius;
    //         sqRadiiSum *= sqRadiiSum;
    //         float pp = dx * dx + dy * dy - sqRadiiSum;
    //         if (pp < 0)
    //         {
    //             return true;
    //         }
    //
    //         // check if the circles are moving away from each other and hence can’t collide
    //         float pv = dx * dvx + dy * dvy;
    //         if (pv >= 0)
    //         {
    //             return false;
    //         }
    //
    //         // check if the circles can reach each other between the frames
    //         float vv = dvx * dvx + dvy * dvy;
    //         if ((pv + vv) <= 0 && (vv + 2 * pv + pp) >= 0)
    //         {
    //             return false;
    //         }
    //
    //         // if we've gotten this far then it’s possible for intersection if the distance between
    //         // the circles is less than the radii sum when it’s at a minimum. Therefore find the time
    //         // when the distance is at a minimum and test this
    //         float tmin = -pv / vv;
    //         return (pp + pv * tmin < 0);
    //     }
    //
    //     /// <summary>
    //     /// given two circles, find the closest points between them.
    //     /// </summary>
    //     /// <param name="A">teh first circle</param>
    //     /// <param name="B">the second circle</param>
    //     /// <param name="ClosestPointA">the closet point of circle A to circle B</param>
    //     /// <param name="ClosestPointB">the closet point of circle B to circle A</param>
    //     public static void ClosestPoints(CircleF A, CircleF B, out Vector2 ClosestPointA, out Vector2 ClosestPointB)
    //     {
    //         //get the direction of the vector between the circles
    //         Vector2 lineDirection2 = A.Position - B.Position;
    //         lineDirection2.Normalize();
    //         Vector2 lineDirection1 = new Vector2(-lineDirection2.X, -lineDirection2.Y);
    //
    //         //get the vector from the circle centers to the collision point
    //         Vector2 center2collision1 = lineDirection1 * A.Radius;
    //         Vector2 center2collision2 = lineDirection2 * B.Radius;
    //
    //         //get the first collision point
    //         ClosestPointA = A.Position + center2collision1;
    //
    //         //get the second collision point
    //         ClosestPointB = B.Position + center2collision2;
    //     }
    //
    //     /// <summary>
    //     /// given a circle and a line, check if they are colliding and where they are colliding at!
    //     /// NOTE: this does not check for tunneling!  If the circles velocity is greater than it's radius, it can pass right through the line
    //     /// </summary>
    //     /// <param name="A">the circle to check</param>
    //     /// <param name="B">the line to check</param>
    //     /// <param name="ClosestPointA">if a collision is occuring, the closet point on the circle to the line</param>
    //     /// <param name="ClosestPointB">if a collision is occuring, the closest point on the line to the circle</param>
    //     /// <returns>true if the circle and line are colliding, false if not</returns>
    //     public static bool CircleLineCollision(CircleF A, Ray2 B, ref Vector2 ClosestPointA, ref Vector2 ClosestPointB)
    //     {
    //         //get the vector from the circle center to the start of the line segment
    //         Vector2 CV = A.Position - B.Position;
    //
    //         //project the CV onto the line segment
    //         float ProjL = Vector2.Dot(B.Direction, CV);
    //
    //         if (ProjL < 0)
    //         {
    //             //the closest point on the line is the start point
    //             float fCirlclDot = Vector2.Dot(CV, CV);
    //             if (fCirlclDot > Math.Pow(A.Radius , 2))
    //             {
    //                 //There is no collision
    //                 return false;
    //             }
    //             else
    //             {
    //                 ClosestPointB = B.Position;
    //
    //                 //get the collision point on the circle
    //                 CV.Normalize();
    //                 Vector2 CenterToEdge = CV * A.Radius;
    //                 ClosestPointA = A.Position - CenterToEdge;
    //
    //                 return true;
    //             }
    //         }
    //         else if (ProjL > B.Direction.Length())
    //         {
    //             //get the vector of the line segment
    //             Vector2 lineVect = B.Direction;
    //
    //             //The second point of the line segment is the closest
    //             Vector2 CV2 = CV - lineVect;
    //             float cv2Dot = Vector2.Dot(CV2, CV2);
    //             if (cv2Dot > Math.Pow(A.Radius , 2))
    //             {
    //                 //There is no collision
    //                 return false;
    //             }
    //             else
    //             {
    //                 ClosestPointB = B.Position + B.Direction;
    //
    //                 //get the collision point on the circle
    //                 CV = A.Position - ClosestPointB;
    //                 CV.Normalize();
    //                 Vector2 CenterToEdge = CV * A.Radius;
    //                 ClosestPointA = A.Position - CenterToEdge;
    //
    //                 return true;
    //             }
    //         }
    //         else
    //         {
    //             //The closest point is a midpoint on the line segemnt
    //             Vector2 VProj = B.Direction * ProjL;
    //             Vector2 ClosePoint = B.Position + VProj;
    //
    //             //if the dot product of the vector from the closest point and itself is less than the radius squared, there is a collision
    //             float fClosestPoint = Vector2.Dot((ClosePoint - (Vector2) A.Position), (ClosePoint - (Vector2) A.Position));
    //             if (fClosestPoint > Math.Pow(A.Radius , 2))
    //             {
    //                 return false;
    //             }
    //             else
    //             {
    //                 ClosestPointB = ClosePoint;
    //
    //                 //get the collision point on the circle
    //                 CV = A.Position - ClosePoint;
    //                 CV.Normalize();
    //                 Vector2 CenterToEdge = CV * A.Radius;
    //                 ClosestPointA = A.Position - CenterToEdge;
    //                 return true;
    //             }
    //         }
    //     }
    //
    //     /// <summary>
    //     /// Given a rect and a circle that we assume is in that rect...
    //     /// Find if the circle is hitting the wall, where it is hitting the wall, and where to move it to put it back in the box
    //     /// This function also prevents tunneling, if the circle totally leaves the rect it will pop back in
    //     /// </summary>
    //     /// <param name="A">The circle we are checking</param>
    //     /// <param name="B">THe rectangle we are putting the circle in</param>
    //     /// <param name="CollisionPoint">If the circle is touching the wall, the point where they are touching</param>
    //     /// <param name="Overlap">A vector that you can add to the circle's position to put it back in the rect</param>
    //     /// <returns>true if the circle is outsied the edge of the rect, false if it is still all inside</returns>
    //     public static bool CircleRectCollision(CircleF A, Rectangle B, ref Vector2 CollisionPoint, ref Vector2 Overlap)
    //     {
    //         //Get the delta from last frame to this one
    //         Vector2 Velocity = AOld.Position - A.Position;
    //
    //         //get the velocity direction if it isn't zero
    //         float fVelocityLength = 0.0f;
    //         if (fVelocityLength > 0.0f)
    //         {
    //             Velocity.Normalize();
    //         }
    //
    //         //get the bottom of the circle
    //         float fBottom = A.Position.Y + A.Radius;
    //
    //         //check for fast ground hits
    //         float fFastBottom = fBottom + (Velocity.Y * fVelocityLength);
    //         if (fFastBottom > B.Bottom)
    //         {
    //             //a floor hit occured
    //
    //             //get the delta between the current Position and the ground
    //             Overlap.X = 0.0f;
    //             Overlap.Y = (B.Bottom - fBottom);
    //
    //             //get the point where they are colliding
    //             CollisionPoint.X = A.Position.X;
    //             CollisionPoint.Y = B.Bottom;
    //             return true;
    //         }
    //         else
    //         {
    //             //get the top of the polygon (will be fast bottom plus 2 * radius
    //             float fFastTop = fFastBottom - (2.0f * A.Radius);
    //             if (fFastTop < B.Top)
    //             {
    //                 //a ceiling hit occured
    //
    //                 //get the top of the polygon
    //                 float fTop = A.Position.Y - A.Radius;
    //
    //                 //get the delta between the current Position and the ceiling
    //                 Overlap.X = 0.0f;
    //                 Overlap.Y = -1.0f * (fTop - B.Top);
    //
    //                 //get the point where they are colliding
    //                 CollisionPoint.X = A.Position.X;
    //                 CollisionPoint.Y = B.Top;
    //                 return true;
    //             }
    //         }
    //
    //         //get the right edge of the polygon
    //         float fRight = A.Position.X + A.Radius;
    //
    //         //check for fast right wall hits
    //         float fFastRight = fRight + (Velocity.X * fVelocityLength);
    //         if (fFastRight > B.Right)
    //         {
    //             //a right wall hit occured
    //
    //             //get the delta between the current Position and the right wall
    //             Overlap.X = (B.Right - fFastRight);
    //             Overlap.Y = 0.0f;
    //
    //             //get the collision poitn
    //             CollisionPoint.X = B.Right;
    //             CollisionPoint.Y = A.Position.Y;
    //             return true;
    //         }
    //         else
    //         {
    //             //get the left of the polygon (will be fast right plus 2 * radius
    //             float fFastLeft = fFastRight - (2.0f * A.Radius);
    //             if (fFastLeft < B.Left)
    //             {
    //                 //a left wall hit occured
    //
    //                 //get the left edge of the polygon
    //                 float fLeft = A.Position.X - A.Radius;
    //                 Overlap.X = -1.0f * (fLeft - B.Left);
    //                 Overlap.Y = 0.0f;
    //
    //                 //get the collision poitn
    //                 CollisionPoint.X = B.Left;
    //                 CollisionPoint.Y = A.Position.Y;
    //                 return true;
    //             }
    //         }
    //
    //         //no collision occured!
    //         return false;
    //     }
    // }
}
