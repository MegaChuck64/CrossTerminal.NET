using System.Drawing;
using System.Numerics;
using TrippyGL;
using TrippyGL.Fonts.Building;
using TrippyGL.Fonts.Extensions;

namespace CrossTerminal.NET;

public class Font
{
    public TextureFont SpriteFont { get; private set; }    
    public float Size { get; private set; }
    public Font(string fileName, float size, GraphicsDevice graphicsDevice)
    {
        Size = size;
        var fontFile = FontBuilderExtensions.CreateFontFile(fileName, Size);
        SpriteFont = fontFile.CreateFont(graphicsDevice);
    }

    public Vector2 MeasureString(string text) => SpriteFont.Measure(text);
    public float LineHeight => SpriteFont.MeasureHeight("|");    
    public void Draw(TextureBatcher textureBatcher, string text, Vector2 position, Color color)
    {
        if (SpriteFont == null) return;
        if (string.IsNullOrWhiteSpace(text)) return;

        textureBatcher.DrawString(SpriteFont, text, position, new Color4b(color.R, color.G, color.B, color.A));
    }
}