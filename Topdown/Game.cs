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
    private Texture _mask;
    private static TextureConfig _defaultTextureConfig = new TextureConfig(GenerateMipmaps: false);
    private FlareRenderer _renderer = null!;
    private Shader _maskingShader;

    private const string F_SHADER =
        @"
#version 330 core
in vec2 frag_texCoords;
flat in int frag_texId;

uniform sampler2D tex[16];
uniform sampler2D mask;
uniform float _mixingFactor;

out vec4 FragColor;
void main()
{
    FragColor = mix(texture(tex[frag_texId], frag_texCoords), texture(mask, frag_texCoords), _mixingFactor);
}
";

    protected override void Initialize()
    {
        _renderer = new FlareRenderer(GraphicsDevice);
        _texture = GraphicsDevice.LoadTexture(
            "./Resources/Textures/SpriteSheet.png",
            ref _defaultTextureConfig);
        _mask = GraphicsDevice.LoadTexture(
            "./Resources/Textures/Sultan/SpriteSheet.png", ref _defaultTextureConfig);
        _maskingShader = GraphicsDevice.CreateShader(FlareRenderer.DEFAULT_V_SHADER, F_SHADER);
    }


    protected override void Update(double deltaTime)
    {
    }

    private Vector2 _pos = new Vector2(200,200);
    private int _x,_y,_width=16,_height=16;
    private Vector2 _origin;
    private Vector2 _scale = new Vector2(20);
    private float _rotation;
    private bool _invertHorizontal;
    private bool _invertVertical;
    private float _mixingFactor;
    private bool _useShader;
    private Vector2 _end = new Vector2(100,500);
    protected override void Render(double deltaTime)
    {
        GraphicsDevice.Clear(Color.DarkSlateBlue);

        _renderer.Begin();
        _end = Vector2.Transform(_end, Matrix4x4.CreateRotationZ(_rotation,new Vector3(100,100,0)));
        _renderer.DrawLine(new Vector2(100,100), _end,Color.HotPink,5);
        _renderer.DrawTriangle(new Vector2(100,100),new Vector2(100,150),new Vector2(200,300),Color.Wheat);
        if (_useShader)
        {
            _renderer.BeginShader(ref _maskingShader);
            _renderer.SetTextureUniform("mask", ref _maskingShader, ref _mask);
            _renderer.GraphicsDevice.SetShaderUniform(ref _maskingShader,"_mixingFactor",_mixingFactor);
        }

        _renderer.DrawTexture(_texture,_pos,new Rectangle(_x,_y,_width,_height),Color.White,_origin,_scale,_rotation,_invertHorizontal,_invertVertical);
        if(_useShader)
            _renderer.EndShader();
        _renderer.DrawRectangle(new Rectangle((int)(_origin.X-16+_pos.X),(int)(_origin.Y-16+_pos.Y),32,32),Color.Red);
        _renderer.End();
        _renderer.DrawImGui();
        if (ImGui.Begin("DEBUG WINDOW"))
        {
            ImGui.InputFloat2("POSITION", ref _pos);
            ImGui.InputInt("X", ref _x);
            //ImGui.SameLine();
            ImGui.InputInt("Y", ref _y);
            
            ImGui.InputInt("WIDTH", ref _width);
            //ImGui.SameLine();
            ImGui.InputInt("HEIGHT", ref _height);

            ImGui.InputFloat2("ORIGIN", ref _origin);
            ImGui.InputFloat2("SCALE", ref _scale);
            ImGui.InputFloat("ROTATION", ref _rotation);
            ImGui.Checkbox("Invert Horizontal", ref _invertHorizontal);
            ImGui.Checkbox("Invert Vertical", ref _invertVertical);
            ImGui.Checkbox("Use Shader", ref _useShader);
            ImGui.SliderFloat("Mixing Factor", ref _mixingFactor,0,1);
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