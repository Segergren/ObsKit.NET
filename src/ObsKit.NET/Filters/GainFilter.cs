namespace ObsKit.NET.Filters;

/// <summary>
/// Gain audio filter (gain_filter).
/// Applies a fixed gain to the audio signal.
/// </summary>
public sealed class GainFilter : Filter
{
    /// <summary>
    /// The filter type ID for gain.
    /// </summary>
    public const string FilterTypeId = "gain_filter";

    /// <summary>
    /// Creates a gain filter.
    /// </summary>
    /// <param name="name">The filter name.</param>
    /// <param name="gainDb">The gain in decibels. Default: 0 dB.</param>
    public GainFilter(string name = "Gain", double gainDb = 0.0)
        : base(FilterTypeId, name)
    {
        if (gainDb != 0.0)
            SetGain(gainDb);
    }

    /// <summary>
    /// Sets the gain. Default: 0 dB.
    /// </summary>
    /// <param name="db">The gain in decibels (-30 to 30).</param>
    public GainFilter SetGain(double db)
    {
        Update(s => s.Set("db", db));
        return this;
    }
}
