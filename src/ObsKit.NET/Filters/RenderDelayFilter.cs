namespace ObsKit.NET.Filters;

/// <summary>
/// Render delay video filter (gpu_delay).
/// Delays a source's video by buffering rendered frames on the GPU.
/// </summary>
public sealed class RenderDelayFilter : Filter
{
    /// <summary>
    /// The filter type ID for render delay.
    /// </summary>
    public const string FilterTypeId = "gpu_delay";

    /// <summary>
    /// Creates a render delay filter.
    /// </summary>
    /// <param name="name">The filter name.</param>
    /// <param name="delayMs">The delay in milliseconds (0 to 500).</param>
    public RenderDelayFilter(string name = "Render Delay", int delayMs = 0)
        : base(FilterTypeId, name)
    {
        if (delayMs != 0)
            SetDelay(delayMs);
    }

    /// <summary>
    /// Sets the video delay. Maximum: 500 ms.
    /// </summary>
    /// <param name="milliseconds">The delay in milliseconds.</param>
    public RenderDelayFilter SetDelay(int milliseconds)
    {
        Update(s => s.Set("delay_ms", (long)milliseconds));
        return this;
    }
}
