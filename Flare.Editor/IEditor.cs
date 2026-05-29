using Flare;
using Flare.Rendering;

namespace Flare.Editor;

public interface IEditor
{
    public string Name { get; }
    
    public void Init(FlareCore core);
    public void Destroy();
    
    public void Update(float dt);
    public void Draw(IGraphicsDevice graphicsDevice,FlareRenderer renderer);
    public void DrawImGui();
}