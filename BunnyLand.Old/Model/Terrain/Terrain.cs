using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BunnyLand.Models
{
    public enum TerrainTypes
    {
        None,
        Destructible,
        Indestructible
    }
    /* Base terrain class, implements basic functionality and defines abstract methods for generating terrain */
    public abstract class Terrain
    {
        protected TerrainTypes[,] collisionArray;
        protected Color[] foregroundColors;
        protected PerlinNoise noiseGenerator;
        protected int width, height;
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        protected Random rand = new Random();
        protected bool isTerrainGenerated = false;
        protected bool isTextureInitialized = false;

        public Terrain(int width, int height) : this(width, height, 0.5, 0.5) { }

        public Terrain(int width, int height, double smoothness, double steepness)
        {
            noiseGenerator = perlinGenerator(smoothness, steepness);
            collisionArray = new TerrainTypes[width, height];
            foregroundColors = new Color[width * height];
            this.width = width;
            this.height = height;
        }

        private PerlinNoise perlinGenerator(double smoothness, double steepness)
        {
            int octaves = 2 + (int)(smoothness * 6); // gives from 2 to 8 octaves
            double persistency = 1 - (0.25 + steepness * 0.5); // gives from 0.25 to 0.75 persistency
            return new PerlinNoise(octaves, persistency, 0.002, 1);
        }

        public bool IsPositionClear(Vector2 pos)
        {
            if (IsPositionInsideLevel(pos) && collisionArray[(int)pos.X, (int)pos.Y] == TerrainTypes.None)
                return true;
            else
                return false;
        }

        public bool IsPositionInsideLevel(Vector2 pos)
        {
            if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height)
                return false; // out of level bounds
            else
                return true;
        }

        /// <summary>
        /// Returns true if 8 corner points of the rectangle are clear.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>
        /// 	<c>true</c> if rectangle is clear; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRectangleClear(Rectangle rectangle)
        {
            Vector2 topLeft = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 topRight = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 bottomLeft = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 bottomRight = new Vector2(rectangle.Right, rectangle.Bottom);
            Vector2 top = new Vector2(rectangle.Center.X, rectangle.Top);
            Vector2 bottom = new Vector2(rectangle.Center.X, rectangle.Bottom);
            Vector2 left = new Vector2(rectangle.Left, rectangle.Center.Y);
            Vector2 right = new Vector2(rectangle.Right, rectangle.Center.Y);
            return (IsPositionClear(topLeft) && IsPositionClear(topRight) && IsPositionClear(bottomLeft) && IsPositionClear(bottomRight)
                && IsPositionClear(top) && IsPositionClear(bottom) && IsPositionClear(left) && IsPositionClear(right));
        }

        public TerrainTypes GetPixelType(int x, int y)
        {
            return collisionArray[x, y];
        }

        public void GenerateTerrain(int seed)
        {
            GenerateTerrainImpl(seed);
            isTerrainGenerated = true;
            isTextureInitialized = false;
        }

        protected abstract void GenerateTerrainImpl(int seed);

        public Color[] GetTextureColor()
        {
            if (!isTerrainGenerated)
                GenerateTerrain(0);
            if (!isTextureInitialized) //Texture not yet initialised
                SetTextureColor(Color.Brown, Color.SteelBlue, 10);
            return foregroundColors;
        }

        public Color[] GetTextureShade() //for testing, mostly
        {
            Color[] colors = new Color[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double d = noiseGenerator.Function2D(x, y) + 1; // should give values from 0.0 to 2.0
                    byte i = (byte)(d * 256 / 2);
                    if (i < 0)
                        i = 0;
                    else if (i > 255)
                        i = 255; // cap colors
                    colors[x + y * width] = new Color(i, i, i);
                }
            }
            return colors;
        }

        public Color[] AddToTerrain(int centerX, int centerY, int width, int height, bool destructible, bool collidable, bool extendTerrain, Color[] colors)
        {
            int left = centerX - width / 2;
            int right = centerX + width / 2;
            int top = centerY - height / 2;
            int bottom = centerY + height / 2;
            int globalX, globalY;
            Color[] result = new Color[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    globalX = left + x;
                    globalY = top + y;
                    if (globalX >= 0 && globalX < this.width && globalY >= 0 && globalY < this.height)
                    {
                        if (colors[x + y * width].A == 255) //Pixel is not transparent at all
                        {
                            if (collisionArray[globalX, globalY] == TerrainTypes.Destructible || extendTerrain)
                            {
                                if (collidable && extendTerrain)
                                    collisionArray[globalX, globalY] = destructible ? TerrainTypes.Destructible : TerrainTypes.Indestructible;
                                foregroundColors[globalX + globalY * this.width] = colors[x + y * width];
                                result[x + y * width] = colors[x + y * width];
                            }
                        }
                        else
                        {
                            result[x + y * width] = foregroundColors[globalX + globalY * this.width];
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Takes position and radius of a circle, removes pixels from collision array, 
        /// modifies texture accordingly, and returns a Color array size (2*radius)^2 for easy render-to-texture.
        /// </summary>
        /// <param name="centerX">The center X.</param>
        /// <param name="centerY">The center Y.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public Color[] DestroyCircle(int centerX, int centerY, int radius)
        {
            int size = radius * 2 + 1;
            Color[] result = new Color[(radius * 2 + 1) * (radius * 2 + 1)];
            int tempX, tempY;
            int localR;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    tempX = centerX + x;
                    tempY = centerY + y;
                    if (tempX >= 0 && tempX < width && tempY >= 0 && tempY < height) //within level boundaries
                    {
                        //Check if pixel is inside destruction radius
                        localR = (int)Math.Sqrt(x * x + y * y);
                        if (localR <= radius && collisionArray[tempX, tempY] == TerrainTypes.Destructible)
                        {
                            //Destroy pixel and update arrays
                            collisionArray[tempX, tempY] = TerrainTypes.None;
                            foregroundColors[tempX + tempY * width] = Color.TransparentBlack;
                            result[(x + radius) + (y + radius) * (radius * 2 + 1)] = Color.TransparentBlack;
                        }
                        else
                            result[(x + radius) + (y + radius) * (radius * 2 + 1)] = foregroundColors[tempX + tempY * width];
                    }
                }
            }
            return result;
        }

        public void SetTextureColor(Color destructibleColor, Color indestructibleColor, int variance)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (collisionArray[x, y] == TerrainTypes.Destructible)
                    {
                        //There is a pixel here.
                        //foregroundColors[x + y * width] = new Color((byte)(color.R + 100), color.G, color.B);

                        foregroundColors[x + y * width] = RandomizeColor(destructibleColor, variance);
                    }
                    else if (collisionArray[x, y] == TerrainTypes.Indestructible)
                    {
                        //The pixel here is indestructible
                        foregroundColors[x + y * width] = RandomizeColor(indestructibleColor, variance);
                    }
                    else
                    {
                        //There is no pixel here.
                        foregroundColors[x + y * width] = Color.TransparentBlack;
                    }
                }
            }
            isTextureInitialized = true;
        }
        /* Adds grass to destructible terrain */
        public void AddGrass(Color grass, int variance, int thickness)
        {
            int grassCounter = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y == 0)
                        grassCounter = 0;
                    if (collisionArray[x, y] == TerrainTypes.None) //No terrain
                        grassCounter = thickness;
                    else if (collisionArray[x, y] == TerrainTypes.Indestructible)
                        grassCounter = 0;
                    else if (grassCounter > 0)
                    {
                        foregroundColors[x + y * width] = RandomizeColor(grass, variance);
                        grassCounter--;
                    }
                }
            }
        }
        /* 
         * Randomizes a color with a given variance. 
         * Uses a global random generator to avoid reuse, so can not be static 
         */
        protected Color RandomizeColor(Color color, int variance)
        {
            byte r = (byte)Utility.Bound(color.R + ((rand.NextDouble() * 2 - 1) * variance), 0, 255);
            byte g = (byte)Utility.Bound(color.G + ((rand.NextDouble() * 2 - 1) * variance), 0, 255);
            byte b = (byte)Utility.Bound(color.B + ((rand.NextDouble() * 2 - 1) * variance), 0, 255);
            return new Color(r, g, b);
        }
    }
    /* Terrain made from an image file */
    public class CustomTerrain : Terrain
    {
        /* Colors lower than this brightness threshold will count as indestructible */
        private int _thresholdDestructibleTerrain = 50;
        public int ThresholdDestructibleTerrain
        {
            get { return _thresholdDestructibleTerrain; }
            protected set { _thresholdDestructibleTerrain = Utility.Bound(value, 0, ThresholdNoTerrain); }
        }
        /* Colors higher than the previous threshold and lower than this threshold will count as no terrain */
        private int _thresholdNoTerrain = 200;
        public int ThresholdNoTerrain
        {
            get { return _thresholdNoTerrain; }
            protected set { _thresholdNoTerrain = Utility.Bound(value, ThresholdDestructibleTerrain, 255); }
        }
        public CustomTerrain(int width, int height, Color[] colors)
            : base(width, height)
        {
            foregroundColors = colors;
        }
        /* Creates collision array from the blue value in each pixel */
        protected override void GenerateTerrainImpl(int seed)
        {
            int blue;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    blue = foregroundColors[Utility.Convert2DIndexTo1D(x, y, width)].B;
                    if (blue <= ThresholdDestructibleTerrain)
                        collisionArray[x, y] = TerrainTypes.Indestructible;
                    else if (blue <= ThresholdNoTerrain)
                        collisionArray[x, y] = TerrainTypes.Destructible;
                    else
                    {
                        collisionArray[x, y] = TerrainTypes.None;
                        foregroundColors[Utility.Convert2DIndexTo1D(x, y, width)] = Color.TransparentBlack;
                    }
                }
            }
            isTextureInitialized = true;
        }
    }
    /* Randomly generated terrain in one dimension (only on the ground or in the ceiling) */
    public class SimpleTerrain : Terrain
    {

        /// <summary>
        /// Gets or sets the maximum bound of the generated slope.
        /// </summary>
        /// <value>The top.</value>
        public double Top { get; set; }
        /// <summary>
        /// Gets or sets the minimum bound of the generated slope.
        /// </summary>
        /// <value>The bottom.</value>
        public double Bottom { get; set; }
        public SimpleTerrain(int width, int height)
            : this(width, height, 0.5f, 0.1f) { }
        public SimpleTerrain(int width, int height, float top, float bottom)
            : base(width, height)
        {
            Top = top;
            Bottom = bottom;
        }
        protected override void GenerateTerrainImpl(int seed)
        {

            seed = seed * width;
            int[] terrainContour = new int[width];
            double delta = Math.Abs(Top - Bottom);
            double min = Math.Min(Top, Bottom);
            bool isGround = Top > Bottom; //terrain only on ground or only on ceiling?
            for (int i = 0; i < width; i++)
            {
                double x = (noiseGenerator.Function(i + seed) + 1) / 2; //gives roughly 0.0-1.0
                //x = Math.Max(0.0, x);
                //x = Math.Min(1.0, x);

                terrainContour[i] = (int)(delta * height * x + min * height); // får et tall mellom 60 og 300
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y <= terrainContour[x])
                    {
                        if (isGround)
                            collisionArray[x, height - y - 1] = TerrainTypes.Destructible;
                        else
                            collisionArray[x, height - y - 1] = TerrainTypes.None;
                    }
                    else
                    {
                        if (!isGround)
                            collisionArray[x, height - y - 1] = TerrainTypes.Destructible;
                        else
                            collisionArray[x, height - y - 1] = TerrainTypes.None;
                    }
                }
            }
        }
    }
    /* Randomly generated terrain in two dimensions */
    public class RandomTerrain : Terrain
    {
        protected double _threshold = 0;
        public double Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }
        public RandomTerrain(int width, int height)
            : base(width, height) { }
        protected override void GenerateTerrainImpl(int seed)
        {
            seed = seed * Math.Max(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double d = noiseGenerator.Function2D(x + seed, y + seed);
                    if (d > Threshold)
                    {
                        collisionArray[x, y] = TerrainTypes.Destructible;
                    }
                    else
                    {
                        collisionArray[x, y] = TerrainTypes.None;
                    }
                }
            }
        }
    }
}
