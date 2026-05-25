using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Flare.Rendering;

public class SpriteBatch : IDisposable
{
    public IRenderer Renderer { get; private set; }
    public IGraphicsDevice GraphicsDevice { get; private set; }
    public bool DrawWireFrame = false;
    private VertexArrayObject<Vertex> _vao;
    private BufferObject<Vertex> _vbo;
    private BufferObject<uint> _ebo;
    private Vertex[] _vertices;
    private int _currentVertexIndex = 0;
    private uint[] _indices;
    private int _currentTriangleIndex = 0;
    private readonly int _vertexBufferSize;

    private Shader _defaultShader;
    private Shader _currentShader;
    private Texture _defaultTexture;
    private Texture _currentTexture;
    private List<Texture> _textureSlots = new List<Texture>(MAX_TEXTURE_SLOTS);
    private const int MAX_TEXTURE_SLOTS = 16;

    private const string DEFAULT_V_SHADER = @"
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

    private const string DEFAULT_F_SHADER = @"
    #version 330 core
    in vec3 frag_position;
    in vec4 frag_color;
    in vec3 frag_normal;
    in vec2 frag_texCoords;
    flat in int frag_texId;

    uniform sampler2D tex[16];
    
    out vec4 FragColor;
    void main()
    {
        vec4 texColor = texture(tex[frag_texId],frag_texCoords);
        FragColor = frag_color * texColor;
    }
";

    //private bool IsActiveCustomShader => _defaultShader.ProgramId == _activeShader.ProgramId;

    public SpriteBatch(IRenderer renderer, IGraphicsDevice graphicsDevice, int vertexBufferSize = 65536)
    {
        Renderer = renderer;
        GraphicsDevice = graphicsDevice;

        _vertices = new Vertex[vertexBufferSize];

        _indices = new uint[vertexBufferSize * sizeof(uint)];
        _vertexBufferSize = vertexBufferSize;

        _vbo = GraphicsDevice.CreateBufferObject(new Span<Vertex>(_vertices), BufferTargetARB.ArrayBuffer,
            BufferUsageARB.StreamDraw);
        _ebo = GraphicsDevice.CreateBufferObject(new Span<uint>(_indices), BufferTargetARB.ElementArrayBuffer,
            BufferUsageARB.StreamDraw);
        _vao = GraphicsDevice.CreateVertexArrayObject(_vbo, _ebo);

        Vertex.SetupVertexAttribPtr(_vao, GraphicsDevice);

        GraphicsDevice.CompileShader(DEFAULT_V_SHADER, DEFAULT_F_SHADER, out _defaultShader);
        GraphicsDevice.CreateTexture(1, 1, [255, 255, 255, 255], out _defaultTexture);

        Renderer.BindTextureToSlot(_defaultTexture, TextureUnit.Texture0);

        Rectangle viewport = renderer.Viewport;
        graphicsDevice.SetShaderUniform(ref _defaultShader,"transform",Matrix4x4.CreateOrthographicOffCenter(0,viewport.Width,viewport.Height,0,0,1));
        Logger.LogInfo("Created sprite batch");
    }


    public void Begin()
    {
        Array.Clear(_vertices);
        Array.Clear(_indices);
        _currentVertexIndex = 0;
        _currentTriangleIndex = 0;
        _textureSlots.Clear();
        _textureSlots.Add(_defaultTexture);
        _currentShader = _defaultShader;
    }

    public void End()
    {
        Flush();
    }

    public void SetTextureUniform(string name, ref Shader shader, Texture texture)
    {
        int slot = -1;
        for (var i = 0; i < _textureSlots.Count; i++)
        {
            if (_textureSlots[i] == texture)
                slot = i;
        }

        if (slot == -1)
        {
            _textureSlots.Add(texture);
        }

        //TODO: IMPLEMENT MECHANISM INCASE PASSED LIMIT OF 16 TEXTURE SLOTS (0,1 ARE FOR DEFAULT AND USER TEXTURES) 
        slot = _textureSlots.IndexOf(texture);
        Renderer.BindTextureToSlot(_textureSlots[slot], TextureUnit.Texture0);
        GraphicsDevice.SetShaderUniform(ref _currentShader, name, (TextureUnit)((int)TextureUnit.Texture0 + slot));
    }

    private void Flush()
    {
        if (_currentVertexIndex == 0)
            return;

        Renderer.SetActiveShader(_currentShader);
        Renderer.BindVao(_vao);

        Renderer.UploadBufferData(_vbo, _vertices);
        Renderer.UploadBufferData(_ebo, _indices);

        // 1. Bind active texture slots normally
        for (int i = 0; i < _textureSlots.Count; i++)
            Renderer.BindTextureToSlot(_textureSlots[i], (TextureUnit)((int)TextureUnit.Texture0 + i));

        // 2. CRITICAL FIX: Assign values to uniform sampler2D tex[16] array slots
        int[] samplers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        GraphicsDevice.SetShaderUniform(ref _defaultShader,"tex",samplers);

        Renderer.DrawVertices(_currentTriangleIndex, DrawWireFrame);

        Array.Clear(_vertices);
        Array.Clear(_indices);
        _currentVertexIndex = 0;
        _currentTriangleIndex = 0;
        _textureSlots.Clear();
        _textureSlots.Add(_defaultTexture);
    }

    public void BeginShader(Shader shader)
    {
        Flush();
        _currentShader = shader;
    }

    public void EndShader()
    {
        Flush();
        _currentShader = _defaultShader;
    }

