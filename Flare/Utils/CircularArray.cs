using System.Buffers;

namespace Flare;

public class CircularArray<T>(int capacity) : IDisposable
{
    private int _index = 0;
    public int Index
    {
        get => _index;
        set => _index = value % Data.Length;
    }
    public T[] Data = ArrayPool<T>.Shared.Rent(capacity);
    public int Capacity => Data.Length;

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Data);
    }

    public void Add(T item)
    {
        Data[Index] = item;
        Index++;
    }

    public void Resize(int newCapacity)
    {
        ArrayPool<T>.Shared.Return(Data);
        Data = ArrayPool<T>.Shared.Rent(newCapacity);
        Index = 0;
    }

    public T GetCurrent()
    {
        return Data[Index];
    }

    public T GetNext()
    {
        Index++;
        return Data[Index];
    }

    public T this[int i] => Data[i];
}