using Flare.Maps.MapRenderers;

namespace Flare.Maps;

public class Map
{
    public string Name { get; set; } = string.Empty;
    public List<TileLayer> TileLayers { get; set; } = new List<TileLayer>(8);
    public List<ObjectLayer> ObjectLayers { get; set; } = new List<ObjectLayer>(8);
    public List<RoutesLayer> RoutesLayers { get; set; } = new List<RoutesLayer>(8);
    public Tileset Tileset;
    public int Width { get; set; }
    public int Height { get; set; }
    public int TileSize { get; set; }
    public int Scale { get; set; } = 1;

    public IMapRenderer? Renderer { get; set; }
}