namespace ObsKit.NET.Filters;

/// <summary>
/// Crop/pad video filter (crop_filter).
/// Crops pixels from the edges of a source, or pads when values are negative.
/// </summary>
public sealed class CropFilter : Filter
{
    /// <summary>
    /// The filter type ID for crop/pad.
    /// </summary>
    public const string FilterTypeId = "crop_filter";

    /// <summary>
    /// Creates a crop filter (relative mode by default).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public CropFilter(string name = "Crop/Pad")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Crops the given number of pixels from each edge (negative values pad).
    /// </summary>
    /// <param name="left">Pixels to remove from the left.</param>
    /// <param name="top">Pixels to remove from the top.</param>
    /// <param name="right">Pixels to remove from the right.</param>
    /// <param name="bottom">Pixels to remove from the bottom.</param>
    public CropFilter SetCrop(int left, int top, int right, int bottom)
    {
        Update(s => s
            .Set("relative", true)
            .Set("left", (long)left)
            .Set("top", (long)top)
            .Set("right", (long)right)
            .Set("bottom", (long)bottom));
        return this;
    }

    /// <summary>
    /// Crops to an absolute region of the source.
    /// </summary>
    /// <param name="x">The left edge of the region.</param>
    /// <param name="y">The top edge of the region.</param>
    /// <param name="width">The region width.</param>
    /// <param name="height">The region height.</param>
    public CropFilter SetAbsoluteCrop(int x, int y, int width, int height)
    {
        Update(s => s
            .Set("relative", false)
            .Set("left", (long)x)
            .Set("top", (long)y)
            .Set("cx", (long)width)
            .Set("cy", (long)height));
        return this;
    }
}
