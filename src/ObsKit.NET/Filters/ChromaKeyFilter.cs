namespace ObsKit.NET.Filters;

/// <summary>
/// Chroma key video filter (chroma_key_filter).
/// Makes a key color (e.g. a green screen) transparent.
/// </summary>
public sealed class ChromaKeyFilter : Filter
{
    /// <summary>The filter type ID for the chroma key (v2, SDR/sRGB) filter.</summary>
    public const string FilterTypeId = "chroma_key_filter_v2";

    /// <summary>
    /// Preset key colors.
    /// </summary>
    public enum KeyColor
    {
        /// <summary>Key out green (default).</summary>
        Green,

        /// <summary>Key out blue.</summary>
        Blue,

        /// <summary>Key out magenta.</summary>
        Magenta,

        /// <summary>Key out a custom color set via <see cref="SetCustomKeyColor"/>.</summary>
        Custom
    }

    /// <summary>
    /// Creates a chroma key filter with OBS defaults
    /// (green key, similarity 400, smoothness 80, spill 100).
    /// </summary>
    /// <param name="name">The filter name.</param>
    public ChromaKeyFilter(string name = "Chroma Key")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the key color preset. Default: green.
    /// </summary>
    public ChromaKeyFilter SetKeyColor(KeyColor color)
    {
        Update(s => s.Set("key_color_type", color switch
        {
            KeyColor.Blue => "blue",
            KeyColor.Magenta => "magenta",
            KeyColor.Custom => "custom",
            _ => "green"
        }));
        return this;
    }

    /// <summary>
    /// Sets a custom key color as 0xAABBGGRR (OBS color format) and switches to custom mode.
    /// </summary>
    public ChromaKeyFilter SetCustomKeyColor(uint abgr)
    {
        Update(s => s
            .Set("key_color_type", "custom")
            .Set("key_color", (long)abgr));
        return this;
    }

    /// <summary>
    /// Sets the color similarity. Default: 400 (1 to 1000).
    /// </summary>
    public ChromaKeyFilter SetSimilarity(int similarity)
    {
        Update(s => s.Set("similarity", (long)similarity));
        return this;
    }

    /// <summary>
    /// Sets the edge smoothness. Default: 80 (1 to 1000).
    /// </summary>
    public ChromaKeyFilter SetSmoothness(int smoothness)
    {
        Update(s => s.Set("smoothness", (long)smoothness));
        return this;
    }

    /// <summary>
    /// Sets the key color spill reduction. Default: 100 (1 to 1000).
    /// </summary>
    public ChromaKeyFilter SetSpillReduction(int spill)
    {
        Update(s => s.Set("spill", (long)spill));
        return this;
    }

    /// <summary>
    /// Sets the opacity of the keyed image. Default: 1.0 (0 to 1).
    /// </summary>
    public ChromaKeyFilter SetOpacity(double opacity)
    {
        Update(s => s.Set("opacity", opacity));
        return this;
    }

    /// <summary>
    /// Sets the contrast adjustment. Default: 0 (-4 to 4).
    /// </summary>
    public ChromaKeyFilter SetContrast(double contrast)
    {
        Update(s => s.Set("contrast", contrast));
        return this;
    }

    /// <summary>
    /// Sets the brightness adjustment. Default: 0 (-1 to 1).
    /// </summary>
    public ChromaKeyFilter SetBrightness(double brightness)
    {
        Update(s => s.Set("brightness", brightness));
        return this;
    }

    /// <summary>
    /// Sets the gamma adjustment. Default: 0 (-1 to 1).
    /// </summary>
    public ChromaKeyFilter SetGamma(double gamma)
    {
        Update(s => s.Set("gamma", gamma));
        return this;
    }
}
