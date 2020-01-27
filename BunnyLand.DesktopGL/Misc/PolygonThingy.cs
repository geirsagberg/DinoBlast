using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BunnyLand.DesktopGL.Misc
{
    public class PolygonThingy
    {
        public VertexPositionColor[] vertices;
        public short[] triangleVertexOrder;

        public PolygonThingy(bool variant1)
        {
            vertices = new VertexPositionColor[4];
            triangleVertexOrder = new short[6];

            if (variant1) {
                vertices[0].Position = new Vector3(-0.2f, -0.2f, 0.0f);    // 0
                vertices[1].Position = new Vector3(0.5f, -0.2f, 0.0f);   // 1
                vertices[2].Position = new Vector3(0.5f, 0.5f, 0.0f);  // 2
                vertices[3].Position = new Vector3(-0.2f, 0.5f, 0.0f);   // 3

                vertices[0].Color = new Color(Color.Red, 1);
                vertices[1].Color = new Color(Color.Green, 1);
                vertices[2].Color = new Color(Color.Blue, 1);
                vertices[3].Color = new Color(Color.Black, 1);

                // Clockwise ordering of vertices
                // First triangle
                triangleVertexOrder[0] = 0;
                triangleVertexOrder[1] = 2;
                triangleVertexOrder[2] = 1;
                // Second triangle
                triangleVertexOrder[3] = 2;
                triangleVertexOrder[4] = 0;
                triangleVertexOrder[5] = 3;
            } else {
                vertices[0].Position = new Vector3(-0.5f, -0.5f, 0.0f);    // 0
                vertices[1].Position = new Vector3(-0.5f, -0.9f, 0.0f);   // 1
                vertices[2].Position = new Vector3(-0.9f, -0.9f, 0.0f);  // 2
                vertices[3].Position = new Vector3(-0.9f, -0.5f, 0.0f);   // 3

                vertices[0].Color = new Color(Color.Red, 1);
                vertices[1].Color = new Color(Color.Green, 1);
                vertices[2].Color = new Color(Color.Blue, 1);
                vertices[3].Color = new Color(Color.Black, 1);

                // Clockwise ordering of vertices
                // First triangle
                triangleVertexOrder[0] = 0;
                triangleVertexOrder[1] = 1;
                triangleVertexOrder[2] = 2;
                // Second triangle
                triangleVertexOrder[3] = 2;
                triangleVertexOrder[4] = 3;
                triangleVertexOrder[5] = 0;
            }
        }
    }
}
