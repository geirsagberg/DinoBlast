using BunnyLand.DesktopGL.Utils;
using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Xunit;

namespace BunnyLand.Tests
{
    public class CollisionHelperTests
    {
        [Fact]
        public void Collision_detection_with_velocities_is_same_as_with_static_and_relative_velocities()
        {
            var firstCircle = new CircleF(new Point2(3, 0), 1);
            var secondCircle = new CircleF(new Point2(3, 3), 1);
            var firstVelocity = new Vector2(3, -3);
            var secondVelocity = new Vector2(3, 3);

            var penetration = CollisionHelper.CalculatePenetrationVector(firstCircle, secondCircle, firstVelocity,
                secondVelocity);
            var relativeVelocity = firstVelocity - secondVelocity;
            var otherPenetration = CollisionHelper.CalculatePenetrationVector(firstCircle, secondCircle,
                relativeVelocity, Vector2.Zero);

            penetration.Should().Be(new Vector2(0, 2));
            penetration.Should().BeEquivalentTo(otherPenetration);
        }
    }
}
