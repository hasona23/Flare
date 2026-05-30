namespace Flare;

public static class Time
{
    public static float DeltaTime { get; private set; }
    public static float UnScaledDeltaTime { get; private set; }
    public static float Fps { get; private set; }
    public static float Ups { get; private set; }
    public static float TimeScale { get; set; } = 1;
   
    internal static void UpdateUps(double deltaTime)
    {
        UnScaledDeltaTime = (float)deltaTime;
        DeltaTime = UnScaledDeltaTime * TimeScale;
        Ups = 1/UnScaledDeltaTime;
    }

    internal static void UpdateFps(double deltaTime)
    {
        Fps = (float)deltaTime;
        Fps = 1 / Fps;
    }
}