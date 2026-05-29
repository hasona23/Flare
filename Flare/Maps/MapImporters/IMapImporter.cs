namespace Flare.Maps.MapImporters;

public interface IMapImporter
{
    public Map Import(string filePath,string tilesetDir);
}