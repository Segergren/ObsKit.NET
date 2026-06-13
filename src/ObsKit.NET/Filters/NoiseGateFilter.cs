namespace ObsKit.NET.Filters;

/// <summary>
/// Noise gate audio filter (noise_gate_filter).
/// Mutes audio below the close threshold and opens above the open threshold.
/// </summary>
public sealed class NoiseGateFilter : Filter
{
    /// <summary>
    /// The filter type ID for the noise gate.
    /// </summary>
    public const string FilterTypeId = "noise_gate_filter";

    /// <summary>
    /// Creates a noise gate filter with OBS defaults
    /// (open -26 dB, close -32 dB, attack 25 ms, hold 200 ms, release 150 ms).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public NoiseGateFilter(string name = "Noise Gate")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the threshold above which the gate opens. Default: -26 dB.
    /// </summary>
    /// <param name="db">The open threshold in decibels.</param>
    public NoiseGateFilter SetOpenThreshold(double db)
    {
        Update(s => s.Set("open_threshold", db));
        return this;
    }

    /// <summary>
    /// Sets the threshold below which the gate closes. Default: -32 dB.
    /// </summary>
    /// <param name="db">The close threshold in decibels.</param>
    public NoiseGateFilter SetCloseThreshold(double db)
    {
        Update(s => s.Set("close_threshold", db));
        return this;
    }

    /// <summary>
    /// Sets how quickly the gate opens. Default: 25 ms.
    /// </summary>
    /// <param name="milliseconds">The attack time in milliseconds.</param>
    public NoiseGateFilter SetAttackTime(int milliseconds)
    {
        Update(s => s.Set("attack_time", (long)milliseconds));
        return this;
    }

    /// <summary>
    /// Sets how long the gate stays open after falling below the close threshold. Default: 200 ms.
    /// </summary>
    /// <param name="milliseconds">The hold time in milliseconds.</param>
    public NoiseGateFilter SetHoldTime(int milliseconds)
    {
        Update(s => s.Set("hold_time", (long)milliseconds));
        return this;
    }

    /// <summary>
    /// Sets how quickly the gate closes. Default: 150 ms.
    /// </summary>
    /// <param name="milliseconds">The release time in milliseconds.</param>
    public NoiseGateFilter SetReleaseTime(int milliseconds)
    {
        Update(s => s.Set("release_time", (long)milliseconds));
        return this;
    }
}
