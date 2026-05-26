using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Flare.Rendering.Exceptions;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Flare.Rendering;

// ReSharper disable once InconsistentNaming
public class OpenGLGraphicsDevice : IGraphicsDevice
{
    public Rectangle Viewport
    {
        get;
        set
        {
            field = value;
            _gl.Viewport(value.X, value.Y, (uint)value.Width, (uint)value.Height);
        }
    }

    private Color _clearColor;
    private readonly GL _gl;
    
    public OpenGLGraphicsDevice(GL gl)
    {
        _gl = gl;
        //StbImage uses top left as origin
        //OpenGL uses bottom left as origin thus need to reverse stbImage input to match our coordinate system
        StbImage.stbi_set_flip_vertically_on_load(1);
        int[] viewport = new int[4];
        unsafe
        {
            fixed (int* pViewport = viewport)
                _gl.GetInteger(GLEnum.Viewport, pViewport);
        }

        Viewport = new Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);

        // Enable transparency blending to remove texture background
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Logger.LogInfo("Created OpenGL GraphicsDevice");
    }


    public void Clear(Color color)
    {
        if (_clearColor != color)
        {
            _clearColor = color;
            _gl.ClearColor(_clearColor);
        }

        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public string CheckErrors()
    {
        GLEnum error = _gl.GetError();
        if (error == GLEnum.NoError)
            return string.Empty;
        return error.ToString();
    }

    public void DrawVertices(int indicesCount, bool drawWireframe = false)
    {
        unsafe
        {
            _gl.PolygonMode(TriangleFace.FrontAndBack, drawWireframe ? PolygonMode.Line : PolygonMode.Fill);
            _gl.DrawElements(PrimitiveType.Triangles, (uint)indicesCount, DrawElementsType.UnsignedInt, (void*)(0));
        }
    }
    
    #region VAO
    
    public VertexArrayObject<TVertexType> CreateVao<TVertexType>(ref BufferObject<TVertexType> vbo, ref BufferObject<uint> ebo)
        where TVertexType : unmanaged
    {
        uint id = _gl.GenVertexArray();
        VertexArrayObject<TVertexType> vao = new(id);
        _gl.BindVertexArray(vao.Id);
        _gl.BindBuffer(vbo.BufferType, vbo.Id);
        _gl.BindBuffer(ebo.BufferType, ebo.Id);

        return vao;
    }

    public void BindVao<TVertexType>(ref VertexArrayObject<TVertexType> vao) where TVertexType : unmanaged
    {
        _gl.BindVertexArray(vao.Id);
    }

    public void SetupVertexAttributePointer<TVertex>(
        ref VertexArrayObject<TVertex> vao,
        uint index,
        int count,
        VertexAttribPointerType type,
        int offSetBytes) where TVertex : unmanaged
    {
        _gl.BindVertexArray(vao.Id);

        unsafe
        {
            
            _gl.VertexAttribPointer(
                index,
                count,
                type,
                false,
                (uint)sizeof(TVertex), // Stride: Size of single Vertex struct
                (void*)offSetBytes // Offset: Bytes from start of struct
            );

          
            _gl.EnableVertexAttribArray(index);
        }
    }

    public void SetupVertexAttributeIPointer<TVertex>(
        ref VertexArrayObject<TVertex> vao,
        uint index,
        int count,
        VertexAttribPointerType type,
        int offSetBytes) where TVertex : unmanaged
    {
      
        _gl.BindVertexArray(vao.Id);

        unsafe
        {
            _gl.VertexAttribIPointer(
                index,
                count,
                VertexAttribIType.Int,
                (uint)sizeof(TVertex), // Stride
                (void*)offSetBytes // Offset: Bytes from start of struct
            );
            
            _gl.EnableVertexAttribArray(index);
        }
    }

    public void DestroyVao<TVertexType>(ref VertexArrayObject<TVertexType> vertexArrayObject)
        where TVertexType : unmanaged
    {
        _gl.DeleteVertexArray(vertexArrayObject.Id);
    }

    #endregion
    #region BufferObject
    public BufferObject<TDataType> CreateBufferObject<TDataType>(Span<TDataType> data, BufferTargetARB bufferType,
        BufferUsageARB bufferUsage) where TDataType : unmanaged
    {
        //Getting the handle, and then uploading the data to said handle.
        uint id = _gl.GenBuffer();
        BufferObject<TDataType> bufferObject = new BufferObject<TDataType>(id, bufferType, bufferUsage);
        _gl.BindBuffer(bufferObject.BufferType, bufferObject.Id);
        unsafe
        {
            fixed (void* d = data)
            {
                _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, bufferUsage);
            }
        }

        _gl.BindBuffer(bufferObject.BufferType, 0);
        return bufferObject;
    }
    public void UploadBufferData<T>(ref BufferObject<T> buffer, Span<T> data) where T : unmanaged
    {
        _gl.BindBuffer(buffer.BufferType, buffer.Id);

        //_gl.BufferData(buffer.BufferType, data, buffer.Usage);
        unsafe
        {
            _gl.BufferSubData(buffer.BufferType, 0, (uint)data.Length * (uint)sizeof(T), data);
        }
    }
    public void DestroyBufferObject<TDataType>(ref BufferObject<TDataType> bufferObject) where TDataType : unmanaged
    {
        _gl.DeleteBuffer(bufferObject.Id);
    }
    
    #endregion
    #region Textures
    public Texture CreateTexture(int width, int height, Span<byte> data,ref TextureConfig config)
    {
        uint id = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, id);

        unsafe
        {
            fixed (byte* ptr = data)
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
        if(config.GenerateMipmaps)
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        ApplyTextureConfig(ref config);
        return new Texture(id, width, height);
    }

    public Texture LoadTexture(string path,ref TextureConfig config)
    {
        uint id = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, id);

        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
        unsafe
        {
            fixed (byte* ptr = result.Data)
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                    (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
        if(config.GenerateMipmaps)
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        ApplyTextureConfig(ref config);
       
        return new Texture(id, result.Width, result.Height);
       
    }
    public void BindTextureToSlot(ref Texture texture, TextureUnit slot)
    {
        _gl.ActiveTexture(slot);
        _gl.BindTexture(TextureTarget.Texture2D, texture.Id);
    }
    public void DestroyTexture(ref Texture texture)
    {
        _gl.DeleteTexture(texture.Id);
    }
    private void ApplyTextureConfig(ref TextureConfig config)
    {
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)config.MinFilter);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)config.MagFilter);
        
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)config.WrapS);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)config.WrapT);
    }
    #endregion
    
    #region Shaders
    
    public Shader CreateShader(string vertexSource, string fragmentSource)
    {
        uint vertexShaderId = _gl.CreateShader(GLEnum.VertexShader);
        _gl.ShaderSource(vertexShaderId, vertexSource);
        _gl.CompileShader(vertexShaderId);
        _gl.GetShader(vertexShaderId, GLEnum.CompileStatus, out int result);
        if (result != (int)GLEnum.True)
        {
            string err = _gl.GetShaderInfoLog(vertexShaderId);
            Logger.LogError($"Failed to compile vertex shader: {err}");
            _gl.DeleteShader(vertexShaderId);
            throw new ShaderException(err);
        }

        uint fragmentShaderId = _gl.CreateShader(GLEnum.FragmentShader);
        _gl.ShaderSource(fragmentShaderId, fragmentSource);
        _gl.CompileShader(fragmentShaderId);
        _gl.GetShader(fragmentShaderId, GLEnum.CompileStatus, out result);
        if (result != (int)GLEnum.True)
        {
            string err = _gl.GetShaderInfoLog(fragmentShaderId);
            Logger.LogError($"Failed to compile fragment shader: {err}");
            _gl.DeleteShader(fragmentShaderId);
            _gl.DeleteShader(fragmentShaderId);
            throw new ShaderException(err);
        }

        uint programId = _gl.CreateProgram();
        _gl.AttachShader(programId, vertexShaderId);
        _gl.AttachShader(programId, fragmentShaderId);

        _gl.LinkProgram(programId);
        _gl.GetProgram(programId, GLEnum.LinkStatus, out result);
        if (result != 1)
        {
            string error = _gl.GetProgramInfoLog(programId);
            Logger.LogError("Failed to link Program Shader: " + error);
            _gl.DeleteShader(vertexShaderId);
            _gl.DeleteShader(fragmentShaderId);
            _gl.DeleteProgram(programId);
            throw new ShaderException(error);
        }

        //Program is linked thus can free them now
        _gl.DetachShader(programId, vertexShaderId);
        _gl.DetachShader(programId, fragmentShaderId);
        _gl.DeleteShader(vertexShaderId);
        _gl.DeleteShader(fragmentShaderId);
        return new Shader(programId);
    }
    public void UseShader(ref Shader shader)
    {
        _gl.UseProgram(shader.ProgramId);
    }

    public void DestroyShader(ref Shader shader)
    {
        _gl.DeleteProgram(shader.ProgramId);
    }
    

    #region SetShaderUniform

    private int GetShaderUniform(ref Shader shader, string name)
    {
        int location;
        if (shader.Uniforms.TryGetValue(name, out var uniform))
            location = uniform;
        else
        {
            location = _gl.GetUniformLocation(shader.ProgramId, name);
            if (location == -1)
            {
                Logger.LogError("Could not find uniform " + name);
            }
            else
            {
                shader.Uniforms[name] = location;
            }
        }

        return location;
    }

    // ── Scalars ────────────────────────────────────────────────────────────────
    public void SetShaderUniform(ref Shader shader, string name, TextureUnit textureSlot)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)(textureSlot - TextureUnit.Texture0));
    }

    public void SetShaderUniform(ref Shader shader, string name, uint value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, float value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, int value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, double value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, bool value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, value ? 1 : 0);
    }

    public void SetShaderUniform(ref Shader shader, string name, Vector2 value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform2(shader.ProgramId, location, value.X, value.Y);
    }

    public void SetShaderUniform(ref Shader shader, string name, Vector3 value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform3(shader.ProgramId, location, value.X, value.Y, value.Z);
    }

    public void SetShaderUniform(ref Shader shader, string name, Vector4 value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform4(shader.ProgramId, location, value.X, value.Y, value.Z, value.W);
    }

    public void SetShaderUniform(ref Shader shader, string name, Matrix4x4 value, bool transpose = false)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniformMatrix4(shader.ProgramId, location, 1, transpose, ref value.M11);
    }

    // ── Spans ──────────────────────────────────────────────────────────────────

    public void SetShaderUniform(ref Shader shader, string name, Span<uint> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<int> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<float> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<double> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<Vector2> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform2(shader.ProgramId, location, (uint)value.Length, MemoryMarshal.Cast<Vector2, float>(value));
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<Vector3> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform3(shader.ProgramId, location, (uint)value.Length, MemoryMarshal.Cast<Vector3, float>(value));
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<Vector4> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform4(shader.ProgramId, location, (uint)value.Length, MemoryMarshal.Cast<Vector4, float>(value));
    }

    public void SetShaderUniform(ref Shader shader, string name, Span<Matrix4x4> value, bool transpose = false)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniformMatrix4(shader.ProgramId, location, (uint)value.Length, transpose,
            MemoryMarshal.Cast<Matrix4x4, float>(value));
    }

    #endregion
    #endregion

    public void Dispose()
    {
        _gl.Dispose();
    }
}