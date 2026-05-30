using System.Drawing;
using System.Numerics;
using Flare.Rendering;


namespace Flare.Vfx.Particles;

public class ParticleSystem : IParticleSystem, IDisposable
{
    public ParticleSystemSettings Settings { get; set; }
    public CircularArray<Particle> Particles { get; set; }
    private readonly AssetManager _contentManager;
    private float _timer;

    public ParticleSystem(ParticleSystemSettings settings, Vector2 pos, AssetManager contentManager)
    {
        Settings = settings;
        Particles = new CircularArray<Particle>(Settings.MaxParticles);
        _contentManager = contentManager;
        settings.Bounds.Location = (pos - (settings.Bounds.Size.ToVector2() / 2)).ToPoint();
    }

    public void Emit()
    {
        for (int i = 0; i < Settings.ParticlesPerSpawn; i++)
        {
            AdjustParticle(ref Particles.Data[Particles.Index]);
            Particles.Index++;
        }
    }

    public void Update()
    {
        if (Settings.MaxParticles != Particles.Data.Length)
        {
            Particles.Resize(Settings.MaxParticles);
            Settings.MaxParticles = Particles.Data.Length;
        }

        float dt = Time.DeltaTime;

        if (Settings.SpawnCooldown > 0)
        {
            _timer += dt;
            if (_timer > Settings.SpawnCooldown)
            {
                _timer -= Settings.SpawnCooldown;
                Emit();
            }
        }

        for (int i = 0; i < Particles.Capacity; i++)
        {
            ref Particle particle = ref Particles.Data[i];
            float lifeTimeRatio = particle.LifeTime / Settings.MaxLife;
            particle.LifeTime -= dt;
            if (!particle.IsAlive)
                continue;
            if (Settings.AngularSpeedChangeWithTime != ChangesWithTime.None)
            {
                if (Settings.AngularSpeedChangeWithTime == ChangesWithTime.Increase)
                    particle.Rotation += particle.AngularSpeed * (1 - lifeTimeRatio) * Time.DeltaTime;
                else
                    particle.Rotation += particle.AngularSpeed * lifeTimeRatio * Time.DeltaTime;
            }
            else
            {
                particle.Rotation += particle.AngularSpeed * Time.DeltaTime;
            }

            if (Settings.SpawnDirection == SpawnDirections.RotateAround)
            {
                float radius = (Settings.Bounds.Location.ToVector2() + Settings.Bounds.Size.ToVector2() / 2f -
                                particle.Position).Length();
                radius = MathF.Max(radius, Settings.Bounds.Width / 5f);
                particle.Position.X = MathF.Cos(particle.Rotation) * radius;
                particle.Position.Y = MathF.Sin(particle.Rotation) * radius;
                particle.Position += Settings.Bounds.Location.ToVector2() + Settings.Bounds.Size.ToVector2() / 2f;
            }
            else
            {
                if (Settings.SpeedChangeWithTime != ChangesWithTime.None)
                {
                    if (Settings.SpeedChangeWithTime == ChangesWithTime.Increase)
                        particle.Position += particle.Velocity * (1 - lifeTimeRatio) * Time.DeltaTime;
                    else
                        particle.Position += particle.Velocity * (lifeTimeRatio) * Time.DeltaTime;
                }
                else
                {
                    particle.Position += particle.Velocity * Time.DeltaTime;
                }
            }

            if (Settings.Gravity != 0)
            {
                particle.Velocity.Y += Settings.Gravity * Time.DeltaTime;
                particle.Velocity.Y = MathF.Min(Settings.Gravity * 15f, particle.Velocity.Y);
            }
        }
    }

