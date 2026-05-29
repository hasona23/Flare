using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Flare.Rendering;

public interface IGraphicsDevice : IDisposable
{
    public Rectangle Viewport { get; set; }
    public void Clear(Color color);
    public string CheckErrors();
    public void DrawVertices(int indicesCount, bool drawWireframe = false);

    public VertexArrayObject<TVertexType> CreateVao<TVertexType>(ref BufferObject<TVertexType> vbo,
        ref BufferObject<uint> ebo)
        where TVertexType : unmanaged;
    public void BindVao<TVertexType>(ref VertexArrayObject<TVertexType> vao)
        where TVertexType : unmanaged;

    
    public void SetupVertexAttributePointer<TVertex>(ref VertexArrayObject<TVertex> vao, uint index, int count,
        VertexAttribPointerType type, int offSetBytes) where TVertex : unmanaged;

    public void SetupVertexAttributeIPointer<TVertex>(ref VertexArrayObject<TVertex> vao, uint index, int count,
        VertexAttribPointerType type, int offSetBytes)
        where TVertex : unmanaged;
    public void DestroyVao<TVertexType>(ref VertexArrayObject<TVertexType> vertexArrayObject)
        where TVertexType : unmanaged;

    public BufferObject<TDataType> CreateBufferObject<TDataType>(Span<TDataType> data,
        BufferTargetARB bufferType, BufferUsageARB usage)
        where TDataType : unmanaged;

    public void UploadBufferData<T>(ref BufferObject<T> buffer, Span<T> data) where T : unmanaged;
    
    public void DestroyBufferObject<TDataType>(ref BufferObject<TDataType> bufferObject)
        where TDataType : unmanaged;


    public Texture CreateTexture(int width, int height,string name,Span<byte> data, ref TextureConfig config);
    public Texture LoadTexture(string path, ref TextureConfig config);
    public void BindTextureToSlot(ref Texture texture, TextureUnit slot);
    public void DestroyTexture(ref Texture texture);

    


    Shader CreateShader(string vertexSource, string fragmentSource);
    public void UseShader(ref Shader shader);
    void DestroyShader(ref Shader shader);


    public void SetShaderUniform(ref Shader shader, string name, TextureUnit textureSlot);
    void SetShaderUniform(ref Shader shader, string name, uint value);
    void SetShaderUniform(ref Shader shader, string name, float value);
    void SetShaderUniform(ref Shader shader, string name, int value);
    void SetShaderUniform(ref Shader shader, string name, double value);
    void SetShaderUniform(ref Shader shader, string name, bool value);
    void SetShaderUniform(ref Shader shader, string name, Vector2 value);
    void SetShaderUniform(ref Shader shader, string name, Vector3 value);
    void SetShaderUniform(ref Shader shader, string name, Vector4 value);
    void SetShaderUniform(ref Shader shader, string name, Matrix4x4 value, bool transpose = false);

    void SetShaderUniform(ref Shader shader, string name, Span<uint> value);
    void SetShaderUniform(ref Shader shader, string name, Span<int> value);
    void SetShaderUniform(ref Shader shader, string name, Span<float> value);
    void SetShaderUniform(ref Shader shader, string name, Span<double> value);
    void SetShaderUniform(ref Shader shader, string name, Span<Vector2> value);
    void SetShaderUniform(ref Shader shader, string name, Span<Vector3> value);
    void SetShaderUniform(ref Shader shader, string name, Span<Vector4> value);
    void SetShaderUniform(ref Shader shader, string name, Span<Matrix4x4> value, bool transpose = false);
}