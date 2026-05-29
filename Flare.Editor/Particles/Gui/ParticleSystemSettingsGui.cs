using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Flare;
using Flare.Rendering;
using Flare.Vfx.Particles;
using ImGuiNET;

namespace Flare.Editor.Particles.Gui;

public struct ParticleSystemSettingsGui
{
    private readonly ParticleSystemSettings _settings;

    public ParticleSystemSettingsGui(ParticleSystemSettings settings)
    {
        _settings = settings;
        
        string textureName = _settings.TextureName;

        _availableTextures = Enum.GetNames<ParticleShapes>().ToList();
        if (!_availableTextures.Contains(textureName) && !string.IsNullOrEmpty(_settings.TextureName))
            _availableTextures.Add(textureName);
        if (string.IsNullOrEmpty(textureName))
            _settings.TextureName = _availableTextures[0];
        _availableTextureIndex = _availableTextures.IndexOf(textureName);
        
        _spawnDirectionIndex = (int)_settings.SpawnDirection;
        
        _speedChangesWithTimeIndex = (int)_settings.SpeedChangeWithTime;
        _angularSpeedChangesWithTimeIndex = (int)_settings.AngularSpeedChangeWithTime;
        _sizeChangesWithTimeIndex = (int)_settings.SizeChangeWithTime;
        _alphaChangesWithTimeIndex = (int)_settings.AlphaChangeWithTime;

        _initialColorBuffer = _settings.InitialColor.ToVector4();
        _finalColorBuffer = _settings.FinalColor.ToVector4();
        _newTextureBuffer = "";
    }
    #region Buffers

    private readonly List<string> _availableTextures;
    private int _availableTextureIndex;
    private string _newTextureBuffer;

    private static readonly string[] SpawnDirections = Enum.GetNames<SpawnDirections>();
    private int _spawnDirectionIndex;

    private static readonly string[] ChangesWithTime = Enum.GetNames<ChangesWithTime>();
    private int _speedChangesWithTimeIndex;
    private int _angularSpeedChangesWithTimeIndex;
    private int _sizeChangesWithTimeIndex;
    private int _alphaChangesWithTimeIndex;

    private Vector4 _initialColorBuffer;
    private Vector4 _finalColorBuffer;

    private int _width;
    private int _height;
    #endregion
    
    public void DrawImGui(IGraphicsDevice graphicsDevice,AssetManager assetManager)
    {
        ImGui.PushItemWidth(200f);

        // ── Identity ──────────────────────────────────────────────────────────
        IdentityMenu();
        // ── Color ─────────────────────────────────────────────────────────────
        ColorMenu();
        // ── Motion ────────────────────────────────────────────────────────────
        MotionMenu();
        // ── Emission ──────────────────────────────────────────────────────────
        EmissionMenu();
        // ── Lifetime ──────────────────────────────────────────────────────────
        LifeTimeMenu();
        // ── Texture ───────────────────────────────────────────────────────────
        TextureMenu(assetManager);

        ImGui.PopItemWidth();
    }

