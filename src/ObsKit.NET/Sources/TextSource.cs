using ObsKit.NET.Core;

namespace ObsKit.NET.Sources;

/// <summary>
/// Text source (text_gdiplus on Windows, text_ft2_source elsewhere).
/// Renders a text string with a configurable font, color, and outline.
/// </summary>
public sealed class TextSource : Source
{
    /// <summary>
    /// The source type ID for Windows text rendering (GDI+).
    /// </summary>
    public const string WindowsTypeId = "text_gdiplus";

    /// <summary>
    /// The source type ID for FreeType2 text rendering (Linux/macOS).
    /// </summary>
    public const string FreeType2TypeId = "text_ft2_source";

    /// <summary>
    /// Font style flags (OBS_FONT_*).
    /// </summary>
    [Flags]
    public enum FontStyle
    {
        /// <summary>Regular text.</summary>
        Regular = 0,

        /// <summary>Bold text.</summary>
        Bold = 1 << 0,

        /// <summary>Italic text.</summary>
        Italic = 1 << 1,

        /// <summary>Underlined text.</summary>
        Underline = 1 << 2,

        /// <summary>Strikethrough text.</summary>
        Strikeout = 1 << 3
    }

    /// <summary>
    /// Gets the platform-appropriate source type ID.
    /// </summary>
    public static string TypeIdForPlatform => OperatingSystem.IsWindows() ? WindowsTypeId : FreeType2TypeId;

    /// <summary>
    /// Creates a text source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="text">Optional initial text.</param>
    public TextSource(string name = "Text", string? text = null)
        : base(TypeIdForPlatform, name)
    {
        if (!string.IsNullOrEmpty(text))
            SetText(text);
    }

    /// <summary>
    /// Sets the displayed text.
    /// </summary>
    public TextSource SetText(string text)
    {
        Update(s => s.Set("text", text));
        return this;
    }

    /// <summary>
    /// Reads the displayed text from a UTF-8 text file. OBS watches the file and
    /// re-renders when it changes — useful for "now playing" or score displays
    /// written by another process.
    /// </summary>
    /// <param name="path">The path to the text file.</param>
    public TextSource SetTextFromFile(string path)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
            {
                s.Set("read_from_file", true);
                s.Set("file", path);
            }
            else
            {
                s.Set("from_file", true);
                s.Set("text_file", path);
            }
        });
        return this;
    }

    /// <summary>
    /// Switches back to showing the text set via <see cref="SetText"/>.
    /// </summary>
    public TextSource UseInlineText()
    {
        Update(s => s.Set(OperatingSystem.IsWindows() ? "read_from_file" : "from_file", false));
        return this;
    }

    /// <summary>
    /// Sets the font.
    /// </summary>
    /// <param name="face">The font face name (e.g. "Arial").</param>
    /// <param name="size">The font size in points.</param>
    /// <param name="style">Optional style flags.</param>
    public TextSource SetFont(string face, int size, FontStyle style = FontStyle.Regular)
    {
        using var font = new Settings();
        font.Set("face", face);
        font.Set("size", (long)size);
        font.Set("flags", (long)style);
        Update(s => s.Set("font", font));
        return this;
    }

    /// <summary>
    /// Sets the text color as 0xAABBGGRR (OBS color format).
    /// </summary>
    public TextSource SetColor(uint abgr)
    {
        if (OperatingSystem.IsWindows())
        {
            // GDI+ uses an RGB color plus a separate opacity percentage.
            var alpha = (abgr >> 24) & 0xFF;
            Update(s => s
                .Set("color", (long)(abgr & 0xFFFFFF))
                .Set("opacity", (long)(alpha * 100 / 255)));
        }
        else
        {
            // FreeType2 uses two alpha colors for a vertical gradient.
            Update(s => s
                .Set("color1", (long)abgr)
                .Set("color2", (long)abgr));
        }
        return this;
    }

    /// <summary>
    /// Enables or disables the text outline.
    /// </summary>
    public TextSource SetOutline(bool enabled)
    {
        Update(s => s.Set("outline", enabled));
        return this;
    }

    /// <summary>
    /// Enables or disables word wrapping. On Windows this enables wrapping at the
    /// given extent width; elsewhere it sets a custom width with word wrap.
    /// </summary>
    /// <param name="width">The wrap width in pixels.</param>
    public TextSource SetWordWrap(int width)
    {
        if (OperatingSystem.IsWindows())
        {
            Update(s => s
                .Set("extents", true)
                .Set("extents_wrap", true)
                .Set("extents_cx", (long)width));
        }
        else
        {
            Update(s => s
                .Set("word_wrap", true)
                .Set("custom_width", (long)width));
        }
        return this;
    }
}
