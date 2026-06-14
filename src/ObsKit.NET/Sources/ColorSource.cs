namespace ObsKit.NET.Sources;

/// <summary>
/// Solid color source (color_source_v3).
/// </summary>
public sealed class ColorSource : Source
{
    /// <summary>
    /// The source type ID for the color source. Uses the current v3 registration
    /// (SRGB-aware); the bare "color_source" id is the obsolete v1.
    /// </summary>
    public const string SourceTypeId = "color_source_v3";

    /// <summary>
    /// Creates a color source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="abgr">The color as 0xAABBGGRR (OBS color format).</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    public ColorSource(string name = "Color", uint abgr = 0xFFD1D1D1, int width = 1920, int height = 1080)
        : base(SourceTypeId, name)
    {
        Update(s => s
            .Set("color", (long)abgr)
            .Set("width", (long)width)
            .Set("height", (long)height));
    }

    /// <summary>
    /// Sets the color as 0xAABBGGRR (OBS color format).
    /// </summary>
    public ColorSource SetColor(uint abgr)
    {
        Update(s => s.Set("color", (long)abgr));
        return this;
    }

    /// <summary>
    /// Sets the source dimensions.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    public ColorSource SetSize(int width, int height)
    {
        Update(s => s
            .Set("width", (long)width)
            .Set("height", (long)height));
        return this;
    }
}
