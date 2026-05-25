    using System.Numerics;
    using System.Runtime.InteropServices;
    using Silk.NET.OpenGL;
    using StbImageSharp;

    namespace Flare.Rendering;

    public class OpenGLGraphicsDevice:IGraphicsDevice
    {
        private readonly GL _gl;
        public GL GL => _gl;
        public OpenGLGraphicsDevice(GL gl)
        {
            _gl = gl;
            StbImage.stbi_set_flip_vertically_on_load(1);   
            Logger.LogInfo("Created OpenGL GraphicsDevice");
        }
        
        public bool CompileShader(string vertexSource, string fragmentSource, out Shader shader)
        {
            uint vertexShaderId  = _gl.CreateShader(GLEnum.VertexShader);
            _gl.ShaderSource(vertexShaderId, vertexSource);
            _gl.CompileShader(vertexShaderId);
            _gl.GetShader(vertexShaderId, GLEnum.CompileStatus,out int result);
            if (result != (int)GLEnum.True)
            {
                string err =  _gl.GetShaderInfoLog(vertexShaderId);
                Logger.LogError($"Failed to compile vertex shader: {err}");
                _gl.DeleteShader(vertexShaderId);
                shader = new Shader();
                return false;
            }
            
            uint fragmentShaderId  = _gl.CreateShader(GLEnum.FragmentShader);
            _gl.ShaderSource(fragmentShaderId, fragmentSource);
            _gl.CompileShader(fragmentShaderId);
            _gl.GetShader(fragmentShaderId, GLEnum.CompileStatus,out result);
            if (result != (int)GLEnum.True)
            {
                string err =  _gl.GetShaderInfoLog(fragmentShaderId);
                Logger.LogError($"Failed to compile fragment shader: {err}");
                _gl.DeleteShader(fragmentShaderId);
                _gl.DeleteShader(fragmentShaderId);
                shader = new Shader();
                return false;
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
                shader = new Shader();
                return false;
            }
            //Program is linked thus can free them now
            _gl.DetachShader(programId, vertexShaderId);
            _gl.DetachShader(programId, fragmentShaderId);
            _gl.DeleteShader(vertexShaderId);
            _gl.DeleteShader(fragmentShaderId);
            shader = new Shader(programId);
            return true;
        }
        
        public void DestroyShader(ref Shader shader)
        {
            _gl.DeleteProgram(shader.ProgramId);
        }

        public BufferObject<TDataType> CreateBufferObject<TDataType>(Span<TDataType> data, BufferTargetARB bufferType,BufferUsageARB bufferUsage) where TDataType : unmanaged
        {
            //Getting the handle, and then uploading the data to said handle.
            uint id = _gl.GenBuffer();
            BufferObject<TDataType> bufferObject = new BufferObject<TDataType>(id,bufferType,bufferUsage);
            _gl.BindBuffer(bufferObject.BufferType, bufferObject.Id);
            unsafe
            {
                fixed (void* d = data)
                {
                    _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
                }
            }
            _gl.BindBuffer(bufferObject.BufferType, 0);
            return bufferObject;
        }

        public void DestroyBufferObject<TDataType>(ref BufferObject<TDataType> bufferObject) where TDataType : unmanaged
        {
            _gl.DeleteBuffer(bufferObject.Id);
        }

        public VertexArrayObject<TVertexType> CreateVertexArrayObject<TVertexType>(BufferObject<TVertexType> vbo, BufferObject<uint> ebo) where TVertexType : unmanaged
        {
            uint id = _gl.GenVertexArray();
            VertexArrayObject<TVertexType> vao = new (id);
            _gl.BindVertexArray(vao.Id);
           _gl.BindBuffer(vbo.BufferType, vbo.Id);
           _gl.BindBuffer(ebo.BufferType, ebo.Id);

           return vao;
        }

        public void SetupVertexAttributePointer<TVertex>(
            VertexArrayObject<TVertex> vao, // Pass by value is fine if it holds the ID
            uint index,
            int size,
            VertexAttribPointerType type,
            int offSetBytes) where TVertex : unmanaged
        {
            // 1. CRITICAL: Bind this VAO so OpenGL knows which object gets these rules
            _gl.BindVertexArray(vao.Id); 

            unsafe
            {
                // 2. Configure the layout
                _gl.VertexAttribPointer(
                    index, 
                    size, 
                    type, 
                    false, 
                    (uint)sizeof(TVertex), // Stride: Size of 1 entire Vertex struct
                    (void*)offSetBytes     // Offset: Bytes from start of struct
                );
            
                // 3. Enable it
                _gl.EnableVertexAttribArray(index);
            }
        }
        public void SetupVertexAttributeIPointer<TVertex>(
            VertexArrayObject<TVertex> vao, // Pass by value is fine if it holds the ID
            uint index,
            int size,
            VertexAttribPointerType type,
            int offSetBytes) where TVertex : unmanaged
        {
            // 1. CRITICAL: Bind this VAO so OpenGL knows which object gets these rules
            _gl.BindVertexArray(vao.Id); 

            unsafe
            {
                // 2. Configure the layout
                _gl.VertexAttribIPointer(
                    index, 
                    size, 
                    VertexAttribIType.Int, 
                    (uint)sizeof(TVertex), // Stride: Size of 1 entire Vertex struct
                    (void*)offSetBytes     // Offset: Bytes from start of struct
                );
            
                // 3. Enable it
                _gl.EnableVertexAttribArray(index);
            }
        }
        public void DestroyVertexArrayObject<TVertexType>(ref VertexArrayObject<TVertexType> vertexArrayObject) where TVertexType : unmanaged
        {
            _gl.DeleteVertexArray(vertexArrayObject.Id);
        }
        

        public bool CreateTexture(int width,int height,Span<byte> data,out Texture texture)
        {
            texture = new Texture();
            uint id = _gl.GenTexture();
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, id);
            
            unsafe
            {
                fixed (byte* ptr = data)
                    _gl.TexImage2D(TextureTarget.Texture2D,0,InternalFormat.Rgba,(uint)width,(uint)height,0,PixelFormat.Rgba,PixelType.UnsignedByte,ptr);
            }
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            texture = new Texture(id,width,height);
            return true;
        }

        public bool LoadTexture(string path, out Texture texture)
        {
           
            uint id = _gl.GenTexture();
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, id);
           
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
            unsafe
            {
                fixed (byte* ptr = result.Data)
                    // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                        (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            _gl.BindTexture(TextureTarget.Texture2D, 0);
            texture = new Texture(id,result.Width,result.Height);
            return true;
        }

        public void DestroyTexture(ref Texture texture)
        {
            _gl.DeleteTexture(texture.Id);
        }

        #region SetShaderUniform

        
         private int GetShaderUniform(ref Shader shader,string name)
         {
             int location = 0;
             if (shader.Uniforms.ContainsKey(name))
                 location = shader.Uniforms[name];
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
            _gl.ProgramUniform1(shader.ProgramId, location, (uint)(textureSlot-TextureUnit.Texture0));
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

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<uint> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<int> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<float> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<double> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform1(shader.ProgramId, location, (uint)value.Length, value);
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Vector2> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform2(shader.ProgramId, location, (uint)value.Length, MemoryMarshal.Cast<Vector2, float>(value));
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Vector3> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform3(shader.ProgramId, location, (uint)value.Length, MemoryMarshal.Cast<Vector3, float>(value));
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Vector4> value)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniform4(shader.ProgramId, location, (uint)value.Length, MemoryMarshal.Cast<Vector4, float>(value));
    }

    public void SetShaderUniform(ref Shader shader, string name, ReadOnlySpan<Matrix4x4> value, bool transpose = false)
    {
        int location = GetShaderUniform(ref shader, name);
        if (location == -1)
            return;
        _gl.ProgramUniformMatrix4(shader.ProgramId, location, (uint)value.Length, transpose, MemoryMarshal.Cast<Matrix4x4, float>(value));
    }
        #endregion
    }