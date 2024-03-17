using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using FontStashSharp;
using System.Numerics;
using TrippyGL;
using CrossTermLib.Internals;

namespace CrossTermLib;

public class Terminal : IDisposable
{
    private readonly IWindow _window;
    private readonly string _fontPath;
    private readonly List<string> _lines = [];

    private Renderer _renderer = null!;
    private FontSystem _fontSystem = null!;
    private GraphicsDevice _graphicsDevice = null!;

    private string _currentLine = string.Empty;

    private bool _entered = false;
    private bool _isDebugging = false;
    private bool _loaded = false;
    private bool _disposed = false;
    private bool _showCursor = false;
    private float _cursorTimer = 0f;
    private int _cursorCol = 0;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsClosing { get; private set; } = false;
    public float CursorBlinkSpeed { get; set; }
    public Vector4 BackgroundColor { get; set; }
    public int FontSize { get; set; }
    public Vector4 FontColor { get; set; }

    /// <summary>
    /// Blocks until first render
    /// </summary>
    public Terminal(int w, int h, string title, string fontPath, float cursorBlinkSpeed, Vector4 backgroundColor, Vector4 fontColor, int fontSize)
    {
        _fontPath = fontPath;
        FontSize = fontSize;
        Width = w;
        Height = h;
        CursorBlinkSpeed = cursorBlinkSpeed;
        BackgroundColor = backgroundColor;
        FontColor = fontColor;

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(w, h);
        options.Title = title;
        options.IsEventDriven = true;       
        options.WindowBorder = WindowBorder.Fixed;
        
        _window = Window.Create(options);
        
        _window.Load += _window_Load;

        _window.Render += _window_Render;

        _window.Closing += _window_Closing;

        _window.Resize += _window_Resize;

        _window.StateChanged += _window_StateChanged;

        _window.Initialize();

        while (!_loaded) ;
    }

    #region Console Commands 

    /// <summary>
    /// Blocking until user presses enter
    /// </summary>
    /// <returns></returns>
    public string ReadLine()
    {
        while (!_entered && !IsClosing)
        {
            _window.ContinueEvents();
            _window.DoEvents();
            _window.DoRender();
        }


        _entered = false;

        return _lines.Last();
    }
    public void WriteLine(string msg)
    {
        _lines.Add(msg);
        _window.DoRender();
    }

    public void Clear()
    {
        _lines.Clear();

        _window.ContinueEvents();
        _window.DoEvents();
        _window.DoRender();
    }

    #endregion

    #region Window Events

    private void _window_StateChanged(WindowState obj)
    {
        if (obj != WindowState.Normal)
            _window.WindowState = WindowState.Normal;
    }
    private void _window_Resize(Vector2D<int> size)
    {
        if (IsClosing)
            return;

        _graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
        _renderer.OnViewportChanged();


        _window.ContinueEvents();
        _window.DoEvents();
        _window.DoRender();
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

        var fontSettings = new FontSystemSettings
        {
            FontResolutionFactor = 4,
            KernelWidth = 1,
            KernelHeight = 1,
        };

        _fontSystem = new FontSystem(fontSettings);
        _fontSystem.AddFont(File.ReadAllBytes(_fontPath));
        _window_Resize(_window.Size);
    }

    private void _window_Render(double dt)
    {
        if (IsClosing)
            return;

        _loaded = true;

        _graphicsDevice.ClearColor = BackgroundColor;
        _graphicsDevice.Clear(ClearBuffers.Color);

        _renderer.Begin();
        var font = _fontSystem.GetFont(FontSize);
        var y = 2f;
        foreach (var line in _lines)
        {
            var sz = font.MeasureString(line, Vector2.One);            
            DrawLine(line, font, 2f, y, FontColor.ToFS());
            y += (sz.Y *1.5f);
        }

        _cursorTimer += (float)dt;
        if (_cursorTimer > CursorBlinkSpeed)
        {
            _cursorTimer = 0f;
            _showCursor = !_showCursor;
        }

        DrawLine(_currentLine, font, 2f, y, FontColor.ToFS(), _showCursor);

        if (_isDebugging)
        {
            DrawFPS(font, dt);
        }
        
        _renderer.End();
    }

    #endregion

    #region Drawing
    
    private void DrawLine(string text, SpriteFontBase font, float x, float y, FSColor color, bool drawCursor = false)
    {
        var scale = new Vector2(1, 1);
        var origin = new Vector2(0, 0);
        
        font.DrawText(_renderer, text, new Vector2(x, y), color, 0f, origin, scale);
        if (drawCursor)
        {
            var cursPos = _cursorCol > text.Length - 1 ? text.Length - 1 : _cursorCol;
            var shrtStr = font.MeasureString(new string(text.Take(cursPos + 1).ToArray()), scale);
            var xPos = shrtStr.X + 2;
            font.DrawText(_renderer, "_", new Vector2(xPos, y + 4), color, 0f, origin, scale);
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
        _currentLine += arg2;
        _cursorCol++;
        _window.DoRender();
    }


    private void Terminal_KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Escape)
        {
            _window.Close();
        }
        else if (arg2 == Key.Enter)
        {
            _lines.Add(_currentLine);
            _currentLine = string.Empty;
            _entered = true;
        }
        else if (arg2 == Key.GraveAccent)
        {
            _isDebugging = !_isDebugging;
        }
        else if (arg2 == Key.Backspace)
        {
            if (_currentLine?.Length > 0)
                _currentLine = _currentLine.Remove(_currentLine.Length - 1);
        }

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