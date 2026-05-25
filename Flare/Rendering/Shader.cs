namespace Flare.Rendering;

public readonly struct Shader(uint programId):IEquatable<Shader>
{
    public readonly uint ProgramId = programId;
    public readonly Dictionary<string, int> Uniforms = new Dictionary<string, int>();
    public Shader() : this(0)
    {
    }

    public bool Equals(Shader other)
    {
        return ProgramId == other.ProgramId && Uniforms.Equals(other.Uniforms);
    }

    public override bool Equals(object? obj)
    {
        return obj is Shader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProgramId, Uniforms);
    }
    
    public static bool operator ==(Shader s1, Shader s2)
    {
        return s1.Equals(s2);
    }

    public static bool operator !=(Shader s1, Shader s2)
    {
        return !s1.Equals(s2);
    }
}
