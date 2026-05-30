using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flare.Rendering;
using ImGuiNET;

namespace Flare.Editor;

public class Game() : FlareCore(TITLE,WIDTH,HEIGHT)
{
    public const int WIDTH = 1280;
    public const int HEIGHT = 720;
    public const string TITLE = "Editor";
    private string _currentEditor = "";
    private readonly Dictionary<string, IEditor> _editors = new();
    private List<string> _editorNames = new();
    private int _resolutionXBuffer, _resolutionYBuffer;
    private bool _isResizingWindowOpen;
    private FlareRenderer _renderer = null!;
   

    private void GetAllEditors()
    {
        var editors = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(IEditor).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract).ToList();
        foreach (var editor in editors)
        {
            IEditor instance = (IEditor)Activator.CreateInstance(editor);

            if (instance != null)
                _editors[instance.Name] = instance;
        }
    }

    protected override void Initialize()
    {
        GetAllEditors();
        foreach (var (_, editor) in _editors)
        {
            editor.Init(this);
        }

        _editorNames = _editors.Keys.ToList();
        _currentEditor = "ParticleEditor";
        (_resolutionXBuffer, _resolutionYBuffer) = (WIDTH, HEIGHT);
        _renderer = new FlareRenderer(GraphicsDevice);
    }

    protected override void Update(double deltaTime)
    {
        if (_editors.TryGetValue(_currentEditor, out var editor))
            editor.Update((float)deltaTime);
    }

    protected override void Render(double deltaTime)
    {
        //TODO: User Screen Resizable
        //Screen.AttachScreenBuffer();
        if (_editors.TryGetValue(_currentEditor, out var editor))
            editor.Draw(GraphicsDevice, _renderer);
        //Screen.DetachScreenBuffer();
        //Screen.DrawScreen(SpriteBatch);
        
        DrawImGui();
    }

    protected override void Destroy()
    {
        foreach (var (_, editor) in _editors)
        {
            editor.Destroy();
        }
    }
    

    protected void DrawImGui()
    {
        if (!_editors.ContainsKey(_currentEditor))
        {
            ImGui.Begin("Choose Editor");
            foreach (var editorName in _editorNames)
            {
                if (ImGui.Button(editorName))
                    _currentEditor = editorName;

                ImGui.SameLine();
            }

            ImGui.End();
        }

        ImGui.BeginMainMenuBar();
        {
            if (ImGui.BeginMenu("Editors"))
            {
                foreach (var editorName in _editorNames)
                {
                    if (ImGui.MenuItem(editorName))
                    {
                        _editors[_currentEditor]?.Destroy();
                        _currentEditor = editorName;
                        _editors[_currentEditor]?.Init(this);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Settings"))
            {
                if (ImGui.MenuItem("Change Resolution"))
                {
                    _isResizingWindowOpen = true;
                }

                ImGui.EndMenu();
            }
        }
        ImGui.EndMainMenuBar();
        if (_isResizingWindowOpen)
        {
            ImGui.OpenPopup("ResizeWindow");
            _isResizingWindowOpen = false;
        }

        if (ImGui.BeginPopupModal("ResizeWindow"))
        {
            ImGui.Text("Resolution: ");
            ImGui.InputInt("X", ref _resolutionXBuffer, 1);
            ImGui.InputInt("Y", ref _resolutionYBuffer, 1);
            if (ImGui.Button("Apply"))
            {
                //TODO: Screen Change Resolution
                //Screen.ChangeResolution(new Point(_resolutionXBuffer, _resolutionYBuffer));
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Toggle Full Screen"))
            {
                //TODO: Screen FullScreen
                //Screen.IsFullScreen = !Screen.IsFullScreen;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (_editors.TryGetValue(_currentEditor, out var editor))
            editor.DrawImGui();
    }
}