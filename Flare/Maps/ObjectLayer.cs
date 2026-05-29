using System.Text;

namespace Flare.Maps;

public class ObjectLayer
{
    public List<MapObject> Objects { get; set; } = new List<MapObject>(8);
    public LayerData Data { get; set; }
    
    public ObjectLayer(LayerData data, params MapObject[] objects)
    {
        Data = data;
        Objects.AddRange(objects);
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
       
        stringBuilder.AppendLine($"Object Layer: {Data.Name} ({Data.Tag})");
        stringBuilder.AppendLine($"Objects: {Objects.Count}");
        for (int i = 0; i < Objects.Count; i++)
        {
            stringBuilder.AppendLine(Objects[i].ToString());
        }
        return stringBuilder.ToString();
    }
}