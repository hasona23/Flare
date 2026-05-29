using System.Numerics;
using System.Text;

namespace Flare.Maps;

public struct Route(string name,params Vector2[] points)
{
    public string Name = name;
    public Vector2[] Coordinates { get; private set; } = points;
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Route: {Name} - {Coordinates.Length} points");
        foreach (var point in Coordinates)
        {
            sb.Append($"({point.X},{point.Y})");   
        }
        sb.AppendLine();
        return sb.ToString();
    }
}