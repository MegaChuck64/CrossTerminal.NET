using CrossTermLib.Internals;
using System.Numerics;

namespace CrossTermLib;

public class Terminal(int cols, int rows, string fontPath, int fontSize, string title, Vector4 defaultFontColor, Vector4? backgroundColor = null, float paddingPercentage = 1.4f) : IDisposable
{
    private readonly ColorTerminal _colorTerminal = new(
            cols,
            rows,
            fontPath,
            fontSize,
            title,
            cursorBlinkSpeed: 0.4f,
            paddingPercentage,
            defaultFontColor,
            backgroundColor ?? new Vector4(0f, 0f, 0f, 1f));

    private bool disposedValue;

    public bool IsClosing => _colorTerminal.IsClosing;
    public void Write(char c, Vector4? color = null) =>
        _colorTerminal.Write(new CharInfo(c, color));
    

    public void Write(string txt, Vector4? color = null) =>
        _colorTerminal.Write(new StringInfo(txt, color == null ? null : [ color.Value ]));
    

    public void WriteLine(string txt, Vector4? color = null) =>
        _colorTerminal.WriteLine(new StringInfo(txt, color == null ? null : [ color.Value ]));
    

    public string ReadLine() =>
        _colorTerminal.ReadLine().Text;

    //public char Read()
    //{
    //    var res = _colorTerminal.ReadChar();
    //    return res.C;
    //}

    public void Refresh() => _colorTerminal.Tick();
    public void Clear() =>
        _colorTerminal.Clear();

    public void SetCursorPosition(int x, int y)
    {
        _colorTerminal.CurrentCol = x;
        _colorTerminal.CurrentRow = y;
    }

    public (int x, int y) GetCursorPosition =>
        (_colorTerminal.CurrentCol, _colorTerminal.CurrentRow);

    public (int x, int y) GetWindowSize() =>
        (_colorTerminal.Cols, _colorTerminal.Rows);
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _colorTerminal.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

//public interface IConsole
//{
//    public void Write(char c);
//    public void Write(string text);
//    public void WriteLine(string text);

//    public string ReadLine();
//    public char Read();
//    public void Clear();

//    public (int x, int y) GetCursorPosition();
//    public void SetCursorPosition(int x, int y);

//    public void SetWindowPosition(int x, int y);
//    public void SetWindowSize(int w, int h);

//    public void SetBufferPosition(int x, int y);
//    public void SetBufferSize(int w, int h);
//}