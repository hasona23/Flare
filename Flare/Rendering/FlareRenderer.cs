using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;
using Rectangle = System.Drawing.Rectangle;

namespace Flare.Rendering;

public class FlareRenderer : IDisposable
{
    private int _flushCount;
    public int FlushPerFrame { get; private set; }
    public IGraphicsDevice GraphicsDevice { get; private set; }
    public bool DrawWireFrame;
    private VertexArrayObject<Vertex> _vao;
    private BufferObject<Vertex> _vbo;
    private BufferObject<uint> _ebo;

    private Matrix4x4 DefaultTransform => Matrix4x4.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width,
        GraphicsDevice.Viewport.Height, 0, 0, 1);

    #region Buffers

    private readonly Vertex[] _vertices;
    private int _currentVertexIndex;
    private readonly uint[] _indices;
    private int _currentTriangleIndex;

    #endregion

    #region Texture and Shaders

    private Shader _defaultShader;
    private Shader _currentShader;
    private Texture _defaultTexture;
    //private Texture _currentTexture;
    private readonly List<Texture> _textureSlots = new List<Texture>(MAX_TEXTURE_SLOTS);

    #endregion

    #region Constants

    private const int MAX_TEXTURE_SLOTS = 16;
    private const int QUAD_VERTEX_COUNT = 4;
    private const int QUAD_INDICES_COUNT = 6;

    public const string DEFAULT_V_SHADER = @"
    #version 330 core
    layout (location=0) in vec3 position;
    layout (location=1) in vec4 color;
    layout (location=2) in vec3 normal;
    layout (location=3) in vec2 texCoords;
    layout (location=4) in int texId;

    uniform mat4 transform;

    out vec3 frag_position;
    out vec4 frag_color;
    out vec3 frag_normal;
    out vec2 frag_texCoords;
    flat out int frag_texId;
    void main()
    {
        gl_Position = transform * vec4(position,1);
        frag_position = position;
        frag_color = color;
        frag_normal = normal;
        frag_texCoords = texCoords;
        frag_texId = texId;
    }";

    public const string DEFAULT_F_SHADER = @"
    #version 330 core
    in vec3 frag_position;
    in vec4 frag_color;
    in vec3 frag_normal;
    in vec2 frag_texCoords;
    flat in int frag_texId;
    //16 is safety margin incase had 9 in buffer and couldnt flush
    //If user wants to pass a custom additional texrture then he sets a field/array for it
    uniform sampler2D tex[16];
    
    out vec4 FragColor;
    void main()
    {
        vec4 texColor = texture(tex[frag_texId],frag_texCoords);
        FragColor = frag_color * texColor;
    }
