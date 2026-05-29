using System.Numerics;
using ImGuiNET;
using System.Runtime.InteropServices;
using Flare.Rendering;

namespace Flare.EntityCore;

/// <summary>
/// Can be used to make an Encapsulating Entity for a specific type of Entity. This is useful for managing a group of entities as a single entity.
/// </summary>
/// <typeparam name="TEntity">Type of Entity</typeparam>
/// <param name="name">Name of Manager</param>
public class EntityManager<TEntity>(string name, World world) : Entity(name, Vector2.Zero, world) where TEntity : Entity
{
    #region Fields
    private readonly List<TEntity> _entities = new List<TEntity>();

    private readonly List<TEntity> _entitiesToAdd = new List<TEntity>();
    public ReadOnlySpan<TEntity> Entities => CollectionsMarshal.AsSpan(_entities);
    #endregion

    #region BaseMethods
    public override void Init()
    {
        _entities.AddRange(_entitiesToAdd);
        _entitiesToAdd.Clear();

        foreach (var entity in _entities)
        {
            entity.Init();
        }
    }
    public override void Destroy()
    {
        foreach (var entity in _entities)
        {
            entity.Destroy();
        }
        _entities.Clear();
        base.Destroy();
    }
    
    public override void Draw(FlareRenderer renderer)
    {
        foreach (var entity in _entities)
        {
            if(entity.IsActive)
                entity.Draw(renderer);
        }
    }

    public override void DrawImGui()
    {
        ImGui.Indent();
        foreach (var entity in _entities)
        {
            if(!entity.IsActive)
                ImGui.PushStyleColor(ImGuiCol.Header,entity.IsDead?new Vector4(1f, 0f, 0f, 1f):new Vector4(1f,1f,0f,1f));
            if(ImGui.CollapsingHeader(entity.Name))
                entity.DrawImGui();
            if(!entity.IsActive)
                ImGui.PopStyleColor();
        }
        ImGui.Unindent();
    }

    public override void Update()
    {
        FlushEntityQueues();
        foreach (var entity in _entities)
        {
            if(entity.IsActive)
                entity.Update();
        }
    }
    #endregion

    #region EntityManagerMethods
    public void FlushEntityQueues()
    {
        _entities.AddRange(_entitiesToAdd);
        foreach(var entity in _entitiesToAdd)
        {
            entity.Init();
        }
        _entitiesToAdd.Clear();

        _entities.RemoveAll(entity => entity.IsDead);
    }
    public void AddEntity(TEntity entity)
    {
        _entitiesToAdd.Add(entity);
    }
    
    public TEntity? GetEntityAt(int index)
    {
        if (index < 0 || index >= _entities.Count)
            return null;
        return _entities[index];
    }
    public TEntity? GetEntityName(string name)
    {
        foreach (var entity in _entities)
        {
            if (entity.Name == name)
                return entity;
        }
        return null;
    }
    public bool TryGetEntityName(string name, out TEntity? entity)
    {
        entity = null;
        foreach (var e in _entities)
        {
            if (e.Name == name)
            {
                entity = e;
                return true;
            }
        }
        return false;
    }

    public void RemoveEntity(TEntity entityToRemove)
    {
        foreach (var entity in _entities)
        {
            if (entity == entityToRemove)
                entity.Destroy();
        }
    }
    public bool RemoveEntityName(string name)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].Name == name)
            {
                _entities[i].Destroy();
                return true;
            }
        }
        return false;
    }
    public bool RemoveEntityAt(int index)
    {
        if (index < 0 || index >= _entities.Count)
            return false;
        _entities[index].Destroy();
        return true;
    }
    #endregion
}
