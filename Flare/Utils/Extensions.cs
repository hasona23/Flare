using System.Drawing;
using System.Numerics;
using Flare.Rendering;

namespace Flare;

public static class Extensions
{
    /// <summary>
    /// Get Source rectangle based on Cell ID/Order
    /// </summary>
    /// <param name="atlas">Texture to get from</param>
    /// <param name="cellSize">Size of Rectangle</param>
    /// <param name="gid">Order of Cell in Texture Starting from ZERO</param>
    /// <returns></returns>
    public static Rectangle GetSource(this Texture atlas, int cellSize, int gid)
    {
        return new Rectangle
        {
            X = ((gid) % (atlas.Width / cellSize)) * cellSize,
            Y = ((gid) / (atlas.Width / cellSize)) * cellSize,
            Width = cellSize,
            Height = cellSize,
        };
    }

    public static Color ToColor(this Vector4 color) => Color.FromArgb(
        (int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255), (int)(color.W * 255));

    public static Vector4 ToVector4(this Color color) => new Vector4(color.R, color.G, color.B, color.A) / 255f;
    public static Vector2 ToVector2(this Point point) => new Vector2(point.X, point.Y);
    public static Vector2 ToVector2(this Size size) => new Vector2(size.Width, size.Height);
    public static Point ToPoint(this Vector2 vector) => new Point((int)vector.X, (int)vector.Y);

    //TODO: Move to FlareRenderer.cs
    public static void DrawGrid(this FlareRenderer renderer, Vector2 pos, int rows, int cols, int cellSize, Color color)
    {
        for (int i = 0; i <= rows; i++)
        {
            renderer.DrawTexture(FlareCore.Pixel, pos, color, scale: new Vector2(cols * cellSize, 1));
        }

        for (int i = 0; i <= cols; i++)
        {
            renderer.DrawTexture(FlareCore.Pixel, pos, color, scale: new Vector2(1, rows * cellSize));
        }
    }
}