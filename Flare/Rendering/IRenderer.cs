using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Flare.Rendering;

public interface IRenderer
{
    public void Clear(Color color);
    public void SetTextureFilter(TextureMagFilter magFilter, TextureMinFilter minFilter);
    public void SetTextureWrap(TextureWrapMode wrapX, TextureWrapMode wrapY);

    public void BindVao<TVertexType>(VertexArrayObject<TVertexType> vao)
        where TVertexType : unmanaged;

    public void UploadBufferData<T>(BufferObject<T> buffer, ReadOnlySpan<T> data) where T : unmanaged;
    
    public Rectangle Viewport { get; set; }

    public void BindTextureToSlot(Texture texture,TextureUnit slot);
  
    public void DrawVertices(int indicesCount,bool drawWireframe=false);
   
    public void SetActiveShader(Shader shader);

    

}