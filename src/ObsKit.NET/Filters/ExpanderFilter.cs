namespace ObsKit.NET.Filters;

/// <summary>
/// Expander audio filter (expander_filter).
/// Reduces audio below a threshold; a smoother alternative to a noise gate.
/// </summary>
public sealed class ExpanderFilter : Filter
{
    /// <summary>
    /// The filter type ID for the expander.
    /// </summary>
    public const string FilterTypeId = "expander_filter";

    /// <summary>
    /// Expander presets defining the default behavior.
    /// </summary>
    public enum ExpanderPreset
    {
        /// <summary>Gentle expansion (ratio 2:1, release 50 ms).</summary>
        Expander,

        /// <summary>Aggressive gate-like expansion (ratio 10:1, release 125 ms).</summary>
        Gate
    }

    /// <summary>
    /// Level detection modes.
    /// </summary>
    public enum DetectorMode
    {
        /// <summary>Root-mean-square detection (smoother, default).</summary>
        Rms,

        /// <summary>Peak detection (faster response).</summary>
        Peak
    }

    /// <summary>
    /// Creates an expander filter with OBS defaults
    /// (expander preset, threshold -40 dB, attack 10 ms, output gain 0 dB, RMS detector).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public ExpanderFilter(string name = "Expander")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the preset, which adjusts ratio and release defaults.
    /// </summary>
    /// <param name="preset">The preset to apply.</param>
    public ExpanderFilter SetPreset(ExpanderPreset preset)
    {
        Update(s => s.Set("presets", preset == ExpanderPreset.Gate ? "gate" : "expander"));
        return this;
    }

    /// <summary>
    /// Sets the expansion ratio. Default: 2 (expander preset) or 10 (gate preset).
    /// </summary>
    /// <param name="ratio">The ratio (1 to 20).</param>
    public ExpanderFilter SetRatio(double ratio)
    {
        Update(s => s.Set("ratio", ratio));
        return this;
    }

    /// <summary>
    /// Sets the threshold below which audio is reduced. Default: -40 dB.
    /// </summary>
    /// <param name="db">The threshold in decibels.</param>
    public ExpanderFilter SetThreshold(double db)
    {
        Update(s => s.Set("threshold", db));
        return this;
    }

    /// <summary>
    /// Sets how quickly expansion disengages when audio rises. Default: 10 ms.
    /// </summary>
    /// <param name="milliseconds">The attack time in milliseconds.</param>
    public ExpanderFilter SetAttackTime(int milliseconds)
    {
        Update(s => s.Set("attack_time", (long)milliseconds));
        return this;
    }

    /// <summary>
    /// Sets how quickly expansion engages when audio falls. Default: 50 ms (expander) or 125 ms (gate).
    /// </summary>
    /// <param name="milliseconds">The release time in milliseconds.</param>
    public ExpanderFilter SetReleaseTime(int milliseconds)
    {
        Update(s => s.Set("release_time", (long)milliseconds));
        return this;
    }

    /// <summary>
    /// Sets the gain applied after expansion. Default: 0 dB.
    /// </summary>
    /// <param name="db">The output gain in decibels.</param>
    public ExpanderFilter SetOutputGain(double db)
    {
        Update(s => s.Set("output_gain", db));
        return this;
    }

    /// <summary>
    /// Sets the level detection mode. Default: RMS.
    /// </summary>
    /// <param name="mode">The detection mode.</param>
    public ExpanderFilter SetDetector(DetectorMode mode)
    {
        Update(s => s.Set("detector", mode == DetectorMode.Peak ? "peak" : "RMS"));
        return this;
    }
}
