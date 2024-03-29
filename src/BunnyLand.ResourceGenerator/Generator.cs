using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BunnyLand.ResourceGenerator;

public static class Generator
{
    public static void WriteSongs(string contentPath, TextWriter textWriter)
    {
        Write(contentPath, textWriter, "Songs", "mp3", "Song", "Microsoft.Xna.Framework.Media");
    }

    public static void WriteSpriteFonts(string contentPath, TextWriter textWriter)
    {
        Write(contentPath, textWriter, "SpriteFonts", "spritefont", "SpriteFont",
            "Microsoft.Xna.Framework.Graphics");
    }

    public static void WriteTextures(string contentPath, TextWriter textWriter)
    {
        Write(contentPath, textWriter, "Textures", "png", "Texture2D", "Microsoft.Xna.Framework.Graphics");
    }

    public static void WriteSoundEffects(string contentPath, TextWriter textWriter)
    {
        Write(contentPath, textWriter, "SoundEffects", "wav", "SoundEffect", "Microsoft.Xna.Framework.Audio");
    }

    private static void Write(string contentPath, TextWriter textWriter, string className, string extension,
        string propertyType, params string[] namespaces)
    {
        if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

        textWriter.Write($@"// <auto-generated />
// ReSharper disable All
using System.ComponentModel;
{string.Join(Environment.NewLine, namespaces.Select(ns => $"using {ns};"))}

namespace BunnyLand.DesktopGL.Resources
{{
    public partial class {className}
    {{
");

        foreach (var path in Directory.EnumerateFiles(contentPath, $"*.{extension}", EnumerationOptions)) {
            textWriter.Write($@"
        [Description(""{FilePathToRelativePath(path, extension)}"")]
        public {propertyType} {FilePathToPropertyName(path, extension)} {{ get; set; }}");
        }

        textWriter.Write(@"
    }
}
");
    }

    private static string FilePathToRelativePath(string path, string extension)
    {
        return path.Replace("\\", "/", StringComparison.OrdinalIgnoreCase).Split("Content/").Last()
            .Replace($".{extension}", "", StringComparison.OrdinalIgnoreCase);
    }

    private static readonly Regex InvalidPropertyNameRegex = new Regex("[^a-zA-Z0-9]");

    private static readonly EnumerationOptions EnumerationOptions = new EnumerationOptions {
        RecurseSubdirectories = true
    };

    private static string FilePathToPropertyName(string path, string extension)
    {
        var fileName = new FileInfo(path).Name.Replace($".{extension}", "", StringComparison.OrdinalIgnoreCase);
        var propertyName = InvalidPropertyNameRegex.Replace(fileName, "");
        return propertyName;
    }
}