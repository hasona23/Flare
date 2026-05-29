using Flare.Rendering;

namespace Flare;

//TODO: Make Asset Manager
public class AssetManager:IDisposable
{
    public Texture LoadTexture(string name)
    {
        return new Texture();
    }

    public void UnloadAssets()
    {
        
    }
    public void Dispose()
    {
    }
}