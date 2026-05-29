using System.Numerics;

namespace Flare;

public class Camera2D
{
    public Vector2 Position = Vector2.Zero;
    public float Rotation = 0f;
    public float Zoom = 1f;
    public Vector2 Offset = new Vector2(0f, 0f);
    public Matrix4x4 GetViewMatrix()
    {
        return
            Matrix4x4.CreateTranslation(new Vector3(-Position, 0f)) *
            Matrix4x4.CreateRotationZ(Rotation) *
            Matrix4x4.CreateScale(Zoom, Zoom, 1f) *
            Matrix4x4.CreateTranslation(Offset.X, Offset.Y, 0f);
    }
}