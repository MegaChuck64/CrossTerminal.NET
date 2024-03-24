using CrossTermLib.Internals;
using FontStashSharp;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace CrossTermLib;

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

public struct CharInfo(char c, Vector4? color = null)
{
    public char C { get; set; } = c;
    public Vector4 Color { get; set; } = color ?? Vector4.One;
}

public struct StringInfo
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
                Colors[i] = Vector4.One;
            }
        }
        else if (colors.Length < text.Length)
        {
            //if not enough colors given, just fill the rest with white
            Colors = new Vector4[text.Length];
            colors.CopyTo(Colors, 0);
            for (int i = colors.Length - 1; i < text.Length; i++)
            {
                Colors[i] = Vector4.One;
            }
        }
        else
        {
            Colors = colors;
        }

        Text = text;
    }
}


internal class ColorTerminal : IDisposable
{
    private bool disposedValue;

    private readonly TerminalCore _core;
    private readonly FontSystem _fontSystem = null!;
    private readonly Vector2 _characterSize;

    private char[,] _buffer;
    private Vector4[,] _colorBuffer;
   
    private bool _entered = false;
    private bool _isDebugging = false;
    private float _cursorTimer = 0f;
    private bool _showCursor = false;

    public int Cols { get; private set; }
    public int Rows { get; private set; }
    public string Title { get; private set; }
    public float PaddingPercentage { get; private set; }
    public float FontSize { get; private set; }

    public int CurrentCol 
    {
        get { return _currentCol; }
        set 
        {
            if (value < Cols - 1)
                _currentCol = value;
        }
    }
    public int CurrentRow
    {
        get { return _currentRow; }
        set
        {
            if (value < Rows - 1)
                _currentRow = value;
        }
    }

    private int _currentCol;
    private int _currentRow;
    public int PixelWidth => _core.CoreWindow.Size.X;
    public int PixelHeight => _core.CoreWindow.Size.Y;

    public bool IsClosing => _core.IsClosing;

    public Vector4 DefaultFontColor { get; set; }
    public Vector4 BackgroundColor { get; set; }
    public float CursorBlinkRate { get; set; }


    public ColorTerminal(int cols, int rows, string fontPath, int fontSize, string title, float cursorBlinkSpeed, float paddingPercentage, Vector4 defaultFontColor, Vector4 backgroundColor)
    {
        Cols = cols;
        Rows = rows;
        CursorBlinkRate = cursorBlinkSpeed;
        PaddingPercentage = paddingPercentage;
        Title = title;
        FontSize = fontSize;
        DefaultFontColor = defaultFontColor;
        BackgroundColor = backgroundColor;

        var fontSettings = new FontSystemSettings
        {
            FontResolutionFactor = 2,
            KernelWidth = 2,
            KernelHeight = 2,
        };
        _fontSystem = new FontSystem(fontSettings);
        _fontSystem.AddFont(File.ReadAllBytes(fontPath));

        var font = _fontSystem.GetFont(FontSize);
        var wSize = font.MeasureString("W");
        _characterSize = new Vector2(wSize.X, font.LineHeight) * PaddingPercentage;

        _buffer = new char[Cols, Rows];
        _colorBuffer = new Vector4[Cols, Rows];
        Clear();

        var options = WindowOptions.Default with
        {
            IsEventDriven = true,
            Size = new Vector2D<int>((int)Math.Round(_characterSize.X * Cols), (int)Math.Round(_characterSize.Y * Rows)),
            Title = title,
            WindowBorder = WindowBorder.Fixed,
            
        };

        _core = new TerminalCore(options);

        _core.Render += _core_Render;
        _core.CharKeyDown += _core_CharKeyDown;
        _core.KeyDown += _core_KeyDown;
    }

    #region Events 
    private void _core_KeyDown(Key key)
    {
        if (key == Key.Enter)
        {
            _showCursor = false;
            _cursorTimer = 0f;
            WriteLine(new StringInfo(string.Empty));
            _entered = true;
        }
        else if (key == Key.GraveAccent)
        {
            _isDebugging = !_isDebugging;
        }
        else if (key == Key.Backspace)
        {
            //this is kind of weird behavior... ?
            if (CurrentCol == Cols - 1 && _buffer[CurrentCol, CurrentRow] != ' ')
            {
                _buffer[CurrentCol, CurrentRow] = ' ';
            }
            else
            {

                if (CurrentCol > 0)
                    _currentCol--;

                _buffer[CurrentCol, CurrentRow] = ' ';
            }
        }

        Tick();
    }

