using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

namespace CrossTerminal.NET;

public interface IConsole
{
    public void Write(char c);
    public void Write(string text);
    public void WriteLine(string text);

    public string ReadLine();
    public char Read();
    public void Clear();

    public (int x, int y) GetCursorPosition();
    public void SetCursorPosition(int x, int y);

    public void SetWindowPosition(int x, int y);
    public void SetWindowSize(int w, int h);

    public void SetBufferPosition(int x, int y);
    public void SetBufferSize(int w, int h);
}

public interface IColorConsole
{
    public void Write(ColorChar c);
    public void Write(ColorString text);
    public void WriteLine(ColorString text);

    public ColorString ReadLine();
    public ColorChar Read();
}

public struct ColorChar
{
    public char Char { get;  set; }
    public Color Color { get; set; }

    public ColorChar(char c, Color color)
    {
        Char = c;
        Color = color;
    }
}

public struct ColorString
{
    public IList<ColorChar> Chars { get; set; }

    public ColorString(string text, Func<char, int, Color> iterator)     
    {
        Chars = [];
        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            Chars.Add(new ColorChar(c, iterator?.Invoke(c, i) ?? Color.White));
        }
    }
    

    public ColorString(string text, IEnumerable<Color> colors)
    {
        ArgumentNullException.ThrowIfNull(text);

        ArgumentNullException.ThrowIfNull(colors);

        if (text.Length != colors.Count())
            throw new ArgumentException("string and colors array must be same size");

        Chars = [];
        for (int i = 0; i < text.Length; i++)
        {
            Chars.Add(new ColorChar(text[i], colors.ElementAt(i)));
        }        
    }

    public ColorString(IEnumerable<ColorChar> colorChars)
    {
        Chars = colorChars.ToList();
    }

    public readonly ColorChar this[int i]
    {
        get { return Chars[i]; }
        set { Chars[i] = value; }
    }

    public override readonly string ToString()
    {
        string s = string.Empty;
        foreach (var cc in Chars)
        {
            s += cc.Char;
        }
        return s;
    }
}

public class Terminal : IConsole, IColorConsole, IDisposable
{
    private bool disposedValue;

    private const string _charsCheck = @"123456789`~!@#$%^&*()-_=+qwertyuiop[{]}\|asdfghjkl;:'"";zxcvbnm,<.>/?QWERTYUIOPASDFGHJKLZXCVBNM";

    private readonly TerminalCore _core;
    private readonly Vector2 _characterSize;
    private readonly Font _font;
    private readonly ColorChar[,] _buffer;

    private bool _entered = false;
    private bool _keyTyped = false;
    private bool _isDebugging = false;
    private float _cursorTimer = 0f;
    private bool _showCursor = false;
    private int _currentCol;
    private int _currentRow;

    public int Cols { get; private set; }
    public int Rows { get; private set; }
    public string Title { get; private set; }
    public float PaddingPercentage { get; private set; }
    public float FontSize { get; private set; }
    public Color BackgroundColor { get; set; }
    public float CursorBlinkRate { get; set; }

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
    
    public int PixelWidth => _core.Window.Size.X;
    public int PixelHeight => _core.Window.Size.Y;

    public bool IsClosing => _core.IsClosing;

    private int GetCellWidth => (int)Math.Round(_characterSize.X * PaddingPercentage);
    private int GetCellHeight => (int)Math.Round(_characterSize.Y * PaddingPercentage);

    private Vector2D<int> GetCellSize => new(GetCellWidth, GetCellHeight);
    private Vector2D<int> GetWindowPixelSize => new(GetCellWidth * Cols, GetCellHeight * Rows);



