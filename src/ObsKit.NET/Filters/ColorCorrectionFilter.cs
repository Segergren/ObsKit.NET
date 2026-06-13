namespace ObsKit.NET.Filters;

/// <summary>
/// Color correction video filter (color_filter).
/// Adjusts gamma, contrast, brightness, saturation, hue, and opacity.
/// </summary>
public sealed class ColorCorrectionFilter : Filter
{
    /// <summary>The filter type ID for the color correction (v2, SDR/sRGB) filter.</summary>
    public const string FilterTypeId = "color_filter_v2";

    /// <summary>
    /// Creates a color correction filter with neutral defaults.
    /// </summary>
    /// <param name="name">The filter name.</param>
    public ColorCorrectionFilter(string name = "Color Correction")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the gamma adjustment. Default: 0 (-3 to 3).
    /// </summary>
    public ColorCorrectionFilter SetGamma(double gamma)
    {
        Update(s => s.Set("gamma", gamma));
        return this;
    }

    /// <summary>
    /// Sets the contrast adjustment. Default: 0 (-4 to 4).
    /// </summary>
    public ColorCorrectionFilter SetContrast(double contrast)
    {
        Update(s => s.Set("contrast", contrast));
        return this;
    }

    /// <summary>
    /// Sets the brightness adjustment. Default: 0 (-1 to 1).
    /// </summary>
    public ColorCorrectionFilter SetBrightness(double brightness)
    {
        Update(s => s.Set("brightness", brightness));
        return this;
    }

    /// <summary>
    /// Sets the saturation adjustment. Default: 0 (-1 to 5).
    /// </summary>
    public ColorCorrectionFilter SetSaturation(double saturation)
    {
        Update(s => s.Set("saturation", saturation));
        return this;
    }

    /// <summary>
    /// Sets the hue shift in degrees. Default: 0 (-180 to 180).
    /// </summary>
    public ColorCorrectionFilter SetHueShift(double degrees)
    {
        Update(s => s.Set("hue_shift", degrees));
        return this;
    }

    /// <summary>
    /// Sets the opacity. Default: 1.0 (0 to 1).
    /// </summary>
    public ColorCorrectionFilter SetOpacity(double opacity)
    {
        Update(s => s.Set("opacity", opacity));
        return this;
    }

    /// <summary>
    /// Sets the multiply color as 0xAABBGGRR (OBS color format). Default: 0x00FFFFFF (no change).
    /// </summary>
    public ColorCorrectionFilter SetColorMultiply(uint abgr)
    {
        Update(s => s.Set("color_multiply", (long)abgr));
        return this;
    }

    /// <summary>
    /// Sets the additive color as 0xAABBGGRR (OBS color format). Default: 0x00000000 (no change).
    /// </summary>
    public ColorCorrectionFilter SetColorAdd(uint abgr)
    {
        Update(s => s.Set("color_add", (long)abgr));
        return this;
    }
}
