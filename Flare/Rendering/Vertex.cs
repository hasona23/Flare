using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Flare.Rendering;


[StructLayout(LayoutKind.Sequential)]
public struct Vertex(
    Vector3 position,
    Vector4 color = default,
    Vector2 texCoords = default,
    int textureIndex =0,
    Vector3 normal = default)
{
    
    public Vector3 Position = position;
    public Vector4 Color = color == default ? Vector4.One : color;
    public Vector3 Normal = normal;
    public Vector2 TexCoord = texCoords;
    public int TextureIndex = textureIndex;
   
    
    public Vertex(Vector3 position,
        Vector3 color = default,
        Vector2 texCoords = default,
        int textureIndex = 0,
        Vector3 normal = default) : this(position, new Vector4(color,1),texCoords, textureIndex,normal)
    {
        
    }

    public static void SetupVertexAttribPtr(VertexArrayObject<Vertex> vao,IGraphicsDevice graphicsDevice)
    {
        graphicsDevice.SetupVertexAttributePointer(vao,0,3,VertexAttribPointerType.Float,0);//POS
        graphicsDevice.SetupVertexAttributePointer(vao,1,4,VertexAttribPointerType.Float,3 * sizeof(float));//Color
        graphicsDevice.SetupVertexAttributePointer(vao,2,3,VertexAttribPointerType.Float,(3+4) * sizeof(float));//NORMAL
        graphicsDevice.SetupVertexAttributePointer(vao,3,2,VertexAttribPointerType.Float,(3+4+3) * sizeof(float));//TEX-COORD
        graphicsDevice.SetupVertexAttributeIPointer(vao,4,1,VertexAttribPointerType.Int,(3+4+3+2) * sizeof(float));//TEXTURE ID
    }
}