";

    #endregion


    public FlareRenderer(IGraphicsDevice graphicsDevice, int bufferSize = 65536)
    {
        GraphicsDevice = graphicsDevice;

        _vertices = new Vertex[bufferSize];
        _indices = new uint[bufferSize];

        _vbo = GraphicsDevice.CreateBufferObject(_vertices.AsSpan(), BufferTargetARB.ArrayBuffer,
            BufferUsageARB.StreamDraw);

        _ebo = GraphicsDevice.CreateBufferObject(_indices.AsSpan(), BufferTargetARB.ElementArrayBuffer,
            BufferUsageARB.StreamDraw);

        _vao = GraphicsDevice.CreateVao(ref _vbo, ref _ebo);
        Vertex.SetupVertexAttribPtr(ref _vao, GraphicsDevice);


        TextureConfig defaultTextureConfig = new TextureConfig();
        _defaultTexture = GraphicsDevice.CreateTexture(1, 1, [255, 255, 255, 255], ref defaultTextureConfig);

        _textureSlots.Add(_defaultTexture);

        _defaultShader = GraphicsDevice.CreateShader(DEFAULT_V_SHADER, DEFAULT_F_SHADER);
        ChangeShader(ref _defaultShader);

        graphicsDevice.SetShaderUniform(ref _defaultShader, "transform", DefaultTransform);

        Logger.LogInfo("Created sprite batch");
    }

    #region RenderingBackend

    public void Begin()
    {
        ClearBuffers();

        ChangeShader(ref _defaultShader);
        GraphicsDevice.SetShaderUniform(ref _defaultShader, "transform", DefaultTransform);
    }

    public void End()
    {
        Flush();
        FlushPerFrame = _flushCount;
        _flushCount = 0;
    }


    private void Flush()
    {
        if (_currentVertexIndex == 0)
            return;
        _flushCount++;
        GraphicsDevice.BindVao(ref _vao);

        GraphicsDevice.UploadBufferData(ref _vbo, _vertices);
        GraphicsDevice.UploadBufferData(ref _ebo, _indices);
        BindTextureSlots();
        GraphicsDevice.DrawVertices(_currentTriangleIndex, DrawWireFrame);

        ClearBuffers();
        _textureSlots.Clear();
        _textureSlots.Add(_defaultTexture);
    }

    public void BeginShader(ref Shader shader)
    {
        Flush();
        ChangeShader(ref shader);
        GraphicsDevice.SetShaderUniform(ref shader, "transform", DefaultTransform);
    }

    public void EndShader()
    {
        Flush();
        ChangeShader(ref _defaultShader);
    }

    public void BeginTransform(Matrix4x4 matrix)
    {
        Flush();
        GraphicsDevice.SetShaderUniform(ref _currentShader, "transform", matrix * DefaultTransform);
    }

    public void EndTransform()
    {
        Flush();
        GraphicsDevice.SetShaderUniform(ref _currentShader, "transform", DefaultTransform);
    }

    #endregion


    #region DrawTexture

    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor,
        Vector2 origin, Vector2 scale, float rotation, bool invertHorizontal, bool invertVertical)
    {
        int slot = GetTextureSlot(ref texture);
        
        EnsureBounds(QUAD_VERTEX_COUNT, QUAD_INDICES_COUNT);
        Color color = tintColor == default ? Color.White : tintColor;
       
        float uMin = (float)sourceRect.X / texture.Width;
        float vMin = (float)(texture.Height - sourceRect.Y - sourceRect.Height) / texture.Height;
        float uMax = (float)(sourceRect.X + sourceRect.Width) / texture.Width;
        float vMax = (float)(texture.Height - sourceRect.Y) / texture.Height;

        if (invertHorizontal)
        {
            (uMin, uMax) = (uMax, uMin);
        }

        if (invertVertical)
        {
            (vMin, vMax) = (vMax, vMin);
        }

        float width = sourceRect.Width * scale.X;
        float height = sourceRect.Height * scale.Y;


        uint startingIndex = (uint)_currentVertexIndex;
        uint[] quadIndices = GenerateQuadIndices(startingIndex);
        Span<Vertex> data = stackalloc Vertex[QUAD_VERTEX_COUNT];
        GenerateQuadVertices(data, position.AsVector3(), new Vector2(width, height),
            [color, color, color, color],
            new Vector2(uMin, vMin),
            new Vector2(uMax, vMax), slot);
        
        origin *= scale;
        Matrix4x4 transformationMatrix =
            Matrix4x4.CreateRotationZ(-rotation, position.AsVector3());
        for (int i = 0; i < QUAD_VERTEX_COUNT; i++)
        {
            data[i].Position -= origin.AsVector3();
            data[i].Position = Vector3.Transform(data[i].Position, transformationMatrix);
            _vertices[_currentVertexIndex + i] = data[i];
        }

        _currentVertexIndex += QUAD_VERTEX_COUNT;

        for (int i = 0; i < quadIndices.Length; i++)
        {
            _indices[_currentTriangleIndex + i] = quadIndices[i];
        }

        _currentTriangleIndex += quadIndices.Length;
    }

    public void DrawTexture(Texture texture, Vector2 position, Color tintColor)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
        Vector2.Zero,Vector2.One,0,false,false);
    }

    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,
            Vector2.Zero,Vector2.One,0,false,false);   
    }

    public void DrawTexture(Texture texture, Vector2 position, Color tintColor, Vector2 origin, float rotation)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
            origin,Vector2.One,rotation,false,false);    
    }
    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor, Vector2 origin,
        float rotation)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,
            origin,Vector2.One,rotation,false,false);       
    }
    public void DrawTexture(Texture texture, Vector2 position, Color tintColor, Vector2 scale)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
            Vector2.Zero,scale,0,false,false);    
    }
    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor, Vector2 scale)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,
            Vector2.Zero,scale,0,false,false);       
    }
    
    public void DrawTexture(Texture texture, Vector2 position, Color tintColor, Vector2 scale, Vector2 origin, float rotation)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
            origin,scale,rotation,false,false);    
    }
    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor, Vector2 scale,Vector2 origin, float rotation)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,origin,scale,rotation,false,false);       
    }
    public void DrawTexture(Texture texture, Vector2 position, Color tintColor,bool invertHorizontal,bool invertVertical)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
            Vector2.Zero,Vector2.One,0, invertHorizontal, invertVertical);
    }

    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor,bool invertHorizontal,bool invertVertical)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,
            Vector2.Zero,Vector2.One,0,invertHorizontal, invertVertical);   
    }

    public void DrawTexture(Texture texture, Vector2 position, Color tintColor, Vector2 origin, float rotation,bool invertHorizontal,bool invertVertical)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
            origin,Vector2.One,rotation,invertHorizontal, invertVertical);    
    }
    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor, Vector2 origin,
        float rotation,bool invertHorizontal,bool invertVertical)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,
            origin,Vector2.One,rotation,invertHorizontal,invertVertical);       
    }
    public void DrawTexture(Texture texture, Vector2 position, Color tintColor, Vector2 scale,bool invertHorizontal,bool invertVertical)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),new Rectangle(0,0,texture.Width,texture.Height), tintColor,
            Vector2.Zero,scale,0,invertHorizontal, invertVertical);    
    }
    public void DrawTexture(Texture texture, Vector2 position, Rectangle sourceRect, Color tintColor, Vector2 scale,bool invertHorizontal,bool invertVertical)
    {
        DrawTexture(texture, new Vector2(position.X, position.Y),sourceRect, tintColor,
            Vector2.Zero,scale,0,invertHorizontal,invertVertical);       
    }

    #endregion


    #region DrawShapes

    public void DrawTriangle(Vector2 point1, Vector2 point2, Vector2 point3, Color color)
    {
        int startingIndex = _currentVertexIndex;
        _vertices[_currentVertexIndex] = new Vertex(point1.AsVector3(),ColorToVector4(color));
        _vertices[_currentVertexIndex + 1] = new Vertex(point2.AsVector3(),ColorToVector4(color));
        _vertices[_currentVertexIndex + 2] = new Vertex(point3.AsVector3(),ColorToVector4(color));
        _currentVertexIndex += 3;
        for (int i = 0; i < 3; i++)
        {
            _indices[_currentTriangleIndex + i] = (uint)startingIndex + (uint)i;
        }

        _currentTriangleIndex += 3;
    }
    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 lineVector = end - start;
        if(lineVector.LengthSquared() > 0)
            lineVector = Vector2.Normalize(lineVector);
        
        Vector2 perpendicularLine = new Vector2(-lineVector.Y, lineVector.X);

        Span<Vector2> points = stackalloc Vector2[4];
       
        points[0] = new Vector2(start.X+perpendicularLine.X,start.Y+perpendicularLine.Y);
        points[1] = new Vector2(start.X - perpendicularLine.X, start.Y - perpendicularLine.Y);
        points[2] = new Vector2(end.X + perpendicularLine.X,end.Y + perpendicularLine.Y);
        points[3] = new Vector2(end.X - perpendicularLine.X,end.Y - perpendicularLine.Y);
        
        uint startIndex = (uint)_currentVertexIndex;
        uint[] quadIndices = GenerateQuadIndices(startIndex);
        for (var i = 0; i < points.Length; i++)
        {
            _vertices[_currentVertexIndex+i] = new Vertex(points[i].AsVector3(), ColorToVector4(color));
        }
        _currentVertexIndex += QUAD_VERTEX_COUNT;
        for(int i = 0; i< quadIndices.Length; i++)
            _indices[_currentTriangleIndex+i] = quadIndices[i];
        _currentTriangleIndex += quadIndices.Length;
    }
    public void DrawRectangle(Rectangle rectangle, Color color)
    {
        EnsureBounds(QUAD_VERTEX_COUNT, QUAD_INDICES_COUNT);

        Span<Vertex> data = stackalloc Vertex[QUAD_VERTEX_COUNT];
        GenerateQuadVertices(data, new Vector3(rectangle.X, rectangle.Y, 0),
            new Vector2(rectangle.Width, rectangle.Height), [color, color, color, color]);

        uint startingIndex = (uint)_currentVertexIndex;
        uint[] indices = GenerateQuadIndices(startingIndex);

        for (int i = 0; i < QUAD_VERTEX_COUNT; i++)
        {
            _vertices[_currentVertexIndex + i] = data[i];
        }

        _currentVertexIndex += QUAD_VERTEX_COUNT;

        for (int i = 0; i < indices.Length; i++)
        {
            _indices[_currentTriangleIndex + i] = indices[i];
        }

        _currentTriangleIndex += indices.Length;
    }

    public void DrawHollowRectangle(Rectangle rectangle, Color color, int thickness)
    {
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color); //TOP

        DrawRectangle(
            new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness),
            color); //BOTTOM

        DrawRectangle(new Rectangle(rectangle.X, rectangle.Y + thickness, thickness, rectangle.Height - thickness * 2),
            color); //RIGHT

        DrawRectangle(
            new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y + thickness, thickness,
                rectangle.Height - thickness * 2), color);
    }

    #endregion

    public void DrawImGui()
    {
        if (ImGui.Begin("SpriteBatch"))
        {
            ImGui.Checkbox("Wire Mode: ", ref DrawWireFrame);
            if (ImGui.CollapsingHeader("Texture slots"))
            {
                int i = 0;
                foreach (var textureSlot in _textureSlots)
                {
                    ImGui.Text($"{i++} - {textureSlot.Id} | {textureSlot.Width}x{textureSlot.Height}");
                }

                string error = GraphicsDevice.CheckErrors();
                if (!string.IsNullOrEmpty(error))
                {
                    ImGui.Text("OPENGL::ERROR " + error);
                }
            }

            if (ImGui.CollapsingHeader("Stats"))
            {
                ImGui.Text($"VERTICES: {_currentVertexIndex}");
                ImGui.Text($"INDICES: {_currentTriangleIndex}");
            }

            ImGui.End();
        }
    }

    public void Dispose()
    {
        GraphicsDevice.DestroyBufferObject(ref _ebo);
        GraphicsDevice.DestroyBufferObject(ref _vbo);
        GraphicsDevice.DestroyVao(ref _vao);
        GraphicsDevice.DestroyShader(ref _defaultShader);
        GraphicsDevice.DestroyTexture(ref _defaultTexture);
    }


    #region Utils

    private Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R, color.G, color.B, color.A) / 255f;
    }

    private void GenerateQuadVertices(Span<Vertex> data, Vector3 position, Vector2 size, Color[] color,
        Vector2 minTexCoords,
        Vector2 maxTexCoords, int slot, Vector3 normal)
    {
        data[0] = new Vertex(position, ColorToVector4(color[0]),
            new Vector2(minTexCoords.X, maxTexCoords.Y), slot, normal);

        data[1] = new Vertex(position with { X = position.X + size.X }, ColorToVector4(color[1]),
            new Vector2(maxTexCoords.X, maxTexCoords.Y), slot, normal);

        data[2] = new Vertex(position with { Y = position.Y + size.Y }, ColorToVector4(color[2]),
            new Vector2(minTexCoords.X, minTexCoords.Y), slot, normal);

        data[3] = new Vertex(position + size.AsVector3(), ColorToVector4(color[3]),
            new Vector2(maxTexCoords.X, minTexCoords.Y), slot, normal);
    }

    private void GenerateQuadVertices(Span<Vertex> data, Vector3 position, Vector2 size, Color[] color,
        Vector2 minTexCoords,
        Vector2 maxTexCoords, int slot)
    {
        GenerateQuadVertices(data, position, size, color, minTexCoords, maxTexCoords,
            slot, new Vector3(0, 0, 1));
    }

    private void GenerateQuadVertices(Span<Vertex> data, Vector3 position, Vector2 size, Color[] color, Vector3 normal)
    {
        GenerateQuadVertices(data, position, size, color, Vector2.Zero, Vector2.One, 0, normal);
    }

    private void GenerateQuadVertices(Span<Vertex> data, Vector3 position, Vector2 size, Color[] color)
    {
        GenerateQuadVertices(data, position, size, color, new Vector3(0, 0, 1));
    }

    private uint[] GenerateQuadIndices(uint startingIndex)
    {
        return
        [
            startingIndex, startingIndex + 2, startingIndex + 3,
            startingIndex, startingIndex + 1, startingIndex + 3
        ];
    }

    public void EnsureBounds(int additionalVerticesCount, int additionalIndicesCount)
    {
        if ((_currentVertexIndex + additionalVerticesCount >= _vertices.Length) ||
            (_currentTriangleIndex + additionalIndicesCount >= _indices.Length))
        {
            Flush();
        }
    }


    private void ClearBuffers()
    {
        Array.Clear(_vertices);
        Array.Clear(_indices);
        _currentVertexIndex = 0;
        _currentTriangleIndex = 0;
    }


    private void ChangeShader(ref Shader shader)
    {
        _currentShader = shader;
        GraphicsDevice.UseShader(ref _currentShader);
    }

    public void SetTextureUniform(string name, ref Shader shader, ref Texture texture)
    {
        //int slot = GetTextureSlot(ref texture);
        GraphicsDevice.BindTextureToSlot(ref texture, TextureUnit.Texture17);
        GraphicsDevice.SetShaderUniform(ref shader, name, TextureUnit.Texture17);
    }

    public void SetTextureUniform(string name, ref Shader shader, Texture[] textures)
    {
        for (int i = 0; i < textures.Length; i++)
        {
            GraphicsDevice.BindTextureToSlot(ref textures[i], TextureUnit.Texture17 + i);
            GraphicsDevice.SetShaderUniform(ref _currentShader, name, TextureUnit.Texture17 + i);
        }
    }

    private void BindTextureSlots()
    {
        for (int i = 0; i < _textureSlots.Count; i++)
        {
            Texture textureSlot = _textureSlots[i];
            GraphicsDevice.BindTextureToSlot(ref textureSlot, (TextureUnit)((int)TextureUnit.Texture0 + i));
        }

        int[] samplers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        GraphicsDevice.SetShaderUniform(ref _currentShader, "tex", samplers);
    }

    private int GetTextureSlot(ref Texture texture)
    {
        for (int i = 0; i < _textureSlots.Count; i++)
        {
            if (texture.Id == _textureSlots[i].Id)
            {
                return i;
            }
        }

        if (_textureSlots.Count > MAX_TEXTURE_SLOTS)
        {
            Flush();
        }

        _textureSlots.Add(texture);
        return _textureSlots.Count - 1;
    }

    #endregion
}