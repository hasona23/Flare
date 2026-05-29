using System.Numerics;
using Flare.Rendering;
using ImGuiNET;

namespace Flare.EntityCore;

public abstract class Entity(string name, Vector2 position, World world)
{
    public string Name { get; set; } = name;
    public Transform2D Transform  = new(position);
    public bool IsActive { get; set;} = true;
    public bool IsDead { get; private set; } = false;
    public World World { get; private set; } = world;

    public abstract void Update();
    public abstract void Draw(FlareRenderer renderer);
    public abstract void Init();
    public virtual void Destroy()
    {
        IsDead = true;
        IsActive = false;
    }

    public virtual void DrawImGui()
    {
        ImGui.Text($"Entity: {Name}");
        ImGui.Text($"Transform: {Transform.Position} - {Transform.Rotation} - x{Transform.Scale}");
        ImGui.Text($"IsActive: {IsActive}");

    }

}