    public void BeginTransform(Matrix4x4 matrix)
    {
        Rectangle viewport = Renderer.Viewport;
        GraphicsDevice.SetShaderUniform(ref _defaultShader, "transform", 
            matrix * Matrix4x4.CreateOrthographicOffCenter(0,viewport.Width,viewport.Height,0,0,1));
    }

    public void EndTransform()
    {
        Rectangle viewport = Renderer.Viewport;
        GraphicsDevice.SetShaderUniform(ref _defaultShader,"transform",Matrix4x4.CreateOrthographicOffCenter(0,viewport.Width,viewport.Height,0,0,1));

    }

    public void DrawTexture(Texture texture, Vector2 position,float scale = 1, Color tintColor = default, Rectangle? sourceRect = null)
    {
        EnsureBounds(4, 6);
        // 1. Default fallback parameters
        Vector4 color = tintColor == default
            ? Vector4.One
            : new Vector4(tintColor.R, tintColor.G, tintColor.B, tintColor.A) / 255f;
       
        float uMin = 0f, vMin = 0f;
        float uMax = 1f, vMax = 1f;

        // 2. If a source rectangle segment is requested, compute fractional UV coordinates
        if (sourceRect.HasValue)
        {
            Rectangle srcRect = sourceRect.Value;
            uMin = (float)srcRect.X / texture.Width;
            vMin = (float)((texture.Height-srcRect.Y) - srcRect.Height) / texture.Height;
            uMax = (float)(srcRect.X + srcRect.Width) / texture.Width;
            vMax = (float)(texture.Height - srcRect.Y) / texture.Height;
           
        }

        int index = _textureSlots.IndexOf(texture);
        if (index == -1)
        {
            if (_textureSlots.Count >= MAX_TEXTURE_SLOTS)
                Flush();
            _textureSlots.Add(texture);
            index = _textureSlots.IndexOf(texture);
            Renderer.BindTextureToSlot(texture, TextureUnit.Texture0 + index);
        }

        // 4. Construct the 4 Quad Vertices using Normalized Screen coordinates
        // Slot 1 UV calculation passes 'slot' cast to a float if using multi-sampler shading maps
        float width = texture.Width * scale;
        float height = texture.Height * scale;
        if (sourceRect.HasValue)
        {
            width = sourceRect.Value.Width * scale;
            height = sourceRect.Value.Height * scale;
        }
        Span<Vector3> points = stackalloc Vector3[]
        {
            new Vector3(position.X, position.Y, 0f),
            new Vector3(position.X + width, position.Y, 0f),
            new Vector3(position.X, position.Y + height, 0f),
            new Vector3(position.X +width, position.Y + height, 0f),
        };
        Span<Vector2> uvs = stackalloc Vector2[]
        {
            new Vector2(uMin, vMax),
            new Vector2(uMax, vMax),
            new Vector2(uMin, vMin),
            new Vector2(uMax, vMin),
        };
        // 5. Append indexes and arrays smoothly into global batch elements
        uint startingIndex = (uint)_currentVertexIndex;
        uint[] quadIndices = GenerateQuadIndices(startingIndex);
        Span<Vertex> vertices = stackalloc Vertex[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            _vertices[_currentVertexIndex + i] = new Vertex(points[i], color, uvs[i], index);
        }

        _currentVertexIndex += vertices.Length;

        for (int i = 0; i < quadIndices.Length; i++)
        {
            _indices[_currentTriangleIndex + i] = quadIndices[i];
        }

        _currentTriangleIndex += quadIndices.Length;
    }

    private uint[] GenerateQuadIndices(uint startingIndex)
    {
        return
        [
            startingIndex, startingIndex + 2, startingIndex + 3,
            startingIndex, startingIndex + 1, startingIndex + 3
        ];
    }

   
    

    public void DrawRectangle(Rectangle rectangle, Color color)
    {
        EnsureBounds(4, 6);
        Renderer.BindTextureToSlot(_defaultTexture, TextureUnit.Texture0);
        float x = rectangle.X;
        float y = rectangle.Y;
        float height = rectangle.Height;
        float width = rectangle.Width;
        Vector4 colorVec = new Vector4(color.R, color.G, color.B, color.A) / 255f;
        Span<Vector3> points = stackalloc Vector3[4];
        points[0] = new Vector3(x, y, 0);
        points[1] = new Vector3(x + width, y, 0);
        points[2] = new Vector3(x, y + height, 0);
        points[3] = new Vector3(x + width, y + height, 0);

        uint startingIndex = (uint)_currentVertexIndex;
        uint[] indices = GenerateQuadIndices(startingIndex);

        for (int i = 0; i < points.Length; i++)
        {
            _vertices[_currentVertexIndex + i] =
                new Vertex(points[i], colorVec);
        }

        _currentVertexIndex += points.Length;

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

    public void Dispose()
    {
        GraphicsDevice.DestroyBufferObject(ref _ebo);
        GraphicsDevice.DestroyBufferObject(ref _vbo);
        GraphicsDevice.DestroyVertexArrayObject(ref _vao);
        GraphicsDevice.DestroyShader(ref _defaultShader);
        GraphicsDevice.DestroyTexture(ref _defaultTexture);
    }

    public void EnsureBounds(int newVerticesCount, int newIndicesCount)
    {
        if (_currentVertexIndex + newVerticesCount >= _vertices.Length)
            Flush();
        if (_currentVertexIndex + newIndicesCount >= _indices.Length)
            Flush();
    }
}