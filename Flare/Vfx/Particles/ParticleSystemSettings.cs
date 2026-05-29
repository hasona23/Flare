using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flare.Rendering;

namespace Flare.Vfx.Particles;

public class ParticleSystemSettings
{
    public string Name = "Particle System 1";
    public string TextureName = FlareCore.Circle.Name;
    [JsonIgnore]
    public Texture ParticleTexture = FlareCore.Circle;
    public Color InitialColor = Color.White;
    public Color FinalColor = Color.White;
    public ChangesWithTime AlphaChangeWithTime = ChangesWithTime.Decrease;

    public float MaxSpeed = 500;
    public float MinSpeed = 300;
    public ChangesWithTime SpeedChangeWithTime = ChangesWithTime.Decrease;

    public float MaxAngularSpeed = 1000;
    public float MinAngularSpeed = 1000;
    public ChangesWithTime AngularSpeedChangeWithTime = ChangesWithTime.Decrease;

    public float Gravity;

    public float MaxSize = 1;
    public float MinSize = 1;
    public ChangesWithTime SizeChangeWithTime = ChangesWithTime.Decrease;

    public int MaxParticles = 128;
    public int ParticlesPerSpawn = 16;
    public float SpawnCooldown = 1;
    public SpawnDirections SpawnDirection = SpawnDirections.Outward;
    public Rectangle Bounds = new Rectangle(0, 0, 10, 10);

    public float MaxLife = 1;
    public float MinLife = 0.5f;
    
    
    public static ParticleSystemSettings ParseFromJson(string json, AssetManager contentManager)
    {
        ParticleSystemSettings? settings = JsonSerializer.Deserialize<ParticleSystemSettings>(json,
            new JsonSerializerOptions()
            {
                IncludeFields = true
            });
        if (settings == null)
            throw new Exception("Cannot parse particle system settings");
        
        if(settings.TextureName == FlareCore.Circle.Name)
            settings.ParticleTexture = FlareCore.Circle;
        else if(settings.TextureName == FlareCore.Pixel.Name)
            settings.ParticleTexture = FlareCore.Pixel;
        else
            settings.ParticleTexture = contentManager.LoadTexture(settings.TextureName);
        
        return settings;
    }

    public static ParticleSystemSettings ParseFromJsonFile(string path, AssetManager contentManager)
    {
        return ParseFromJson(File.ReadAllText(path), contentManager);
    }
    [JsonConstructor]
    public ParticleSystemSettings()
    { }
   
    public ParticleSystemSettings(string name)
    {
        Name = name;
        ParticleTexture = FlareCore.Circle;
        TextureName = FlareCore.Circle.Name;
    }
}

public enum SpawnDirections
{
    Outward,
    Inward,
    RotateAround,
    Right,
    Left,
    Up,
    Down,
}

public enum ParticleShapes
{
    Circle,
    Pixel,
}

public enum ChangesWithTime
{
    None,
    Increase,
    Decrease,
}