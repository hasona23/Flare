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

    private Texture _texture;

    private static TextureConfig DefaultTextureConfig = new TextureConfig(GenerateMipmaps: false);
    private FlareRenderer _renderer;

    protected override void Initialize()
    {
        _renderer = new FlareRenderer(GraphicsDevice);
        _texture = GraphicsDevice.LoadTexture(
            "./Resources/Textures/SpriteSheet.png",
            ref DefaultTextureConfig);
    }


    protected override void Update(double deltaTime)
    {
    }

    private Vector2 pos = new Vector2(200,200);
    private int x=0,y=0,width=16,height=16;
    private Vector2 origin;
    private Vector2 scale = new Vector2(20);
    private float rotation;
    private bool invertHorizontal;
    private bool invertVertical;
    protected override void Render(double deltaTime)
    {
        GraphicsDevice.Clear(Color.DarkSlateBlue);

        _renderer.Begin();
        _renderer.DrawTexture(_texture,pos,new Rectangle(x,y,width,height),Color.White,origin,scale,rotation,invertHorizontal,invertVertical);
        _renderer.DrawRectangle(new Rectangle((int)(origin.X-16+pos.X),(int)(origin.Y-16+pos.Y),32,32),Color.Red);
        _renderer.End();
        _renderer.DrawImGui();
        if (ImGui.Begin("DEBUG WINDOW"))
        {
            ImGui.InputFloat2("POSITION", ref pos);
            ImGui.InputInt("X", ref x);
            //ImGui.SameLine();
            ImGui.InputInt("Y", ref y);
            ImGui.InputInt("WIDTH", ref width);
            //ImGui.SameLine();
            ImGui.InputInt("HEIGHT", ref height);

            ImGui.InputFloat2("ORIGIN", ref origin);
            ImGui.InputFloat2("SCALE", ref scale);
            ImGui.InputFloat("ROTATION", ref rotation);
            ImGui.Checkbox("Invert Horizontal", ref invertHorizontal);
            ImGui.Checkbox("Invert Vertical", ref invertVertical);
            ImGui.End();
        }
    }

    protected override void Destroy()
    {
    }
}

//BENCHMARK:
// begin rendering 1500
// rendering 29400 - 90000
// end rendering 6400