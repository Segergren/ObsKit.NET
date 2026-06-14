namespace ObsKit.NET.Filters;

/// <summary>
/// Noise suppression audio filter (noise_suppress_filter_v2).
/// Removes background noise using Speex or RNNoise.
/// </summary>
public sealed class NoiseSuppressFilter : Filter
{
    /// <summary>
    /// The filter type ID for noise suppression.
    /// </summary>
    public const string FilterTypeId = "noise_suppress_filter_v2";

    /// <summary>
    /// Noise suppression methods.
    /// </summary>
    public enum SuppressionMethod
    {
        /// <summary>Speex suppression (low CPU usage, adjustable level).</summary>
        Speex,

        /// <summary>RNNoise AI suppression (better quality, higher CPU usage).</summary>
        RnNoise
    }

    /// <summary>
    /// Creates a noise suppression filter using OBS defaults
    /// (RNNoise when available, suppress level -30 dB for Speex).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public NoiseSuppressFilter(string name = "Noise Suppression")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the suppression method.
    /// </summary>
    /// <param name="method">The suppression method.</param>
    public NoiseSuppressFilter SetMethod(SuppressionMethod method)
    {
        Update(s => s.Set("method", method == SuppressionMethod.RnNoise ? "rnnoise" : "speex"));
        return this;
    }

    /// <summary>
    /// Sets the suppression level used by the Speex method. Default: -30 dB.
    /// </summary>
    /// <param name="db">The suppression level in decibels (-60 to 0).</param>
    public NoiseSuppressFilter SetSuppressLevel(int db)
    {
        Update(s => s.Set("suppress_level", (long)db));
        return this;
    }
}
