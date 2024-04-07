using System.Drawing;
using TrippyGL;
using TrippyGL.ImageSharp;

namespace CrossTerminal.NET;

public class Sprite
{
    public Texture2D Texture { get; private set; }
    public RectangleF Destination { get; set; }

    public Sprite(GraphicsDevice graphicsDevice, string fileName, RectangleF? destination = null)
    {
        Texture = Texture2DExtensions.FromFile(graphicsDevice, fileName);
        Destination = destination ?? new RectangleF(0, 0, Texture.Width, Texture.Height);
    }

    public void Draw(TextureBatcher textureBatch)
    {
        textureBatch.Draw(Texture, Destination);
    }
}