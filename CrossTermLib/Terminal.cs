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

    private Renderer _renderer = null!;
    private FontSystem _fontSystem = null!;
    private GraphicsDevice _graphicsDevice = null!;

    private List<string> _lines = [];
    private string _currentLine = string.Empty;
    private bool _entered = false;
    private bool _isDebugging = false;
    private bool _loaded = false;
    private bool _disposed = false;
    private readonly string _fontPath;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public bool IsClosing { get; private set; } = false;

    /// <summary>
    /// Blocks until first render
    /// </summary>
    public Terminal(int w, int h, string title, string fontPath)
    {
        _fontPath = fontPath;
        Width = w;
        Height = h;

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

    private void _window_StateChanged(WindowState obj)
    {
        if (obj != WindowState.Normal)
            _window.WindowState = WindowState.Normal;
    }

    private void _window_Load()
    {
        _graphicsDevice = new GraphicsDevice(GL.GetApi(_window));
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
        }

        _window.DoRender();

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
    private void _window_Closing()
    {
        IsClosing = true;
        _graphicsDevice?.Dispose();
        _fontSystem?.Dispose();
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


    private void _window_Render(double dt)
    {
        if (IsClosing)
            return;

        _loaded = true;

        _graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
        _graphicsDevice.Clear(ClearBuffers.Color);

        _renderer.Begin();
        var font = _fontSystem.GetFont(12);
        var y = 2f;
        foreach (var line in _lines)
        {
            DrawLine(line, font, 2f, y, FSColor.Green);
            y += (font.LineHeight + 4);
        }

        var text = $"?> {_currentLine}";
        DrawLine(text, font, 2f, y + (font.LineHeight + 8), FSColor.LightGreen);

        if (_isDebugging)
        {
            DrawFPS(font, dt);
        }
        
        _renderer.End();
    }

    private void DrawLine(string text, SpriteFontBase font, float x, float y, FSColor color)
    {
        var scale = new Vector2(1, 1);
        var origin = new Vector2(0, 0);

        font.DrawText(_renderer, text, new Vector2(x, y), color, 0f, origin, scale);
    }
    private void DrawFPS(SpriteFontBase font, double dt)
    {
        var fpsTxt = $"FPS: {(int)Math.Round(1f / (float)dt)}";
        var scl = Vector2.One;
        var sz = font.MeasureString(fpsTxt, scl);
        var orgn = Vector2.Zero;

        font.DrawText(_renderer, fpsTxt, new Vector2(2f, _window.Size.Y - sz.Y - 2), FSColor.Yellow, 0f, orgn, scl);

    }

    private void Terminal_KeyChar(IKeyboard arg1, char arg2)
    {
        _currentLine += arg2;
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

        _window.DoRender();
    }

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
}