using FontStashSharp;
using System.Numerics;
using TrippyGL;

namespace CrossTermLib.Internals;

internal static class Utility
{

    public static Color4b ToTrippy(this FSColor c)
    {
        return new Color4b(c.R, c.G, c.B, c.A);
    }

    public static FSColor ToFS(this Vector4 c)
    {
        return new FSColor(c);
    }


}