    public Terminal(int cols, int rows, string fontPath, int fontSize, string title, float cursorBlinkSpeed, float paddingPercentage, Color defaultFontColor, Color backgroundColor)
    {
        Cols = cols;
        Rows = rows;
        CursorBlinkRate = cursorBlinkSpeed;
        PaddingPercentage = paddingPercentage;
        Title = title;
        FontSize = fontSize;
        BackgroundColor = backgroundColor;

        TerminalCore.DefaultFontColor = defaultFontColor;

        
        var options = WindowOptions.Default with
        {
            IsEventDriven = true,
            Size = new Vector2D<int>(100, 100),
            Title = title,
            WindowBorder = WindowBorder.Fixed,
        };

        _core = new TerminalCore(options);

        _font = new Font(fontPath, fontSize, _core.GraphicsDevice);

        var widest = WidestCharacterWidth(_font);
        var highest = TallestCharacterHeight(_font);
        _characterSize = new Vector2(widest, highest);

        SetWindowSize(Cols, Rows);

        _buffer = new ColorChar[Cols, Rows];
        Clear();

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
            WriteLine(new ColorString(string.Empty, (t, i) => Color.White));

            _entered = true;
        }
        else if (key == Key.GraveAccent)
        {
            _isDebugging = !_isDebugging;
        }
        else if (key == Key.Backspace)
        {
            _showCursor = true;
            _cursorTimer = 0f;

            //this is kind of weird behavior... ?
            if (CurrentCol == Cols - 1 && _buffer[CurrentCol, CurrentRow].Char != ' ')
            {
                _buffer[CurrentCol, CurrentRow].Char = ' ';
            }
            else
            {

                if (CurrentCol > 0)
                    _currentCol--;

                _buffer[CurrentCol, CurrentRow].Char = ' ';
            }
        }

