using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using BunnyLand.Models;

namespace BunnyLand
{
    /* Implements diverse utility functions */
    static class Utility
    {

        public static Random Random = new Random();

        /* 
         * Returns the number x bounded within a minimum and maximum value, inclusive. 
         * Returns x if min > max. 
         */
        public static double Bound(double x, double min, double max)
        {
            if (min > max)
                return x;
            else
                return (x < min ? Math.Max(min, x) : Math.Min(max, x));
        }
        public static float Bound(float x, float min, float max)
        {
            if (min > max)
                return x;
            else
                return (x < min ? Math.Max(min, x) : Math.Min(max, x));
        }
        public static int Bound(int x, int min, int max)
        {
            if (min > max)
                return x;
            else
                return (x < min ? Math.Max(min, x) : Math.Min(max, x));
        }
        public static int Convert2DIndexTo1D(int x, int y, int width)
        {
            return x + y * width;
        }

        /// <summary>
        /// Converts the Vector2 to an angle.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>Angle within (-PI,PI)</returns>
        public static float ToAngle(Vector2 vector)
        {
            return MathHelper.WrapAngle((float)Math.Atan2(vector.Y, vector.X));
        }

        /// <summary>
        /// Creates a Vector2 from the given angle and length.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static Vector2 ToVector(float angle, float length)
        {
            angle = MathHelper.WrapAngle(angle);
            return new Vector2(length * (float)Math.Cos(angle), length * (float)Math.Sin(angle));
        }

        public static Vector2 ToVector(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point ToPoint(Vector2 vector2)
        {
            return new Point((int)vector2.X, (int)vector2.Y);
        }

        public static Random RandomGenerator = new Random();

        /// <summary>
        /// Reads a text file and return a list of strings, one string for each line in the file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> ReadFromFile(string path)
        {
            List<string> lines = new List<string>();
            try
            {
                TextReader reader = new StreamReader(path);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(!line.StartsWith("#")) //so we can use comments
                        lines.Add(line);
                }
                reader.Close();
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
            return lines;
        }

        /// <summary>
        /// Returns the aspect ratio of current resolution setting.
        /// </summary>
        /// <returns></returns>
        public static float GetAspectRatio()
        {
            string resolution = Settings.Resolution.ToString().Split('_')[1];   // Settings.Resolution looks like Res_1024x768.
            string[] dimensions = resolution.Split('x');
            int res_width = Int16.Parse(dimensions[0]);
            int res_height = Int16.Parse(dimensions[1]);
            return (float)res_width / res_height;
        }

        public static float RandomFloat(float min, float max)
        {
            return (max - min) * (float)Random.NextDouble() + min;
        }

        /// <summary>
        /// Returns a readable string representation of a Resolution
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string ResolutionToString(Resolution r)
        {
            return r.ToString().Split('_')[1];
        }

        /// <summary>
        /// Converts resolution enum to an int array with 2 members: resolution x and resolution y.
        /// </summary>
        /// <param name="r">The resolution enum.</param>
        /// <returns></returns>
        public static int[] ResolutionToInts(Resolution r)
        {
            int[] res = new int[2];
            char[] splitters = { '_', 'x' };
            string[] strings = r.ToString().Split(splitters);
            int.TryParse(strings[1], out res[0]);
            int.TryParse(strings[2], out res[1]);

            return res;
        }

        public static string MapSizeToString(MapSize size)
        {
            return size.ToString().Split('_')[1];
        }

        public static int[] MapSizeToInts(MapSize mapSize)
        {
            int[] res = new int[2];
            char[] splitters = { '_', 'x' };
            string[] strings = mapSize.ToString().Split(splitters);
            int.TryParse(strings[1], out res[0]);
            int.TryParse(strings[2], out res[1]);

            return res;
        }
    }
}