    public void Draw(FlareRenderer renderer)
    {
        Texture particleTexture = FlareCore.Circle;
        if (Settings.TextureName == Enum.GetName(ParticleShapes.Pixel))
            particleTexture = FlareCore.Pixel;
        else if (Settings.TextureName != Enum.GetName(ParticleShapes.Circle))
            particleTexture = _contentManager.LoadTexture(Settings.TextureName);

        Color color = Color.Pink;
        //Don't change to foreach loop as this will cause copy and particle struct is too large
        for (int i = 0; i < Particles.Capacity; i++)
        {
            if (!Particles.Data[i].IsAlive)
                continue;
            float lifeTimeRation = Particles.Data[i].LifeTime / Settings.MaxLife;


            float t = 1 - lifeTimeRation;
            int r = (byte)(Settings.InitialColor.R + (Settings.FinalColor.R - Settings.InitialColor.R) * t);
            int g = (byte)(Settings.InitialColor.G + (Settings.FinalColor.G - Settings.InitialColor.G) * t);
            int b = (byte)(Settings.InitialColor.B + (Settings.FinalColor.B - Settings.InitialColor.B) * t);
            int a = (byte)((Settings.InitialColor.A + (Settings.FinalColor.A - Settings.InitialColor.A)) * t);

            float size = Particles.Data[i].Size;
            if (Settings.AlphaChangeWithTime != ChangesWithTime.None)
            {
                float alpha = Settings.AlphaChangeWithTime == ChangesWithTime.Increase
                    ? (1 - lifeTimeRation)
                    : lifeTimeRation;
                color = Color.FromArgb((int)(alpha * 255), Color.FromArgb(r, g, b, a));
            }

            if (Settings.SizeChangeWithTime != ChangesWithTime.None)
            {
                size *= Settings.SizeChangeWithTime == ChangesWithTime.Increase ? (1 - lifeTimeRation) : lifeTimeRation;
            }

            renderer.DrawTexture(
                texture: Settings.ParticleTexture,
                position: Particles.Data[i].Position,
                tintColor: color,
                scale: new Vector2(size),
                origin: new Vector2(particleTexture.Width, particleTexture.Height) / 2f,
                rotation: Particles.Data[i].Rotation,
                invertHorizontal: false,
                invertVertical: false,
                sourceRect: new Rectangle(0, 0, Settings.ParticleTexture.Width, Settings.ParticleTexture.Height));
        }
    }

    private Vector2 GetRandomVelocity(Vector2 particlePos)
    {
        Vector2 velocity = Vector2.Zero;
        switch (Settings.SpawnDirection)
        {
            case SpawnDirections.Right:
                velocity.X = 1;
                //up and down randomness
                velocity.Y = (Random.Shared.NextSingle() - .5f) * 2 / 3;
                break;
            case SpawnDirections.Left:
                velocity.X = -1;
                //up and down randomness
                velocity.Y = (Random.Shared.NextSingle() - .5f) * 2 / 3;
                break;
            case SpawnDirections.Up:
                velocity.Y = -1;
                // right and left randomness
                velocity.X = (Random.Shared.NextSingle() - .5f) * 2 / 3;
                break;
            case SpawnDirections.Down:
                velocity.Y = 1;
                // right and left randomness
                velocity.X = (Random.Shared.NextSingle() - .5f) * 2 / 3;
                break;
            //BUG: Inward behaves like outward need to set the ending point at center
            case SpawnDirections.Inward:
                velocity = (Settings.Bounds.Location.ToVector2() + Settings.Bounds.Size.ToVector2() / 2f) - particlePos;
                break;
            case SpawnDirections.Outward:
                velocity = particlePos - (Settings.Bounds.Location.ToVector2() + Settings.Bounds.Size.ToVector2() / 2f);
                break;
        }

        if (velocity.LengthSquared() != 0)
            velocity = Vector2.Normalize(velocity);
        return velocity *
               (float)(Random.Shared.NextDouble() * (Settings.MaxSpeed - Settings.MinSpeed) + Settings.MinSpeed);
    }

    private void AdjustParticle(ref Particle particle)
    {
        Vector2 randomPos = new Vector2((float)(Random.Shared.NextDouble() * Settings.Bounds.Width + Settings.Bounds.X),
            (float)(Random.Shared.NextDouble() * Settings.Bounds.Height + Settings.Bounds.Y));
        particle.Position = randomPos;
        particle.LifeTime = Random.Shared.NextSingle() * (Settings.MaxLife - Settings.MinLife) + Settings.MinLife;
        particle.Velocity = GetRandomVelocity(particle.Position);
        particle.Size = Random.Shared.NextSingle() * (Settings.MaxSize - Settings.MinSize) + Settings.MinSize;
        particle.AngularSpeed = Random.Shared.NextSingle() * (Settings.MaxAngularSpeed - Settings.MinAngularSpeed) +
                                Settings.MinAngularSpeed;
        particle.Rotation = Random.Shared.NextSingle() * 2 * MathF.PI;
        particle.Color = Settings.InitialColor;
    }

    public void Dispose()
    {
        Particles.Dispose();
    }
}