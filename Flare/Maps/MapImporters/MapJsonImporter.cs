using System.Drawing;
using System.Numerics;
using System.Text.Json;

namespace Flare.Maps.MapImporters;

public class MapJsonImporter:IMapImporter
{
    public Map Import(string filePath,string tilesetDir)
    {
        string json = File.ReadAllText(filePath);
        JsonDocument document = JsonDocument.Parse(json);
        

        Map map = new Map();
        
        string tilesetPath = Path.Combine(tilesetDir,
            document.RootElement.GetProperty("tilesets")[0].GetProperty("source").ToString() ??
            throw new JsonException("Tileset path not found"));
        map.Tileset =  LoadTileset(tilesetPath);
        map.Name = Path.GetFileNameWithoutExtension(filePath);
        map.Width = document.RootElement.GetProperty("width").GetInt32();
        map.Height = document.RootElement.GetProperty("height").GetInt32();
        map.TileSize = document.RootElement.GetProperty("tilewidth").GetInt32();
        
       

        foreach (var layer in document.RootElement.GetProperty("layers").EnumerateArray())
        {
            string layerType =  layer.GetProperty("type").GetString() ?? "NO TYPE FOUND";
            
            HandleLayer(layer, layerType,map);

        }
        
        return map;
    }
    private LayerData ParseLayerData(JsonElement jsonLayer,Map map)
    {
        string name = jsonLayer.TryGetProperty("name", out var layerName) ? layerName.ToString() : $"UN-NAMED LAYER";
        bool isVisible = !jsonLayer.TryGetProperty("visible", out var layerVisible) || !layerVisible.GetBoolean();
        string tag = jsonLayer.TryGetProperty("class", out var tagProp) ? tagProp.GetString()??string.Empty : string.Empty;
        Color color = jsonLayer.TryGetProperty("tintcolor", out var tintColor) ? ColorTranslator.FromHtml(tintColor.GetString() ?? "#FFFFFF") : Color.White;
        LayerData layerData = new LayerData(name, tag, map)
        {
            Alpha = jsonLayer.TryGetProperty("opacity", out var opacity) ? opacity.GetSingle() : 1f,
            Color =Color.FromArgb(color.A,color.R, color.G, color.B),
        };
        return layerData;
    }
    private void HandleLayer(JsonElement jsonLayer,string layerType,Map map)
    {
        jsonLayer.TryGetProperty("name", out var layerName);
        
        switch (layerType)
        {
            case TiledLayerTypes.TileLayer:
                ParseTileLayer(jsonLayer, ParseLayerData(jsonLayer, map),map);
                break;
            case TiledLayerTypes.ObjectLayer:
                ParseObjectLayer(jsonLayer, ParseLayerData(jsonLayer, map), map);
                break;
            case TiledLayerTypes.GroupLayer:
                {
                    if (jsonLayer.TryGetProperty("layers", out var layers))
                    {
                        
                        foreach (var layer in layers.EnumerateArray())
                            HandleLayer(layer, layer.GetProperty("type").ToString(),map);
                    }

                    break;
                }

            default:
                throw new Exception($"Unknown layer type: {layerType}");
        }
    }
    private Tileset LoadTileset(string filePath)
    {
        return new Tileset(File.ReadAllText(filePath));
    }

    private void ParseTileLayer(JsonElement jsonTileLayer, LayerData layerData,Map map)
    {
        
        int[] tiles = [.. jsonTileLayer.GetProperty("data")
            .EnumerateArray()
            .Select(t => t.GetInt32() - 1)];
       
        map.TileLayers.Add(new TileLayer(layerData, tiles));
    }

    private void ParseObjectLayer(JsonElement jsonObjectLayer, LayerData layerData,Map map)
    {
        int capacity = jsonObjectLayer.GetProperty("objects").GetArrayLength();
        List<MapObject> objects = new List<MapObject>(capacity);
        List<Route> routes = new List<Route>(capacity);
        foreach(var jsonObject in jsonObjectLayer.GetProperty("objects").EnumerateArray())
        {
            if (jsonObject.TryGetProperty("polyline", out var polyline))
            {
                Vector2[] points = new Vector2[polyline.GetArrayLength()];
                var pointsJson = polyline.EnumerateArray().ToArray();
                for (int i = 0; i < points.Length; i++)
                {
                    var pointJson = pointsJson[i];
                    points[i] = new Vector2(pointJson.GetProperty("x").GetInt32(), pointJson.GetProperty("y").GetInt32());
                }
                Route route = new Route(jsonObject.TryGetProperty("name", out var name) ? name.GetString() ?? "UN-NAMED ROUTE" : "UN-NAMED ROUTE", points);
                routes.Add(route);
            }
            else
            {
                MapObject mapObject = new MapObject
                {
                    Gid = jsonObject.TryGetProperty("gid", out var gid) ? gid.GetInt32() - 1 : (int?)null,
                    Name = jsonObject.TryGetProperty("name", out var name) ? name.GetString() ?? "UN-NAMED OBJECT" : "UN-NAMED OBJECT",
                    X = jsonObject.GetProperty("x").GetInt32(),
                    Y = jsonObject.GetProperty("y").GetInt32(),
                    Width = jsonObject.TryGetProperty("width", out var width) ? width.GetInt32() : 0,
                    Height = jsonObject.TryGetProperty("height", out var height) ? height.GetInt32() : 0
                };
                objects.Add(mapObject);
            }
        }
        if(objects.Count > 0)
            map.ObjectLayers.Add(new ObjectLayer(layerData, [.. objects]));
        if(routes.Count > 0)
            map.RoutesLayers.Add(new RoutesLayer(layerData, [.. routes]));
    }

}