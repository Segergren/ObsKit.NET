namespace ObsKit.NET.Core;

/// <summary>
/// A snapshot of OBS rendering and encoding performance counters,
/// equivalent to the OBS Studio stats dock.
/// </summary>
public sealed class PerformanceStats
{
    internal PerformanceStats(double activeFps, ulong averageFrameTimeNs, uint totalComposedFrames,
        uint laggedFrames, uint totalOutputFrames, uint skippedFrames)
    {
        ActiveFps = activeFps;
        AverageFrameRenderTime = TimeSpan.FromTicks((long)(averageFrameTimeNs / 100));
        TotalComposedFrames = totalComposedFrames;
        LaggedFrames = laggedFrames;
        TotalOutputFrames = totalOutputFrames;
        SkippedFrames = skippedFrames;
    }

    /// <summary>
    /// Gets the current compositing frame rate.
    /// </summary>
    public double ActiveFps { get; }

    /// <summary>
    /// Gets the average time spent rendering a frame.
    /// </summary>
    public TimeSpan AverageFrameRenderTime { get; }

    /// <summary>
    /// Gets the total number of frames composited by the renderer.
    /// </summary>
    public uint TotalComposedFrames { get; }

    /// <summary>
    /// Gets the number of frames missed due to rendering lag
    /// (the GPU could not keep up with compositing).
    /// </summary>
    public uint LaggedFrames { get; }

    /// <summary>
    /// Gets the total number of frames delivered by the video output.
    /// </summary>
    public uint TotalOutputFrames { get; }

    /// <summary>
    /// Gets the number of frames skipped due to encoding lag
    /// (the encoder could not keep up).
    /// </summary>
    public uint SkippedFrames { get; }

    /// <summary>
    /// Gets the fraction of composited frames missed due to rendering lag (0.0-1.0).
    /// </summary>
    public double RenderingLagRatio => TotalComposedFrames > 0 ? (double)LaggedFrames / TotalComposedFrames : 0;

    /// <summary>
    /// Gets the fraction of output frames skipped due to encoding lag (0.0-1.0).
    /// </summary>
    public double EncodingLagRatio => TotalOutputFrames > 0 ? (double)SkippedFrames / TotalOutputFrames : 0;

    public override string ToString() =>
        $"FPS: {ActiveFps:F1}, render: {AverageFrameRenderTime.TotalMilliseconds:F2} ms, " +
        $"lagged: {LaggedFrames}/{TotalComposedFrames}, skipped: {SkippedFrames}/{TotalOutputFrames}";
}
