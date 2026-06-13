namespace ObsKit.NET.Filters;

/// <summary>
/// Limiter audio filter (limiter_filter).
/// Hard-limits audio peaks above the threshold.
/// </summary>
public sealed class LimiterFilter : Filter
{
    /// <summary>
    /// The filter type ID for the limiter.
    /// </summary>
    public const string FilterTypeId = "limiter_filter";

    /// <summary>
    /// Creates a limiter filter with OBS defaults (threshold -6 dB, release 60 ms).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public LimiterFilter(string name = "Limiter")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the threshold above which audio is limited. Default: -6 dB.
    /// </summary>
    /// <param name="db">The threshold in decibels.</param>
    public LimiterFilter SetThreshold(double db)
    {
        Update(s => s.Set("threshold", db));
        return this;
    }

    /// <summary>
    /// Sets how quickly limiting disengages. Default: 60 ms.
    /// </summary>
    /// <param name="milliseconds">The release time in milliseconds.</param>
    public LimiterFilter SetReleaseTime(int milliseconds)
    {
        Update(s => s.Set("release_time", (long)milliseconds));
        return this;
    }
}
