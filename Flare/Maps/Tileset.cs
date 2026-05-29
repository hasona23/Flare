using System.Text.Json;
using Flare.Rendering;

namespace Flare.Maps;

public struct Tileset
{
    public string Name { get; set; }
    public string AtlasName { get; set; }
    public Texture? Atlas { get; set; } = null;

    public Tileset(string json)
    {
        JsonDocument jsonDocument = JsonDocument.Parse(json);
        Name = jsonDocument.RootElement.GetProperty("name").GetString() ?? throw new JsonException("name is null");
        AtlasName = Path.GetFileNameWithoutExtension(jsonDocument.RootElement.GetProperty("image").GetString()??throw new JsonException("atlasName is null"));
    }
}