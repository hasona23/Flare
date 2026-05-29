namespace Flare.Rendering;

public readonly struct Texture(uint id, int width, int height,string name):IEquatable<Texture>
{
    public readonly uint Id = id;
    public readonly int Width = width;
    public readonly int Height =  height;
    public readonly string Name = string.Empty;

    public Texture() : this(0, 0, 0,"Texture")
    {
        
    }

    public bool Equals(Texture other)
    {
        return Id == other.Id && Width == other.Width && Height == other.Height && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is Texture other && Equals(other);
    }

    public static bool operator ==(Texture t1, Texture t2)
    {
        return t1.Equals(t2);
    }

    public static bool operator !=(Texture t1, Texture t2)
    {
        return !t1.Equals(t2);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Width, Height,Name);
    }
}