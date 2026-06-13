namespace ObsKit.NET.Filters;

/// <summary>
/// Compressor audio filter (compressor_filter).
/// Reduces the dynamic range of audio above a threshold.
/// </summary>
public sealed class CompressorFilter : Filter
{
    /// <summary>
    /// The filter type ID for the compressor.
    /// </summary>
    public const string FilterTypeId = "compressor_filter";

    /// <summary>
    /// Creates a compressor filter with OBS defaults
    /// (ratio 10:1, threshold -18 dB, attack 6 ms, release 60 ms, output gain 0 dB).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public CompressorFilter(string name = "Compressor")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the compression ratio. Default: 10 (i.e. 10:1).
    /// </summary>
    /// <param name="ratio">The ratio (1 to 32).</param>
    public CompressorFilter SetRatio(double ratio)
    {
        Update(s => s.Set("ratio", ratio));
        return this;
    }

    /// <summary>
    /// Sets the threshold above which compression is applied. Default: -18 dB.
    /// </summary>
    /// <param name="db">The threshold in decibels.</param>
    public CompressorFilter SetThreshold(double db)
    {
        Update(s => s.Set("threshold", db));
        return this;
    }

    /// <summary>
    /// Sets how quickly compression engages. Default: 6 ms.
    /// </summary>
    /// <param name="milliseconds">The attack time in milliseconds.</param>
    public CompressorFilter SetAttackTime(int milliseconds)
    {
        Update(s => s.Set("attack_time", (long)milliseconds));
        return this;
    }

    /// <summary>
    /// Sets how quickly compression disengages. Default: 60 ms.
    /// </summary>
    /// <param name="milliseconds">The release time in milliseconds.</param>
    public CompressorFilter SetReleaseTime(int milliseconds)
    {
        Update(s => s.Set("release_time", (long)milliseconds));
        return this;
    }

    /// <summary>
    /// Sets the make-up gain applied after compression. Default: 0 dB.
    /// </summary>
    /// <param name="db">The output gain in decibels.</param>
    public CompressorFilter SetOutputGain(double db)
    {
        Update(s => s.Set("output_gain", db));
        return this;
    }

    /// <summary>
    /// Sets a sidechain/ducking source by name; pass null to disable.
    /// When set, that source's level drives the compression instead of the filtered audio.
    /// </summary>
    /// <param name="sourceName">The name of the sidechain source, or null for none.</param>
    public CompressorFilter SetSidechainSource(string? sourceName)
    {
        Update(s => s.Set("sidechain_source", sourceName ?? "none"));
        return this;
    }
}
