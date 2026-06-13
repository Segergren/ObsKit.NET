using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Filters;

/// <summary>
/// Scaling/aspect ratio video filter (scale_filter).
/// Rescales a source before it reaches the canvas.
/// </summary>
public sealed class ScaleFilter : Filter
{
    /// <summary>
    /// The filter type ID for scaling.
    /// </summary>
    public const string FilterTypeId = "scale_filter";

    /// <summary>
    /// Creates a scale filter.
    /// </summary>
    /// <param name="name">The filter name.</param>
    public ScaleFilter(string name = "Scaling/Aspect Ratio")
        : base(FilterTypeId, name)
    {
    }

    /// <summary>
    /// Sets the target resolution.
    /// </summary>
    /// <param name="width">The target width in pixels.</param>
    /// <param name="height">The target height in pixels.</param>
    public ScaleFilter SetResolution(int width, int height)
    {
        Update(s => s.Set("resolution", $"{width}x{height}"));
        return this;
    }

    /// <summary>
    /// Sets the scaling algorithm. Default: bicubic.
    /// </summary>
    /// <param name="sampling">The scaling algorithm (Point, Bilinear, Bicubic, Lanczos, or Area).</param>
    public ScaleFilter SetSampling(ObsScaleType sampling)
    {
        Update(s => s.Set("sampling", sampling switch
        {
            ObsScaleType.Point => "point",
            ObsScaleType.Bilinear => "bilinear",
            ObsScaleType.Lanczos => "lanczos",
            ObsScaleType.Area => "area",
            _ => "bicubic"
        }));
        return this;
    }

    /// <summary>
    /// Sets whether to undistort the aspect ratio when scaling.
    /// </summary>
    public ScaleFilter SetUndistort(bool undistort)
    {
        Update(s => s.Set("undistort", undistort));
        return this;
    }
}
