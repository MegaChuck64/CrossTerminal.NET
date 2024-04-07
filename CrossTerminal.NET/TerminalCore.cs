using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;
using TrippyGL;

namespace CrossTerminal.NET;


internal class TerminalCore : IDisposable
{
    private bool disposedValue;

    public Renderer Renderer { get; private set; } = null!;
    public GraphicsDevice GraphicsDevice { get; private set; } = null!;
    public IInputContext Input { get; private set; } = null!;
    public IWindow Window { get; private set; }
    public static Color DefaultFontColor { get; set; }

    public bool IsClosing { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action? Load;
    public event Action<char>? CharKeyDown;
    public event Action<Key>? KeyDown;
    public event Action<float>? Render;
    public event Action<float>? Update;
    public event Action<Vector2D<int>>? Resize;
    public event Action? Closing;

    public TerminalCore(WindowOptions options)
    {
        Window = Silk.NET.Windowing.Window.Create(options);

        Window.Load += CoreWindow_Load;
        Window.Render += CoreWindow_Render;
        Window.Update += CoreWindow_Update;
        Window.Resize += CoreWindow_Resize;
        Window.Closing += CoreWindow_Closing;
        Window.StateChanged += CoreWindow_StateChanged;

        Window.Initialize();

        while (!IsLoaded) ;
    }

    private void CoreWindow_StateChanged(WindowState obj)
    {
        if (obj != WindowState.Normal)
        {
            Window.WindowState = WindowState.Normal;
            Tick(true);
        }
    }

    private void CoreWindow_Load()
    {
        GraphicsDevice = new GraphicsDevice(GL.GetApi(Window))
        {
            BlendState = BlendState.AlphaBlend,
            RasterizerEnabled = true
        };

        Renderer = new Renderer(GraphicsDevice);

        Input = Window.CreateInput();
        for (int i = 0; i < Input.Keyboards.Count; i++)
        {
            Input.Keyboards[i].KeyDown += TerminalCore_KeyDown;
            Input.Keyboards[i].KeyChar += TerminalCore_KeyChar;
        }

        Load?.Invoke();

        CoreWindow_Resize(Window.Size);
    }


    private void TerminalCore_KeyChar(IKeyboard arg1, char arg2)
    {
        CharKeyDown?.Invoke(arg2);
    }

    private void TerminalCore_KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Escape)
        {
            Window.Close();
        }

        KeyDown?.Invoke(arg2);
    }
    private void CoreWindow_Render(double obj)
    {
        if (IsClosing)
            return;

        IsLoaded = true;

        Render?.Invoke((float)obj);
    }

    private void CoreWindow_Update(double obj)
    {
        if (IsClosing)
            return;

        Update?.Invoke((float)obj);
    }
    private void CoreWindow_Resize(Vector2D<int> obj)
    {
        if (IsClosing)
            return;

        GraphicsDevice.SetViewport(0, 0, (uint)obj.X, (uint)obj.Y);
        Renderer.OnViewportChanged(GraphicsDevice);

        Resize?.Invoke(obj);

        Tick(false);
    }

    private void CoreWindow_Closing()
    {
        IsClosing = true;
        Closing?.Invoke();
    }

    public void ClearWindow(Vector4 backgroundColor)
    {
        GraphicsDevice.ClearColor = backgroundColor;
        GraphicsDevice.Clear(ClearBuffers.Color);
    }

    public void Tick(bool addEmptyEvent = true)
    {
        if (addEmptyEvent)
            Window.ContinueEvents();

        Window.DoEvents();

        Window.DoRender();
    }


    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                GraphicsDevice.Dispose();
                Window.Dispose();
                Input.Dispose();
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