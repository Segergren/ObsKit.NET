namespace ObsKit.NET.Filters;

/// <summary>
/// Sharpness video filter (sharpness_filter).
/// </summary>
public sealed class SharpnessFilter : Filter
{
    /// <summary>
    /// The filter type ID for sharpness.
    /// </summary>
    public const string FilterTypeId = "sharpness_filter";

    /// <summary>
    /// Creates a sharpness filter.
    /// </summary>
    /// <param name="name">The filter name.</param>
    /// <param name="sharpness">The sharpness amount (0 to 1). Default: 0.08.</param>
    public SharpnessFilter(string name = "Sharpness", double sharpness = 0.08)
        : base(FilterTypeId, name)
    {
        if (sharpness != 0.08)
            SetSharpness(sharpness);
    }

    /// <summary>
    /// Sets the sharpness amount. Default: 0.08 (0 to 1).
    /// </summary>
    public SharpnessFilter SetSharpness(double sharpness)
    {
        Update(s => s.Set("sharpness", sharpness));
        return this;
    }
}
