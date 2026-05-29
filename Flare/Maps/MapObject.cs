using System.Drawing;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Flare.Maps;

public struct MapObject
{
    public int? Gid;
    public string? Name;
    public int X;
    public int Y;
    public int? Width;
    public int? Height;

    public MapObject(int? gid,string name, int x, int y, int width, int height)
    {
        Gid = gid;
        X = x;
        Y = y;
        Name = name;
        Width = width;
        Height = height;
    }

    public MapObject(int x, int y)
    {
        X = x;
        Y = y;
    }

    public MapObject(Rectangle rect)
    {
        X = rect.X;
        Y = rect.Y;
    }
    [JsonIgnore]
    public Vector2 Position => new Vector2(X, Y);
    public override string ToString()
    {
        return $"Name: {Name} - ({X},{Y}) - {Width}x{Height} - GID: {Gid}";
    }
}