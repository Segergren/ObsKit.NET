using ObsKit.NET.Core;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents an image source for displaying static images.
/// </summary>
public sealed class ImageSource : Source
{
    /// <summary>
    /// The source type ID for image source.
    /// </summary>
    public const string SourceTypeId = "image_source";

    /// <summary>
    /// Creates an image source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="filePath">Path to the image file.</param>
    public ImageSource(string name, string? filePath = null)
        : base(SourceTypeId, name)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            SetFile(filePath);
        }
    }

    /// <summary>
    /// Creates an image source from a file.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>An image source.</returns>
    public static ImageSource FromFile(string filePath, string? name = null)
    {
        return new ImageSource(name ?? Path.GetFileName(filePath), filePath);
    }

    /// <summary>
    /// Sets the image file path.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    public ImageSource SetFile(string filePath)
    {
        Update(s => s.Set("file", filePath));
        return this;
    }

    /// <summary>
    /// Sets whether to unload the image when not showing.
    /// </summary>
    /// <param name="unload">Whether to unload when not showing.</param>
    public ImageSource SetUnloadWhenNotShowing(bool unload)
    {
        Update(s => s.Set("unload", unload));
        return this;
    }

    /// <summary>
    /// Sets linear alpha for the image.
    /// </summary>
    /// <param name="linear">Whether to use linear alpha.</param>
    public ImageSource SetLinearAlpha(bool linear)
    {
        Update(s => s.Set("linear_alpha", linear));
        return this;
    }
}
