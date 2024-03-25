using System.Numerics;

namespace CrossTermLib.Internals;
internal struct CharInfo(char c, Vector4? color = null)
{
    public char C { get; set; } = c;
    public Vector4 Color { get; set; } = color ?? TerminalCore.DefaultFontColor;
}

internal struct StringInfo
{
    public string Text { get; set; }
    public Vector4[] Colors { get; set; }

    public StringInfo(string text, Vector4[]? colors = null)
    {
        if (colors == null)
        {
            Colors = new Vector4[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                Colors[i] = TerminalCore.DefaultFontColor;
            }
        }
        else if (colors.Length < text.Length)
        {
            //if not enough colors given, just fill the rest with white
            Colors = new Vector4[text.Length];
            colors.CopyTo(Colors, 0);
            var firstCol = colors[0];
            for (int i = colors.Length; i < text.Length; i++)
            {
                Colors[i] = firstCol;
            }
        }
        else
        {
            Colors = colors;
        }

        Text = text;
    }
}
