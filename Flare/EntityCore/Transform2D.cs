using System.Numerics;

namespace Flare.EntityCore;

public struct Transform2D(Vector2 position)
{
    public Vector2 Position = position;
    public float Rotation = 0f;
    public float Scale = 1f;
}
