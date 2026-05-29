using System.Drawing;
using Flare.Rendering;

namespace Flare.Maps.MapRenderers;

public interface IMapRenderer
{
    void DrawBackground(FlareRenderer renderer);
    void DrawGround(FlareRenderer renderer);
    void DrawForeground(FlareRenderer renderer);
    void DrawObjects(FlareRenderer renderer);
    void DrawRoutes(FlareRenderer renderer,Color pointColor,Color lineColor);
    public void DrawAll(FlareRenderer renderer,Color pointColor, Color lineColor)
    {
        DrawBackground(renderer);
        DrawGround(renderer);
        DrawForeground(renderer);
        DrawObjects(renderer);
        DrawRoutes(renderer,pointColor,lineColor);
    }
}
