using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Components
{
    public class Accelerator
    {
        public Vector2 IntendedAcceleration { get; set; }

        // Do we need these? Do we want these?
        // Could be useful if we generalize Accelerator
        public float MaxAcceleration { get; set; }
        public float MaxSpeed { get; set; }

        public Accelerator()
        {
        }

        public Accelerator(Vector2 intendedAcceleration)
        {
            IntendedAcceleration = intendedAcceleration;
        }
    }
}
