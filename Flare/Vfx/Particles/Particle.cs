using System.Drawing;
using System.Numerics;


namespace Flare.Vfx.Particles;

public struct Particle
{
    public static Int128 Allocations { get; private set; } = 0;

    public Vector2 Position = Vector2.Zero;
    public Vector2 Velocity = Vector2.Zero;
    public float Size = 1;
    public float Rotation = 0;
    public float AngularSpeed = 0;
    public Color Color = Color.White;
    public float LifeTime = 0;
    public bool IsAlive => LifeTime > 0;

    public Particle()
    {
        Allocations++;
    }

    public Particle(Vector2 pos, Vector2 vel, float size, float rotation, float angularSpeed, Color color, float lifeTime)
    {
        Allocations++;

        Position = pos;
        Velocity = vel;
        Size = size;
        Rotation = rotation;
        AngularSpeed = angularSpeed;
        Color = color;
        LifeTime = lifeTime;
    }

    public void Reset(Vector2 pos, Vector2 vel, float size, float rotation, float angularSpeed, Color color,
        float lifeTime)
    {
        Position = pos;
        Velocity = vel;
        Size = size;
        Rotation = rotation;
        AngularSpeed = angularSpeed;
        Color = color;
        LifeTime = lifeTime;
    }
}