using System.Drawing;

namespace Flare.Maps;

public struct LayerData(string name,string tag,Map map)
{
    public string Name { get; set; } = name;
    public string Tag { get; set; } = tag;
    public Map Map { get; set; } = map;
    /// <summary>
    ///Color transparency from 0.0 to 1.0
    /// </summary>
    public float Alpha { get; set; } = 1;

    public Color Color { get; set; } = Color.White;
    public bool IsVisible { get; set; } = true;

    public override string ToString()
    {
        return $"{Name} - {Tag} - {Map.Name} - {Alpha} - {Color} - {IsVisible}";
    }
}