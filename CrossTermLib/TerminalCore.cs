using CrossTermLib.Internals;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using TrippyGL;

namespace CrossTermLib;

internal class TerminalCore : IDisposable
{
    private bool disposedValue;

    public Renderer CoreRenderer { get; private set; } = null!;
    public GraphicsDevice CoreGraphicsDevice { get; private set; } = null!;
    public IInputContext CoreInput { get; private set; } = null!;
    public IWindow CoreWindow { get; private set; }
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
        CoreWindow = Window.Create(options);

        CoreWindow.Load += CoreWindow_Load;
        CoreWindow.Render += CoreWindow_Render;
        CoreWindow.Update += CoreWindow_Update;
        CoreWindow.Resize += CoreWindow_Resize;
        CoreWindow.Closing += CoreWindow_Closing;
        CoreWindow.StateChanged += CoreWindow_StateChanged;

        CoreWindow.Initialize();

        while (!IsLoaded) ;
    }

    private void CoreWindow_StateChanged(WindowState obj)
    {
        if (obj != WindowState.Normal)
        {
            CoreWindow.WindowState = WindowState.Normal;
            Tick(true);
        }
    }

    private void CoreWindow_Load()
    {
        CoreGraphicsDevice = new GraphicsDevice(GL.GetApi(CoreWindow))
        {
            BlendState = BlendState.AlphaBlend,
            RasterizerEnabled = true
        };

        CoreRenderer = new Renderer(CoreGraphicsDevice);

        CoreInput = CoreWindow.CreateInput();
        for (int i = 0; i < CoreInput.Keyboards.Count; i++)
        {
            CoreInput.Keyboards[i].KeyDown += TerminalCore_KeyDown;
            CoreInput.Keyboards[i].KeyChar += TerminalCore_KeyChar;
        }

        Load?.Invoke();

        CoreWindow_Resize(CoreWindow.Size);
    }

    public void Tick(bool addEmptyEvent = true)
    {        
        if (addEmptyEvent)
            CoreWindow.ContinueEvents();

        CoreWindow.DoEvents();

        CoreWindow.DoRender();        
    }

    private void TerminalCore_KeyChar(IKeyboard arg1, char arg2)
    {
        CharKeyDown?.Invoke(arg2);
    }

    private void TerminalCore_KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Escape)
        {
            CoreWindow.Close();
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

        CoreGraphicsDevice.SetViewport(0, 0, (uint)obj.X, (uint)obj.Y);
        CoreRenderer.OnViewportChanged();

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
        CoreGraphicsDevice.ClearColor = backgroundColor;
        CoreGraphicsDevice.Clear(ClearBuffers.Color);
    }



    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                CoreGraphicsDevice.Dispose();
                CoreWindow.Dispose();
                CoreInput.Dispose();
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