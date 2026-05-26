using System.Drawing;
using System.Numerics;
using Flare;
using Flare.Rendering;
using ImGuiNET;
using Texture = Flare.Rendering.Texture;

namespace Topdown;

public class Game() : FlareGame(TITLE, WIDTH, HEIGHT)
{
    public const string TITLE = "Topdown";
    public const int WIDTH = 1280;
    public const int HEIGHT = 720;

   
   
    private static TextureConfig DefaultTextureConfig = new TextureConfig(GenerateMipmaps:true);
    private FlareRenderer _renderer;

    protected override void Initialize()
    {
        _renderer = new FlareRenderer(GraphicsDevice);
    }


    protected override void Update(double deltaTime)
    {
    }


    protected override void Render(double deltaTime)
    {
        GraphicsDevice.Clear(Color.DarkSlateBlue);

        _renderer.Begin();
        
        _renderer.End();
        _renderer.DrawImGui();
        ImGui.Text("FLUSHES: "+_renderer.FlushPerFrame);
    }

    protected override void Destroy()
    {
    }
}

//BENCHMARK:
// begin rendering 1500
// rendering 29400 - 90000
// end rendering 6400