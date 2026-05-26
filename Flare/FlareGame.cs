using Flare.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Rectangle = System.Drawing.Rectangle;

namespace Flare;

public abstract class FlareGame:IDisposable
{
    protected readonly WindowOptions WindowOptions;
    protected readonly IWindow Window;
   
    protected bool ExitOnEscape = true;
    protected IGraphicsDevice GraphicsDevice = null!;
    protected ImGuiController ImGuiController = null!;
    
    protected FlareGame(string title, int width, int height) : this(WindowOptions.Default with
    {
        Size = new Vector2D<int>(width, height), Title = title, FramesPerSecond = 60
    })
    {
    }
    protected FlareGame(WindowOptions windowOptions)
    {
        WindowOptions = windowOptions;
        Window = Silk.NET.Windowing.Window.Create(WindowOptions);
        
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
        ImGuiController = new ImGuiController(gl, Silk.NET.Windowing.Window.GetView(),inputContext);
        Initialize();
        
    }
    
    private void FlareDestroy()
    {
        ImGuiController.Dispose();
        GraphicsDevice.Dispose();
        Input.Dispose();
        Destroy();
    }
    private void FlareUpdate(double deltaTime)
    {
       
        if(ExitOnEscape && Input.IsKeyDown(Key.Escape))
            Window.Close();
        Update(deltaTime);
        //INPUT UPDATE CLEARS THE INPUT BUFFERS FOR PRESSED/RELEASED THUS MUST BE ALWAYS CALLED AT END OF LOOP
        Input.Update(deltaTime);
        ImGuiController.Update((float)deltaTime);
    }

    private void FlareRender(double deltaTime)
    {
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