    private void IdentityMenu()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Name", ref _settings.Name, 128);
        }
    }
    private void ColorMenu()
    {
        if (ImGui.CollapsingHeader("Color"))
        {
            if (ImGui.ColorButton("##initial_color", _initialColorBuffer, ImGuiColorEditFlags.None, new Vector2(120, 24)))
                ImGui.OpenPopup("initial_color_picker");
            ImGui.SameLine();
            ImGui.Text("Initial Color");

            if (ImGui.BeginPopup("initial_color_picker"))
            {
                ImGui.ColorPicker4("##picker", ref _initialColorBuffer);

                _settings.InitialColor = _initialColorBuffer.ToColor();

                ImGui.EndPopup();
            }

            ImGui.Text(_settings.InitialColor.ToString());
            ImGui.Text(_initialColorBuffer.ToString());
            if (ImGui.ColorButton("##final_color", _finalColorBuffer, ImGuiColorEditFlags.None, new Vector2(120, 24)))
                ImGui.OpenPopup("final_color_picker");
            ImGui.SameLine();
            ImGui.Text("Final Color");

            if (ImGui.BeginPopup("final_color_picker"))
            {
                ImGui.ColorPicker4("##picker", ref _finalColorBuffer);

                _settings.FinalColor = _finalColorBuffer.ToColor();

                ImGui.EndPopup();
            }

            ImGui.Text(_settings.FinalColor.ToString());
            ImGui.Text(_finalColorBuffer.ToString());

            if (ImGui.Combo("Alpha Over Time", ref _alphaChangesWithTimeIndex, ChangesWithTime,
                    ChangesWithTime.Length))
                _settings.AlphaChangeWithTime = (ChangesWithTime)_alphaChangesWithTimeIndex;
        }
    }
    private void MotionMenu()
    {
        if (ImGui.CollapsingHeader("Motion"))
        {
            if (ImGui.TreeNode("Speed"))
            {
                ImGui.DragFloat("Min Speed", ref _settings.MinSpeed, 1f, 0f, _settings.MaxSpeed);
                ImGui.DragFloat("Max Speed", ref _settings.MaxSpeed, 1f, _settings.MinSpeed, 5000f);
                if (ImGui.Combo("Speed Over Time", ref _speedChangesWithTimeIndex, ChangesWithTime,
                        ChangesWithTime.Length))
                    _settings.SpeedChangeWithTime = (ChangesWithTime)_speedChangesWithTimeIndex;
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Angular Speed"))
            {
                ImGui.DragFloat("Min Angular Speed", ref _settings.MinAngularSpeed, 1f, MathF.PI * -10, _settings.MaxAngularSpeed);
                ImGui.DragFloat("Max Angular Speed", ref _settings.MaxAngularSpeed, 1f, _settings.MinAngularSpeed, 5000f);
                if (ImGui.Combo("Angular Speed Over Time", ref _angularSpeedChangesWithTimeIndex, ChangesWithTime,
                        ChangesWithTime.Length))
                    _settings.AngularSpeedChangeWithTime = (ChangesWithTime)_angularSpeedChangesWithTimeIndex;
                ImGui.TreePop();
            }

            ImGui.DragFloat("Gravity", ref _settings.Gravity, 0.5f);
        }
    }
    private void EmissionMenu()
    {
        if (ImGui.CollapsingHeader("Emission"))
        {
            ImGui.DragInt("Max Particles", ref _settings.MaxParticles, 1, 1, 10000);
            ImGui.DragInt("Particles Per Spawn", ref _settings.ParticlesPerSpawn, 1, 1, _settings.MaxParticles);
            ImGui.DragFloat("Spawn Cooldown", ref _settings.SpawnCooldown, 0.01f, 0.001f, 1000f);

            if (ImGui.Combo("Spawn Direction", ref _spawnDirectionIndex, SpawnDirections, SpawnDirections.Length))
                _settings.SpawnDirection = (SpawnDirections)_spawnDirectionIndex;
            if (ImGui.TreeNode("Size"))
            {
                ImGui.DragFloat("Min Size", ref _settings.MinSize, 0.01f, 0f, _settings.MaxSize);
                ImGui.DragFloat("Max Size", ref _settings.MaxSize, 0.01f, _settings.MinSize, 100f);
                if (ImGui.Combo("Size Over Time", ref _sizeChangesWithTimeIndex, ChangesWithTime, ChangesWithTime.Length))
                    _settings.SizeChangeWithTime = (ChangesWithTime)_sizeChangesWithTimeIndex;
                ImGui.TreePop();
                
            }
            if (ImGui.TreeNode("Bounds"))
            {
                if (ImGui.DragInt("Width", ref _width, 1, 0, int.MaxValue))
                {
                    _settings.Bounds.Width = _width;
                }

                if (ImGui.DragInt("Height", ref _height, 1, 0, int.MaxValue))
                {
                    _settings.Bounds.Height = _height;
                }
                ImGui.TreePop();
            }
        }
    }
    private void LifeTimeMenu()
    {
        if (ImGui.CollapsingHeader("Lifetime"))
        {
            ImGui.DragFloat("Min Life", ref _settings.MinLife, 1f, 0f, _settings.MaxLife);
            ImGui.DragFloat("Max Life", ref _settings.MaxLife, 1f, _settings.MinLife, 100000f);
        }
    }
    private void TextureMenu(AssetManager assetManagers)
    {
        if (ImGui.CollapsingHeader("Texture"))
        {
            ImGui.Text("Texture Path in Content Manager: ");
            ImGui.InputText("##texture_input", ref _newTextureBuffer, 255);
            if (ImGui.Button("Add Texture"))
            {
                try
                {
                    _settings.ParticleTexture = assetManagers.LoadTexture(_newTextureBuffer);
                    _availableTextures.Add(_newTextureBuffer);
                    _settings.TextureName = _newTextureBuffer;
                    _availableTextureIndex = _availableTextures.IndexOf(_settings.TextureName);
                    _newTextureBuffer = "";
                }
                catch
                {
                    ImGui.OpenPopup("TextureErrorPopup");
                }
            }

            if (ImGui.BeginPopupModal("TextureErrorPopup"))
            {
                ImGui.TextColored(new(1, 0, 0, 1), "Couldn't load texture:" + _newTextureBuffer);
                if (ImGui.Button("Close"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            if (ImGui.Combo("##Texture", ref _availableTextureIndex, _availableTextures.ToArray(),
                    _availableTextures.Count))
            {
                _settings.TextureName = _availableTextures[_availableTextureIndex];
                _settings.ParticleTexture = _settings.TextureName switch
                {
                    "Circle" => FlareCore.Circle,
                    "Pixel" => FlareCore.Pixel,
                    _ => assetManagers.LoadTexture(_settings.TextureName)
                };
            }
        }
    }
}