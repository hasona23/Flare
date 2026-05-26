using System.Diagnostics;

namespace Flare;

/// <summary>
/// Used to measure execution time of function (benchmarking and measuring performance)
/// </summary>
public readonly struct DebugWatch(string name) : IDisposable
{
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    
    public void Dispose()
    {
        _watch.Stop();
        Logger.LogInfo($"{name} -------- {_watch.Elapsed.TotalMicroseconds} micro-s");
    }
}