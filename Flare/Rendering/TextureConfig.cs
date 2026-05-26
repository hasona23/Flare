using Silk.NET.OpenGL;

namespace Flare.Rendering;

public record struct TextureConfig(
    TextureMinFilter MinFilter = TextureMinFilter.Nearest,
    TextureMagFilter MagFilter = TextureMagFilter.Nearest,
    TextureWrapMode WrapS = TextureWrapMode.ClampToEdge,
    TextureWrapMode WrapT = TextureWrapMode.ClampToEdge,
    bool GenerateMipmaps = false);