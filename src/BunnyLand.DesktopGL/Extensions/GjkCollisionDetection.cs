using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace BunnyLand.DesktopGL.Extensions;

public static class GjkCollisionDetection
{
    public static Vector2 GetSupportVector(this IShapeF shape, Vector2 direction) =>
        shape switch {
            RectangleF rect => rect.ClosestPointTo(rect.Center + direction),
            CircleF circle => circle.ClosestPointTo(circle.Center + direction),
            _ => throw new NotImplementedException()
        };

    public static Point2 GetCenter(this IShapeF shape) => shape switch {
        RectangleF rect => rect.Center,
        CircleF circle => circle.Center,
        _ => throw new NotImplementedException()
    };

    public static OverlapTestResult TestOverlap(this IShapeF first, IShapeF second)
    {
        var direction = Vector2.Zero;
        var vertices = new List<Vector2>();
        var result = EvolveResult.StillEvolving;
        var iterations = 0;
        while (iterations < 20 && result == EvolveResult.StillEvolving) {
            result = EvolveSimplex(vertices, first, second, ref direction);
            iterations++;
        }

        return new OverlapTestResult(result == EvolveResult.FoundIntersection, vertices, first, second);
    }

    private static Vector2 TripleProduct(Vector2 a, Vector2 b, Vector2 c)
    {
        var a3 = new Vector3(a.X, a.Y, 0);
        var b3 = new Vector3(b.X, b.Y, 0);
        var c3 = new Vector3(c.X, c.Y, 0);

        var first = Vector3.Cross(a3, b3);
        var second = Vector3.Cross(first, c3);

        return new Vector2(second.X, second.Y);
    }

    private static EvolveResult EvolveSimplex(List<Vector2> vertices, IShapeF shapeA, IShapeF shapeB,
        ref Vector2 direction)
    {
        switch (vertices.Count) {
            case 0:
                direction = shapeB.GetCenter() - shapeA.GetCenter();
                break;

            // flip the direction
            case 1:
                direction *= 1;
                break;
            case 2: {
                // line ab is the line formed by the first two vertices
                var ab = vertices[1] - vertices[0];
                // line a0 is the line from the first vertex to the origin
                var a0 = vertices[0] * -1;

                // use the triple-cross-product to calculate a direction perpendicular
                // to line ab in the direction of the origin
                direction = TripleProduct(ab, a0, ab);
                break;
            }
            case 3: {
                // calculate if the simplex contains the origin
                var c0 = vertices[2] * -1;
                var bc = vertices[1] - vertices[2];
                var ca = vertices[0] - vertices[2];

                var bcNormal = TripleProduct(ca, bc, bc);
                var caNormal = TripleProduct(bc, ca, ca);

                if (Vector2.Dot(bcNormal, c0) > 0) {
                    // the origin is outside line bc
                    // get rid of a and add a new support in the direction of bcNorm
                    vertices.Remove(vertices[0]);
                    direction = bcNormal;
                } else if (Vector2.Dot(caNormal, c0) > 0) {
                    // the origin is outside line ca
                    // get rid of b and add a new support in the direction of caNorm
                    vertices.Remove(vertices[1]);
                    direction = caNormal;
                } else {
                    // the origin is inside both ab and ac,
                    // so it must be inside the triangle!
                    return EvolveResult.FoundIntersection;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(vertices),
                    $"Can't have simplex with {vertices.Count} vertixes!");
        }

        return AddSupport(vertices, shapeA, shapeB, direction)
            ? EvolveResult.StillEvolving
            : EvolveResult.NoIntersection;
    }

    private static bool AddSupport(List<Vector2> vertices, IShapeF shapeA, IShapeF shapeB, Vector2 direction)
    {
        var newVertex = CalculateSupport(shapeA, shapeB, direction);
        vertices.Add(newVertex);
        return direction.Dot(newVertex) >= 0;
    }

    private static Vector2 CalculateSupport(IShapeF shapeA, IShapeF shapeB, Vector2 direction)
    {
        var oppositeDirection = direction * -1;
        var newVertex = shapeA.GetSupportVector(direction);
        newVertex -= shapeB.GetSupportVector(oppositeDirection);
        return newVertex;
    }


    private enum EvolveResult
    {
        NoIntersection,
        FoundIntersection,
        StillEvolving
    }
}

public class OverlapTestResult
{
    public bool Colliding { get; set; }
    public List<Vector2> Simplex { get; set; }
    public IShapeF ShapeA { get; set; }
    public IShapeF ShapeB { get; set; }

    public OverlapTestResult(bool colliding, List<Vector2> simplex, IShapeF shapeA, IShapeF shapeB)
    {
        Colliding = colliding;
        Simplex = simplex;
        ShapeA = shapeA;
        ShapeB = shapeB;
    }
}