using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.DesktopGL.Extensions;

// public static class SpriteBatchExtensions
// {
//     private static Texture2D lineTexture = null!;
//     public static void DrawLine(this SpriteBatch spriteBatch, Color color, Vector2 start, Vector2 end,
//         float width = 1, float layerDepth = 0)
//     {
//         EnsureLineTexture(spriteBatch.GraphicsDevice);
//
//         var angle = Math.Atan2(end.Y - start.Y, end.X - start.X).ToFloat();
//         var length = (end - start).Length();
//
//         // TODO: Calculate width
//
//         spriteBatch.Draw(lineTexture, start, null, color, angle, Vector2.Zero, new Vector2(length, 1),
//             SpriteEffects.None, layerDepth);
//     }
//
//     private static void EnsureLineTexture(GraphicsDevice graphicsDevice)
//     {
//         if (lineTexture == null) {
//             lineTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
//             lineTexture.SetData(new[] {Color.White});
//         }
//     }
// }