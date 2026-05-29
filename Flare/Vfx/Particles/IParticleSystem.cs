using Flare.Rendering;

namespace Flare.Vfx.Particles;

public interface IParticleSystem
{
    public ParticleSystemSettings Settings { get; set; }
    public CircularArray<Particle> Particles { get; set; }
    
    public void Emit();
    public void Update();
    public void Draw(FlareRenderer renderer);
}