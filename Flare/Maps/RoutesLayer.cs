using System.Text;

namespace Flare.Maps;

public class RoutesLayer(LayerData data,params Route[] routes)
{
    public LayerData Data { get; set; } = data;
    public Route[] Routes = routes;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Routes Layer: {Data.Name} ({Data.Tag})");
        sb.AppendLine($"Routes: {Routes.Length}");
        foreach (var route in Routes)
        {
            sb.AppendLine(route.ToString());
        }
        return sb.ToString();
    }
}