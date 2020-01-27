using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BunnyLand.Models
{
    public enum Resolution
    {
        Res_800x600,
        Res_1024x768,
        Res_1152x864,
        Res_1280x720,
        Res_1280x800,
        Res_1280x1024,
        Res_1366x768,
        Res_1440x900,
        Res_1600x900,
        Res_1600x1200,
        Res_1680x1050,
        Res_1920x1080,
        Res_1920x1200
    }

    public enum Setting
    {
        Low,
        Medium,
        High,
        VeryHigh
    }

    public static class Settings
    {
        public static Resolution Resolution { get; set; }
        public static bool FullScreen { get; set; }
        public static Setting EntityLimit { get; set; }
        public static Setting GoreLevel { get; set; }

        /// <summary>
        /// Stores the current settings in the file settings.ini
        /// </summary>
        public static void writeSettingsToIniFile()
        {
            TextWriter writer = new StreamWriter("settings.ini");
            writer.WriteLine("resolution = " + Resolution);
            writer.WriteLine("fullScreen = " + FullScreen);
            writer.WriteLine("entityLimit = " + EntityLimit);
            writer.WriteLine("goreLevel = " + GoreLevel);
            writer.Close();
        }
    }
}
