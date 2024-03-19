using CrossTermLib.Internals;
using FontStashSharp;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using TrippyGL;

namespace CrossTermLib;

public class TerminalGame : IDisposable
{
    private readonly IWindow _window;
    private readonly string _fontPath;

    private Renderer _renderer = null!;
    private FontSystem _fontSystem = null!;
    private GraphicsDevice _graphicsDevice = null!;

    private Vector2 _characterSize = Vector2.One;

    private char[,] _buffer;
    private Vector4[,] _colorBuffer;

    private bool _isDebugging = false;

    private bool _disposed = false;

    /// <summary>
    /// This number is multiplied by pixel width and height of 
    /// the 'W' character to calculate the character size.
    /// Character size is the width/height size of a cell.
    /// The whole window is divided into an evenly spaced grid (cols*rows).
    /// </summary>
    public float PaddingPercentage { get; private set; }
    public int PixelWidth { get; private set; }
    public int PixelHeight { get; private set; }
    public int Cols { get; private set; }
    public int Rows { get; private set; }
    public int FontSize { get; set; }
    public bool IsClosing { get; private set; } = false;

    public Vector4 DefaultFontColor { get; set; }
    public Vector4 BackgroundColor { get; set; }
    public event Action<double>? OnUpdate;
    public TerminalGame(
        int cols,
        int rows,
        string title,
        string fontPath,
        Vector4 backgroundColor,
        Vector4 defaultFontColor,
        int fontSize,
        float paddingPercentage)
    {
        _fontPath = fontPath;
        FontSize = fontSize;
        Cols = cols;
        Rows = rows;

        BackgroundColor = backgroundColor;
        DefaultFontColor = defaultFontColor;
        PaddingPercentage = paddingPercentage;

        var fontSettings = new FontSystemSettings
        {
            FontResolutionFactor = 2,
            KernelWidth = 2,
            KernelHeight = 2,
        };

        _fontSystem = new FontSystem(fontSettings);
        _fontSystem.AddFont(File.ReadAllBytes(_fontPath));

        var font = _fontSystem.GetFont(FontSize);
        var wSize = font.MeasureString("W");
        _characterSize = new Vector2(wSize.X, font.LineHeight) * PaddingPercentage;
        PixelWidth = (int)Math.Round(_characterSize.X * Cols);
        PixelHeight = (int)Math.Round(_characterSize.Y * Rows);

        _buffer = new char[Cols, Rows];
        _colorBuffer = new Vector4[Cols, Rows];
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                var c = ' ';
                var col = new Vector4(1f, 1f, 1f, 1f);
                _buffer[x, y] = c;
                _colorBuffer[x, y] = col;
            }
        }

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(PixelWidth, PixelHeight);
        options.Title = title;
        options.IsEventDriven = false;
        options.WindowBorder = WindowBorder.Fixed;

        _window = Window.Create(options);

        _window.Load += _window_Load;

        _window.Render += _window_Render;

        _window.Closing += _window_Closing;

        _window.Resize += _window_Resize;

        _window.StateChanged += _window_StateChanged;

        _window.Update += _window_Update;

        _window.Initialize();

    }

    private void _window_Update(double dt)
    {
        OnUpdate?.Invoke(dt);
    }

    public void Start()
    {
        _window.Run();
    }
    #region Window Events

    /// <summary>
    /// Tick will be called automatically if you are using a WriteLine and Readline flow. 
    /// <para> 
    ///     If you don't call either of those functions, you can call this directly in a while loop
    ///     to mimic a game loop
    ///  </para>
    /// </summary>
    public void Tick(bool addEmptyEvent)
    {
        if (addEmptyEvent)
            _window.ContinueEvents();

        _window.DoEvents();

        _window.DoRender();
    }

    /// <summary>
    /// This might only be needed for WSL
    /// </summary>
    private void _window_StateChanged(WindowState obj)
    {
        if (obj != WindowState.Normal)
        {
            _window.WindowState = WindowState.Normal;
            Tick(true);
        }
    }

    private void _window_Resize(Vector2D<int> size)
    {
        if (IsClosing)
            return;

        _graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
        _renderer.OnViewportChanged();

        Tick(false);
    }

    private void _window_Closing()
    {
        IsClosing = true;
        _graphicsDevice?.Dispose();
        _fontSystem?.Dispose();
    }

    private void _window_Load()
    {
        _graphicsDevice = new GraphicsDevice(GL.GetApi(_window))
        {
            BlendState = BlendState.AlphaBlend,
            RasterizerEnabled = true
        };

        _renderer = new Renderer(_graphicsDevice);

        var input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += Terminal_KeyDown;
            input.Keyboards[i].KeyChar += Terminal_KeyChar;
        }

        _window_Resize(_window.Size);
    }

    private void _window_Render(double dt)
    {
        if (IsClosing)
            return;

        _graphicsDevice.ClearColor = BackgroundColor;
        _graphicsDevice.Clear(ClearBuffers.Color);

        _renderer.Begin();

        var font = _fontSystem.GetFont(FontSize);

        DrawBuffer(font);
        if (_isDebugging)
        {
            DrawFPS(font, dt);
        }

        _renderer.End();
    }

    #endregion

    #region Commands 
    public void Clear()
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                _buffer[x, y] = ' ';
                _colorBuffer[x, y] = Vector4.One;
            }
        }
        Tick(true);
    }

    public void PutChar(char c, int x, int y, Vector4? color = null)
    {
        var col = color ?? Vector4.One;

        if (x < 0 || x > Cols - 1 || y < 0 || y > Rows - 1)
            return;

        _buffer[x, y] = c;
        _colorBuffer[x, y] = col;
    }

    public char GetChar(int x, int y)
    {
        if (x < 0 || x > Cols - 1 || y < 0 || y > Rows - 1)
            return ' ';

        return _buffer[x, y];
    }
    public (char c, Vector4 color) GetCharColor(int x, int y)
    {
        if (x < 0 || x > Cols - 1 || y < 0 || y > Rows - 1)
            return (' ', Vector4.Zero);

        return (_buffer[x, y], _colorBuffer[x, y]);
    }

    #endregion

    #region Drawing

    private void DrawBuffer(SpriteFontBase font)
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                var xPos = x * _characterSize.X;
                var yPos = y * _characterSize.Y;
                font.DrawText(_renderer, _buffer[x, y].ToString(), new Vector2(xPos, yPos), _colorBuffer[x, y].ToFS());
            }
        }
    }

    private void DrawFPS(SpriteFontBase font, double dt)
    {
        var fpsTxt = $"FPS: {(int)Math.Round(1f / (float)dt)}";
        var scl = Vector2.One;
        var sz = font.MeasureString(fpsTxt, scl);
        var orgn = Vector2.Zero;

        font.DrawText(_renderer, fpsTxt, new Vector2(2f, _window.Size.Y - sz.Y - 2), FSColor.Yellow, 0f, orgn, scl);

    }
    #endregion

    #region Input

    private void Terminal_KeyChar(IKeyboard arg1, char arg2)
    {
        //_buffer[_currentCol, _currentRow] = arg2;
        //if (_currentCol < Cols - 1)
        //    _currentCol++;

        _window.DoRender();
    }


    private void Terminal_KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Escape)
        {
            _window.Close();
        }
        //else if (arg2 == Key.Enter)
        //{
        //    WriteLine(string.Empty);
        //    _entered = true;
        //}
        else if (arg2 == Key.GraveAccent)
        {
            _isDebugging = !_isDebugging;
        }
        //else if (arg2 == Key.Backspace)
        //{
        //    //this is kind of weird behavior... ?
        //    if (_currentCol == Cols - 1 && _buffer[_currentCol, _currentRow] != ' ')
        //    {
        //        _buffer[_currentCol, _currentRow] = ' ';
        //    }
        //    else
        //    {

        //        if (_currentCol > 0)
        //            _currentCol--;

        //        _buffer[_currentCol, _currentRow] = ' ';
        //    }
        //}

        _window.DoRender();
    }

    #endregion

    #region Disposal

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _graphicsDevice?.Dispose();
                _fontSystem?.Dispose();
                _window?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Terminal()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}