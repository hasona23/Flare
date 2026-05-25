using System.Drawing;
using Silk.NET.OpenGL;
using Rectangle = System.Drawing.Rectangle;

namespace Flare.Rendering;

public class OpenGLRenderer:IRenderer
{
    private readonly GL _gl;
    public GL GL => _gl;
    private Color _clearColor;
   
    public Rectangle Viewport
    {
        get;
        set
        {
            field = value;
            _gl.Viewport(value.X, value.Y, (uint)value.Width, (uint)value.Height);
        }
    }

    public OpenGLRenderer(GL gl)
    {
        _gl = gl;  
        int[] viewport = new int[4];
        unsafe
        {
            fixed(int* pViewport = viewport)
                _gl.GetInteger(GLEnum.Viewport, pViewport);
        }
        Viewport = new Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);

        // Enable transparency blending
        gl.Enable(EnableCap.Blend);

// Configure how the alpha channels calculate transparency
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Logger.LogInfo("OpenGLRenderer created");
    }
    public void Clear(Color color)
    {
        if(_clearColor != color)
        {
            _clearColor = color;
            _gl.ClearColor(_clearColor);
        }
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void SetTextureFilter(TextureMagFilter magFilter, TextureMinFilter minFilter)
    {
        _gl.TexParameter(GLEnum.Texture2D,TextureParameterName.TextureMagFilter,(int)magFilter);
        _gl.TexParameter(GLEnum.Texture2D,TextureParameterName.TextureMinFilter,(int)minFilter);
    }

    public void SetTextureWrap(TextureWrapMode wrapX, TextureWrapMode wrapY)
    {
        _gl.TexParameter(GLEnum.Texture2D,TextureParameterName.TextureWrapS, (int)wrapX);
        _gl.TexParameter(GLEnum.Texture2D,TextureParameterName.TextureWrapT, (int)wrapY);
    }

    public void BindVao<TVertexType >(VertexArrayObject<TVertexType> vao) where TVertexType : unmanaged
    {
        _gl.BindVertexArray(vao.Id);
    }
    
    public void Render(BufferObject<uint> indices)
    {
        throw new NotImplementedException();
    }

    public void BindTextureToSlot(Texture texture, TextureUnit slot)
    {
        _gl.ActiveTexture(slot);
        _gl.BindTexture(TextureTarget.Texture2D, texture.Id);
    }


    public void DrawVertices(int indicesCount,bool drawWireframe=false)
    {
        unsafe
        {
            
            _gl.PolygonMode(TriangleFace.FrontAndBack,drawWireframe?PolygonMode.Line:PolygonMode.Fill);
            _gl.DrawElements(PrimitiveType.Triangles,(uint)indicesCount,DrawElementsType.UnsignedInt,(void*)(0));
        }
    }
    

    public void UploadBufferData<T>(BufferObject<T> buffer, ReadOnlySpan<T> data) where T : unmanaged
    {
        _gl.BindBuffer(buffer.BufferType, buffer.Id);

        _gl.BufferData(buffer.BufferType, data, buffer.Usage);
        unsafe
        {
            _gl.BufferSubData(buffer.BufferType, 0, (uint)data.Length * (uint)sizeof(T), data);
        }
    }

   
    public void SetActiveShader(Shader shader)
    {
        _gl.UseProgram(shader.ProgramId);
    }

   
}