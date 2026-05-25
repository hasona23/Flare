using System.Numerics;
using Silk.NET.OpenGL;

namespace Flare.Rendering;

public interface IGraphicsDevice
{
    bool CompileShader(string vertexSource,string fragmentSource,out Shader shader);
    void DestroyShader(ref Shader shader);

    public BufferObject<TDataType> CreateBufferObject<TDataType>(Span<TDataType> data, BufferTargetARB bufferType,BufferUsageARB usage) 
        where TDataType : unmanaged;
    public void DestroyBufferObject<TDataType>(ref BufferObject<TDataType> bufferObject) 
        where TDataType : unmanaged;

    public VertexArrayObject<TVertexType> CreateVertexArrayObject<TVertexType>(BufferObject<TVertexType> vbo,
        BufferObject<uint> ebo)
        where TVertexType : unmanaged;

    public void DestroyVertexArrayObject<TVertexType>(ref VertexArrayObject<TVertexType> vertexArrayObject)
        where TVertexType : unmanaged;

    public void SetupVertexAttributePointer<TVertex>(VertexArrayObject<TVertex> vao, uint id, int count,
        VertexAttribPointerType type, int offSetBytes) where TVertex : unmanaged;
    public void SetupVertexAttributeIPointer<TVertex>(VertexArrayObject<TVertex> vao, uint id, int count,
        VertexAttribPointerType type, int offSetBytes)
        where TVertex : unmanaged;
    public bool CreateTexture(int width, int height,Span<byte> data, out Texture texture);
    public bool LoadTexture(string path, out Texture texture);
    public void DestroyTexture(ref Texture texture);
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

    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<uint> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<int> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<float> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<double> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Vector2> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Vector3> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Vector4> value);
    void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Matrix4x4> value, bool transpose = false);
}