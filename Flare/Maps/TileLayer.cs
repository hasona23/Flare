using System.Drawing;
using System.Numerics;
using System.Text;

namespace Flare.Maps;

public static class TileLayerTags
{
    public const string BACKGROUND = "background";
    public const string GROUND = "ground";
    public const string FOREGROUND = "foreground";
    public const string COLLISION = "collision";
}

public enum TileLayerTypes
{
    Background,
    Ground,
    Foreground,
}

public class TileLayer
{
    public int[] Tiles { get; set; }
    public LayerData Data { get; set; }
    public bool IsCollision { get; set; }
    public TileLayerTypes TileLayerTypes { get; set; }

    public TileLayer(LayerData data, params int[] tiles)
    {
        Data = data;
        Tiles = tiles;
        if (data.Tag.Contains(TileLayerTags.COLLISION))
            IsCollision = true;
        if (data.Tag.Contains(TileLayerTags.BACKGROUND))
            TileLayerTypes = TileLayerTypes.Background;
        else if (data.Tag.Contains(TileLayerTags.GROUND))
            TileLayerTypes = TileLayerTypes.Ground;
        else if (data.Tag.Contains(TileLayerTags.FOREGROUND))
            TileLayerTypes = TileLayerTypes.Foreground;
    }

    private static readonly Vector2[] Offsets =
    [
        new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 1),
        new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0),
        new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1)
    ];

    public List<Tile> GetTilesNear(Vector2 position)
    {
        List<Tile> nearTilesBuffer = new List<Tile>(9);
        position.X = MathF.Floor(position.X / (Data.Map.TileSize * Data.Map.Scale));
        position.Y = MathF.Floor(position.Y / (Data.Map.TileSize * Data.Map.Scale));
        foreach (var offset in Offsets)
        {
            position += offset;
            int index = (int)(position.X + position.Y * Data.Map.Width);
            if (index >= 0 && index < Tiles.Length && Tiles[index] != -1)
            {
                Vector2 pos = position * Data.Map.TileSize * Data.Map.Scale;
                nearTilesBuffer.Add(new Tile(Tiles[index],
                    new Rectangle((int)pos.X, (int)pos.Y, Data.Map.TileSize * Data.Map.Scale,
                        Data.Map.TileSize * Data.Map.Scale)));
            }

            position -= offset;
        }

        return nearTilesBuffer;
    }

    public void SetTile(Vector2 position, int gid)
    {
        position.X = MathF.Floor(position.X / (Data.Map.TileSize * Data.Map.Scale));
        position.Y = MathF.Floor(position.Y / (Data.Map.TileSize * Data.Map.Scale));
        int index = (int)(position.X + position.Y * Data.Map.Width);
        if (index >= 0 && index < Tiles.Length)
            Tiles[index] = gid;
    }

    public Tile? GetTileAt(Vector2 position)
    {
        position.X = MathF.Floor(position.X / (Data.Map.TileSize * Data.Map.Scale));
        position.Y = MathF.Floor(position.Y / (Data.Map.TileSize * Data.Map.Scale));
        int index = (int)(position.X + position.Y * Data.Map.Width);
        if (index >= 0 && index < Tiles.Length)
            return new Tile(Tiles[index],
                new Rectangle((position * Data.Map.TileSize * Data.Map.Scale).ToPoint(),
                    new Size(new Point((Data.Map.TileSize * Data.Map.Scale)))));
        return null;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Tile Layer: {Data.Name} ({Data.Tag})");
        sb.AppendLine($"Tiles Count: {Tiles.Length}");
        sb.AppendLine($"Is Collision: {IsCollision}");
        for (int i = 0; i < Tiles.Length; i++)
        {
            sb.Append(Tiles[i]);
            if ((i + 1) % Data.Map.Width == 0)
                sb.AppendLine();
        }

        return sb.ToString();
    }
}