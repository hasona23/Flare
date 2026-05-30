using System.Drawing;
using System.Numerics;
using Flare.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Rectangle = System.Drawing.Rectangle;
using Texture = Flare.Rendering.Texture;

namespace Flare;

public abstract class FlareCore : IDisposable
{
    protected readonly WindowOptions WindowOptions;
    protected readonly IWindow Window;
    public readonly Vector2 Resolution;
    protected bool ExitOnEscape = true;
    public IGraphicsDevice GraphicsDevice { get; private set; }= null!;
    protected ImGuiController ImGuiController = null!;
    public AssetManager AssetManager = new AssetManager();
    public static Texture Pixel;
    public static Texture Circle;

    protected FlareCore(string title, int width, int height) : this(WindowOptions.Default with
    {
        Size = new Vector2D<int>(width, height), Title = title, FramesPerSecond = 60
    })
    {
    }

    protected FlareCore(WindowOptions windowOptions)
    {
        WindowOptions = windowOptions;
        Window = Silk.NET.Windowing.Window.Create(WindowOptions);
        Resolution = new Vector2(windowOptions.Size.X, windowOptions.Size.Y);
        
        Window.Load += FlareInit;
        Window.Update += FlareUpdate;
        Window.Render += FlareRender;
        Window.Closing += FlareDestroy;
        Window.FramebufferResize += OnFrameBufferResize;

        Logger.LogInfo("Game Created");
    }

    private void OnFrameBufferResize(Vector2D<int> newSize)
    {
        GraphicsDevice.Viewport = new Rectangle(0, 0, newSize.X, newSize.Y);
    }

    private void FlareInit()
    {
        IInputContext inputContext = Window.CreateInput();
        Input.InitializeInput(inputContext);
        GL gl = Window.CreateOpenGL();
        GraphicsDevice = new OpenGLGraphicsDevice(gl);
        ImGuiController = new ImGuiController(gl, Silk.NET.Windowing.Window.GetView(), inputContext);
        TextureConfig textureConfig = new TextureConfig(GenerateMipmaps:false);
        Pixel = GraphicsDevice.CreateTexture(1, 1, "Pixel",[255, 255, 255, 255], ref textureConfig);
        int diameter = 32;
        Span<Color> colors = new Color[diameter * diameter];
        Vector2 center = new Vector2(diameter / 2f) - Vector2.One / 2;
        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float distanceSquared = (center - new Vector2(x, y)).LengthSquared();
                if (distanceSquared <= (diameter * diameter / 4f))
                {
                    colors[y * diameter + x] = (((center - new Vector2(x, y)).LengthSquared() <= diameter * diameter / 4f)
                        ? Color.White
                        : Color.Transparent);
                }
            }
        }

        Span<byte> data = stackalloc byte[diameter * diameter * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            int dataIndex = i * 4;
            data[dataIndex] = colors[i].R;
            data[dataIndex + 1] = colors[i].G;
            data[dataIndex + 2] = colors[i].B;
            data[dataIndex + 3] = colors[i].A;
        }

        Circle = GraphicsDevice.CreateTexture(diameter, diameter,"Circle", data, ref textureConfig);

        Initialize();
    }

    private void FlareDestroy()
    {
        GraphicsDevice.DestroyTexture(ref Pixel);
        GraphicsDevice.DestroyTexture(ref Circle);
        ImGuiController.Dispose();
        GraphicsDevice.Dispose();
        Input.Dispose();
        Destroy();
    }

    private void FlareUpdate(double deltaTime)
    {
        if (ExitOnEscape && Input.IsKeyDown(Key.Escape))
            Window.Close();
        Time.UpdateUps(deltaTime);
        Update(deltaTime);
        //INPUT UPDATE CLEARS THE INPUT BUFFERS FOR PRESSED/RELEASED THUS MUST BE ALWAYS CALLED AT END OF LOOP
        Input.Update(deltaTime);
        ImGuiController.Update((float)deltaTime);
    }

    private void FlareRender(double deltaTime)
    {
        Time.UpdateFps(deltaTime);
        Render(deltaTime);
        ImGuiController.Render();
    }

    public void Dispose()
    {
        Window.Dispose();
        Logger.LogInfo("Game Disposed");
    }

    protected abstract void Initialize();
    protected abstract void Update(double deltaTime);
    protected abstract void Render(double deltaTime);
    protected abstract void Destroy();


    public void Run()
    {
        Logger.LogInfo("Game Begin Running");
        Window.Run();
        Logger.LogInfo("Game End Running");
    }
}