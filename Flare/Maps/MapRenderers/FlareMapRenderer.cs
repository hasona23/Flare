using System.Drawing;
using System.Numerics;
using Flare.Rendering;

namespace Flare.Maps.MapRenderers;

public class FlareMapRenderer(Map map) : IMapRenderer
{
    public void DrawBackground(FlareRenderer renderer)
    {
        foreach (TileLayer tileLayer in map.TileLayers)
        {
            if (tileLayer.TileLayerTypes == TileLayerTypes.Background)
            {
                DrawTileLayer(renderer, tileLayer);
            }
        }
    }
    private void DrawTileLayer(FlareRenderer renderer, TileLayer tileLayer)
    {
        for (int tileIndex = 0; tileIndex < tileLayer.Tiles.Length; tileIndex++)
        {
            if (tileLayer.Tiles[tileIndex] == -1)
                continue;
            if (!map.Tileset.Atlas.HasValue)
                return;
            Vector2 tilePosition = new Vector2(tileIndex % map.Width, tileIndex / map.Width);
            renderer.DrawTexture(map.Tileset.Atlas.Value,
                tilePosition * map.TileSize * map.Scale,
                map.Tileset.Atlas.Value.GetSource(map.TileSize, tileLayer.Tiles[tileIndex]),
                Color.White);
        }
    }
    public void DrawForeground(FlareRenderer renderer)
    {
        foreach (TileLayer tileLayer in map.TileLayers)
        {
            if (tileLayer.TileLayerTypes == TileLayerTypes.Foreground)
            {
                DrawTileLayer(renderer, tileLayer);
            }
        }
    }

    public void DrawGround(FlareRenderer renderer)
    {
        foreach (TileLayer tileLayer in map.TileLayers)
        {
            if (tileLayer.TileLayerTypes == TileLayerTypes.Ground)
            {
                DrawTileLayer(renderer, tileLayer);
            }
        }
    }

    private static readonly Color ObjectBoundsColor = Color.FromArgb(100,Color.Red);
    public void DrawObjects(FlareRenderer renderer)
    {
        foreach (ObjectLayer objectLayer in map.ObjectLayers)
        {
            foreach (var obj in objectLayer.Objects)
            {
                if (obj.Gid.HasValue && obj.Gid.Value != -1)
                {
                    if (map.Tileset.Atlas.HasValue)
                    {
                        renderer.DrawTexture(map.Tileset.Atlas.Value,
                            new Vector2(obj.X, obj.Y) * map.Scale,
                            map.Tileset.Atlas.Value.GetSource(map.TileSize, obj.Gid.Value),
                            Color.White);
                    }
                }
                else if (obj.Width.HasValue && obj.Height.HasValue && obj.Width != 0 && obj.Height != 0)
                {
                    
                    renderer.DrawRectangle(
                        new Rectangle(obj.X, obj.Y, obj.Width.Value, obj.Height.Value),
                        ObjectBoundsColor);
                }
                else
                {
                    renderer.DrawRectangle(
                        new Rectangle(obj.X, obj.Y, 10, 10),
                        ObjectBoundsColor);
                }
            }
        }
    }

    public void DrawRoutes(FlareRenderer renderer, Color pointColor, Color lineColor)
    {
        foreach (var routeLayer in map.RoutesLayers)
        {
            for (int j = 0; j < routeLayer.Routes.Length; j++)
            {
                var route = routeLayer.Routes[j];
                for (int k = 0; k < route.Coordinates.Length; k++)
                {
                    var point = route.Coordinates[k];

                    renderer.DrawRectangle(new Rectangle((point - new Vector2(2)).ToPoint(), new Size(new Point(4))), pointColor);
                    if (k < route.Coordinates.Length - 1)
                    {
                        var nextPoint = route.Coordinates[k + 1];
                        renderer.DrawLine(point, nextPoint, lineColor,4);
                    }
                }
            }
        }
    }
}