    private void _core_CharKeyDown(char c)
    {
        _buffer[CurrentCol, CurrentRow] = c;
        if (CurrentCol < Cols - 1)
            _currentCol++;

        Tick();
    }

    private void _core_Render(float dt)
    {
        _core.ClearWindow(BackgroundColor);

       _core.CoreRenderer.Begin();

        var font = _fontSystem.GetFont(FontSize);
        _cursorTimer += dt;

        DrawBuffer(font);
        if (_isDebugging)
        {
            DrawFPS(font, dt);
        }

        _core.CoreRenderer.End();
    }

    #endregion

    #region Drawing

    private void DrawBuffer(SpriteFontBase font)
    {
        if (_cursorTimer > CursorBlinkRate)
        {
            _cursorTimer = 0f;
            _showCursor = !_showCursor;
        }

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                var xPos = x * _characterSize.X;
                var yPos = y * _characterSize.Y;
                font.DrawText(_core.CoreRenderer, _buffer[x, y].ToString(), new Vector2(xPos, yPos), _colorBuffer[x, y].ToFS());
                if (_showCursor && x == CurrentCol && y == CurrentRow)
                {
                    font.DrawText(_core.CoreRenderer, "_", new Vector2(xPos, yPos + 2), _colorBuffer[x, y].ToFS());
                }
            }
        }
    }

    private void DrawFPS(SpriteFontBase font, double dt)
    {
        var fpsTxt = $"FPS: {(int)Math.Round(1f / (float)dt)}";
        var scl = Vector2.One;
        var sz = font.MeasureString(fpsTxt, scl);
        var orgn = Vector2.Zero;

        font.DrawText(_core.CoreRenderer, fpsTxt, new Vector2(2f, _core.CoreWindow.Size.Y - sz.Y - 2), FSColor.Yellow, 0f, orgn, scl);

    }
    #endregion

    #region Commands 

    public void Tick() => _core.Tick();
    public void Clear()
    {
        var spc = ' ';
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                _buffer[x, y] = spc;
                _colorBuffer[x, y] = DefaultFontColor;
            }
        }
        CurrentCol = 0;
        CurrentRow = 0;
    }

    public void AdvanceCursor()
    {
        _currentCol++;
        if (_currentCol > Cols - 1)
        {
            _currentCol = 0;
            _currentRow++;

            if (_currentRow > Rows - 1)
            {
                _currentRow = Rows - 1;
            }
        }
    }

    public CharInfo ReadChar()
    {
        var c = _buffer[CurrentCol, CurrentRow];
        var col = _colorBuffer[CurrentCol, CurrentRow];
        AdvanceCursor();

        return new CharInfo(c, col);
    }
    
    public StringInfo ReadLine()
    {
        while (!_entered && !_core.IsClosing)
        {
            Tick();
        }

        _entered = false;

        var line = string.Empty;
        var cols = new List<Vector4>();
        for (int i = 0; i < Cols; i++)
        {
            line += _buffer[i, CurrentRow - 1];
            cols.Add(_colorBuffer[i, CurrentRow - 1]);
        }
        line = line.TrimEnd();

        return new StringInfo(line, cols.Take(line.Length).ToArray());
    }

    public void Write(CharInfo c)
    {
        _buffer[CurrentCol, CurrentRow] = c.C;
        _colorBuffer[CurrentCol, CurrentRow] = c.Color;

        AdvanceCursor();
    }

    public void Write(StringInfo msg)
    {
        var ndx = _currentCol;

        for (int i = 0; i < msg.Text.Length; i++)
        {
            var c = msg.Text[i];
            var col = msg.Colors[i];

            _buffer[ndx, _currentRow] = c;
            _colorBuffer[ndx, _currentRow] = col;
            AdvanceCursor();

            ndx++;
            //todo: wrapping vs. cutoff? whats the word 
            if (ndx > Cols - 1)
                break;
        }                
    }

    public void WriteLine(StringInfo msg)
    {        
        var ndx = _currentCol;

        for (int i = 0; i < msg.Text.Length; i++)
        {
            var c = msg.Text[i];
            var col = msg.Colors[i];

            _buffer[ndx, _currentRow] = c;
            _colorBuffer[ndx, _currentRow] = col;

            ndx++;

            if (ndx > Cols - 1)
                break;
        }

        _currentCol = 0;
        _currentRow++;
        if (_currentRow > Rows - 1)
            _currentRow = Rows - 1;

    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _core?.Dispose();
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