using Silk.NET.OpenGL;

namespace Flare.Rendering;

public readonly struct BufferObject<TDataType>(uint id, BufferTargetARB bufferType,BufferUsageARB bufferUsage)
    where TDataType : unmanaged
{
    public readonly uint Id = id;
    public readonly BufferTargetARB BufferType = bufferType;
    public readonly BufferUsageARB Usage = bufferUsage;
}