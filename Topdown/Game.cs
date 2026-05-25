using System.Drawing;
using System.Numerics;
using System.Text;
using Flare;
using Flare.Rendering;
using ImGuiNET;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using StbImageSharp;
using Shader = Flare.Rendering.Shader;
using Texture = Flare.Rendering.Texture;

namespace Topdown;

public class Game() : FlareGame(TITLE, WIDTH, HEIGHT)
{
    public const string TITLE = "Topdown";
    public const int WIDTH = 1280;
    public const int HEIGHT = 720;
    private SpriteBatch _spriteBatch;
    private Texture _spriteTexture;
    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(Renderer, GraphicsDevice, 4);
        GraphicsDevice.LoadTexture(
            "path",
            out _spriteTexture);
    }


    protected override void Update(double deltaTime)
    {
        if (Renderer is OpenGLRenderer glRenderer)
        {
            GLEnum err = glRenderer.GL.GetError();
            if (err != GLEnum.NoError)
                Logger.LogError(err.ToString());
        }

        if (Input.IsKeyPressed(Key.Enter))
        {
            _spriteBatch.DrawWireFrame = !_spriteBatch.DrawWireFrame;
        }

        
    }

   

    protected override void Render(double deltaTime)
    {
       
        Renderer.Clear(Color.DarkSlateBlue);
        _spriteBatch.Begin();
        _spriteBatch.BeginTransform(Matrix4x4.CreateRotationZ(-MathF.PI/8,Vector3.Zero));
        _spriteBatch.DrawRectangle(
            new Rectangle(Renderer.Viewport.Width / 2 - 250, Renderer.Viewport.Height / 2 - 250, 500, 500),
            Color.DarkGoldenrod);
        _spriteBatch.DrawHollowRectangle(new Rectangle(0, 0, 200, 300), Color.CornflowerBlue, 10);
        _spriteBatch.DrawTexture(_spriteTexture, new Vector2(300,300),4);
        _spriteBatch.EndTransform();
        _spriteBatch.DrawTexture(_spriteTexture, new Vector2(300,100),4,Color.White,new Rectangle(0,80,32,16));
        _spriteBatch.DrawTexture(_spriteTexture, new Vector2(500,100),4,Color.CornflowerBlue,new Rectangle(0,80,32,16));
        _spriteBatch.End();
        
        ImGui.ShowDemoWindow();
    }

    protected override void Destroy()
    {
    }
}