        Tick();
    }

    private void _core_CharKeyDown(char c)
    {
        _showCursor = true;
        _cursorTimer = 0f;

        if (c == '`')
            return;

        _buffer[CurrentCol, CurrentRow].Char = c;
        if (CurrentCol < Cols - 1)
            _currentCol++;

        Tick();
    }

    private void _core_Render(float dt)
    {
        _core.ClearWindow(
            new Vector4(
                BackgroundColor.R, 
                BackgroundColor.G, 
                BackgroundColor.B, 
                BackgroundColor.A));

        _core.Renderer.Begin();

        _cursorTimer += dt;

        DrawBuffer();
        //if (_isDebugging)
        //{
        //    DrawFPS(font, dt);
        //}

        _core.Renderer.End();
    }


    private void DrawBuffer()
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
                var xPos = x * GetCellWidth;
                var yPos = y * GetCellHeight;
                _core.Renderer.DrawString(_font, _buffer[x, y].Char.ToString(), new Vector2(xPos, yPos), _buffer[x, y].Color);
                if (_showCursor && x == CurrentCol && y == CurrentRow)
                {
                    _core.Renderer.DrawString(_font, "_", new Vector2(xPos, yPos + 2), _buffer[x, y].Color);
                }
            }
        }
    }

    #endregion

    #region Helpers

    public void Tick() => _core.Tick();

    public void Clear()
    {
        var spc = ' ';
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                _buffer[x, y].Char = spc;
                _buffer[x, y].Color = TerminalCore.DefaultFontColor;
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

    private static float WidestCharacterWidth(Font font)
    {
        var w = 0f;
        var ch = ' ';
        foreach (var c in _charsCheck)
        {
            var width = font.MeasureString(c.ToString()).X;
            if (width > w)
            {
                w = width;
                ch = c;
            }
        }

        return w;
    }

    private static float TallestCharacterHeight(Font font)
    {
        var h = 0f;
        var ch = ' ';

        foreach (var c in _charsCheck)
        {
            var height = font.MeasureString(c.ToString()).Y;
            if (height > h)
            {
                h = height;
                ch = c;
            }
        }

        return h;
    }


    #endregion

    #region Window Management

    public (int x, int y) GetCursorPosition()
    {
        return (CurrentCol, CurrentRow);
    }

    public void SetBufferPosition(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void SetBufferSize(int w, int h)
    {
        throw new NotImplementedException();
    }

    public void SetCursorPosition(int x, int y)
    {
        CurrentCol = x;
        CurrentRow = y;
    }

    public void SetWindowPosition(int x, int y)
    {
        _core.Window.Position = new Vector2D<int>(x, y);
    }

    public void SetWindowSize(int w, int h)
    {
        _core.Window.Size = new Vector2D<int>(GetCellWidth * w, GetCellHeight * h);
        Cols = w;
        Rows = h;
    }
    
    #endregion

    #region Write
    public void Write(char c)
    {
        _buffer[_currentCol, _currentRow].Char = c;
        AdvanceCursor();
    }

    public void Write(string text)
    {
        var ndx = _currentCol;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];

            _buffer[ndx, _currentRow].Char = c;
            AdvanceCursor();

            ndx++;
            //todo: wrapping vs. cutoff? whats the word 
            if (ndx > Cols - 1)
                break;
        }
    }

    public void Write(ColorChar c)
    {
        _buffer[CurrentCol, CurrentRow] = c;

        AdvanceCursor();
    }

    public void Write(ColorString text)
    {

        for (int i = 0; i < text.Chars.Count; i++)
        {
            _buffer[CurrentCol, _currentRow] = text[i];

            AdvanceCursor();

            if (CurrentCol > Cols - 1)
                break;
        }
    }

    public void Write(char c, Color color) =>
        Write(new ColorChar(c, color));

    public void Write(string text, Color color) =>
        Write(new ColorString(text, (c, i) => color));

    public void Write(string text, Func<char, Color> iterator) =>
        Write(new ColorString(text, (c, i) => iterator.Invoke(c)));
    public void Write(string text, Func<char, int, Color> iterator) =>
        Write(new ColorString(text, iterator));

    public void Write(string text, IEnumerable<Color> colors) =>
        Write(new ColorString(text, colors));
    
    #endregion
    
    #region WriteLine

    public void WriteLine(string text)
    {
        var ndx = _currentCol;

        for (int i = 0; i < text.Length; i++)
        {
            _buffer[ndx, _currentRow].Char = text[i];

            ndx++;

            if (ndx > Cols - 1)
                break;
        }

        _currentCol = 0;
        _currentRow++;
        if (_currentRow > Rows - 1)
            _currentRow = Rows - 1;
    }

    public void WriteLine(ColorString text)
    {
        var ndx = _currentCol;

        for (int i = 0; i < text.Chars.Count; i++)
        {

            _buffer[ndx, _currentRow] = text[i];
            
            ndx++;

            if (ndx > Cols - 1)
                break;
        }

        _currentCol = 0;
        _currentRow++;
        if (_currentRow > Rows - 1)
            _currentRow = Rows - 1;

    }

    public void WriteLine(string text, Color color) =>
        WriteLine(new ColorString(text, (c, i) => color));

    public void WriteLine(string text, Func<char, int, Color> iterator) =>
        WriteLine(new ColorString(text, iterator));
    public void WriteLine(string text, Func<char, Color> iterator) =>
        WriteLine(new ColorString(text, (c, i) => iterator.Invoke(c)));

    public void WriteLine(string text, IEnumerable<Color> colors) =>
        WriteLine(new ColorString(text, colors));

    #endregion

    #region Read
    public char Read()
    {
        while (!_keyTyped && !_core.IsClosing)
        {
            Tick();
        }

        _keyTyped = false;

        var c = _buffer[_currentCol, _currentRow].Char;
        AdvanceCursor();
        return c;
    }

    ColorChar IColorConsole.Read()
    {
        while (!_keyTyped && !_core.IsClosing)
        {
            Tick();
        }

        _keyTyped = false;

        var c = _buffer[_currentCol, _currentRow];
        AdvanceCursor();
        return c;
    }

    #endregion

    #region ReadLine

    public string ReadLine()
    {
        while (!_entered && !_core.IsClosing)
        {
            Tick();
        }

        _entered = false;

        var line = string.Empty;
        for (int i = 0; i < Cols; i++)
        {
            line += _buffer[i, CurrentRow - 1].Char;
        }
        line = line.TrimEnd();

        return line;
    }

    ColorString IColorConsole.ReadLine()
    {
        while (!_entered && !_core.IsClosing)
        {
            Tick();
        }

        _entered = false;

        var line = string.Empty;
        var cols = new List<ColorChar>();
        for (int i = CurrentCol; i < Cols; i++)
        {
            line += _buffer[i, CurrentRow - 1].Char;
            cols.Add(_buffer[i, CurrentRow - 1]);
        }

        return new ColorString(
            cols.Take(line.TrimEnd().Length));
    }

    #endregion

    #region Dispose

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
    
    #endregion
}