using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text.Json;
using Flare;
using Flare.Editor.Particles.Gui;
using Flare.Rendering;
using Flare.Vfx.Particles;
using ImGuiNET;
using Silk.NET.Input;

namespace Flare.Editor.Particles;

public class ParticleEditor : IEditor
{
    public string Name => "ParticleEditor";
    private FlareCore _core;
    private ParticleSystem _particleSystem;
    private ParticleSystemSettings _particleSystemSettings;
    private string _pathBuffer = "";
    private int _gridCellSize = 16;
    private const string PARTICLE_FILE_EXTENSION = ".flarep";
    private IGraphicsDevice _graphicsDevice;
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    private readonly Color _backgroundColor = Color.Black;

    public void Init(FlareCore core)
    {
        _core = core;
        _particleSystemSettings ??= new ParticleSystemSettings("Settings1");
        _particleSystem =
            new ParticleSystem(_particleSystemSettings, _core.Resolution / 2f, core.AssetManager);
        _systemGui = new ParticleSystemSettingsGui(_particleSystemSettings);
        _graphicsDevice = core.GraphicsDevice;
    }

    public void Destroy()
    {
        _particleSystem.Dispose();
        _core.AssetManager.UnloadAssets();
    }

    public void Update(float dt)
    {
        _particleSystem.Update();
        _particleSystemSettings.Bounds.Location = (_core.Resolution / 2).ToPoint();
        _particleSystemSettings.Bounds.Location -= (Size)(_particleSystemSettings.Bounds.Size.ToVector2()/2).ToPoint();
        if (ImGui.IsAnyItemHovered())
            return;
        if (Input.IsKeyPressed(Key.E))
            _particleSystem.Emit();

        if (Input.IsKeyDown(Key.R))
            _particleSystem.Emit();
        
        if(Input.IsKeyPressed(Key.C))
            Array.Clear(_particleSystem.Particles.Data);
    }

    public void Draw(IGraphicsDevice graphicsDevice, FlareRenderer renderer)
    {
        graphicsDevice.Clear(_backgroundColor);
        renderer.Begin();
        Vector2 resolution = _core.Resolution;
        //TODO: Change to grid method
        for (int i = 0; i <= (int)(resolution.X/_gridCellSize); i++)
        {
            renderer.DrawTexture(FlareCore.Pixel,new Vector2(i*_gridCellSize, 0),Color.DarkGray,scale:new Vector2(2, (int)resolution.Y));
        }

        for (int i = 0; i <= (int)(resolution.Y / _gridCellSize); i++)
        {
            renderer.DrawTexture(FlareCore.Pixel, new Vector2(0, i * _gridCellSize), Color.DarkGray,scale: new Vector2((int)resolution.X, 2));
        }
        renderer.End();
        renderer.Begin();
        {
            _particleSystem.Draw(renderer);
        }
        renderer.End();
    }

    public void Save(string path)
    {
        if (Path.GetExtension(path) != PARTICLE_FILE_EXTENSION)
            path = $"{path}.{PARTICLE_FILE_EXTENSION}";
        string json = JsonSerializer.Serialize(_particleSystemSettings, _options);
        File.WriteAllText(path, json);
    }

    private ParticleSystemSettingsGui _systemGui; 
    public void Load(string path)
    {
        if (Path.GetExtension(path) != PARTICLE_FILE_EXTENSION)
            path = $"{path}.{PARTICLE_FILE_EXTENSION}";
        string json = File.ReadAllText(path);
        if(_particleSystemSettings.TextureName != FlareCore.Circle.Name && _particleSystemSettings.TextureName != FlareCore.Pixel.Name)
           _graphicsDevice.DestroyTexture(ref _particleSystemSettings.ParticleTexture);
        _particleSystemSettings = ParticleSystemSettings.ParseFromJson(json, _core.AssetManager);
        _particleSystem.Settings = _particleSystemSettings;
        _systemGui = new ParticleSystemSettingsGui(_particleSystemSettings);
    }

    public void DrawImGui()
    {
        ImGui.SetNextWindowPos(Vector2.Zero + new Vector2(0,20));
        float width = _core.GraphicsDevice.Viewport.Width / 5f;
        float height = _core.GraphicsDevice.Viewport.Height;
        ImGui.SetNextWindowSize(new Vector2(width, height));
        ImGui.SetNextWindowBgAlpha(0.5f);
       
        ImGui.Begin(Name);
        {
            
            ImGui.Text($"FPS: {Time.Fps}");
            ImGui.Text($"Particles: {_particleSystem.Particles.Data.Length}");
            ImGui.Text($"Index: {_particleSystem.Particles.Index}");
            if (ImGui.Button("Save"))
            {
                ImGui.OpenPopup("SavePopup");
            }

            ImGui.SameLine();
            if (ImGui.Button("Load"))
            {
                ImGui.OpenPopup("LoadPopup");
            }

            if (ImGui.BeginPopupModal("SavePopup"))
            {
                ImGui.Text("Path: ");
                ImGui.InputText("##path", ref _pathBuffer, 256);
                ImGui.SameLine();
                ImGui.Text(PARTICLE_FILE_EXTENSION);
                if (ImGui.Button("Save"))
                {
                    Save(_pathBuffer);
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Button("Cancel"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopupModal("LoadPopup"))
            {
                ImGui.Text("Path: ");
                ImGui.InputText("##path", ref _pathBuffer, 256);
                ImGui.SameLine();
                ImGui.Text(PARTICLE_FILE_EXTENSION);
                if (ImGui.Button("Load"))
                {
                    Load(_pathBuffer);
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Button("Cancel"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            if (ImGui.CollapsingHeader("Screen"))
            {
                ImGui.Text("Grid Cell Size: ");
                ImGui.InputInt("##gridSize", ref _gridCellSize, 1, 1);
            }

            if (ImGui.CollapsingHeader("Settings"))
            {
                _systemGui.DrawImGui(_core.GraphicsDevice,_core.AssetManager);
            }
        }
        ImGui.End();
    }
}