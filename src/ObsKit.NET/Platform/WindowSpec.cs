namespace ObsKit.NET.Platform;

/// <summary>
/// Helpers for building OBS window specification strings ("Title:Class:Executable").
/// OBS encodes each part so that literal ':' and '#' characters survive the join.
/// </summary>
public static class WindowSpec
{
    /// <summary>
    /// Encodes a single window spec part ('#' becomes "#22", ':' becomes "#3A").
    /// </summary>
    /// <param name="value">The raw title, class name, or executable name.</param>
    public static string Encode(string value)
    {
        return value.Replace("#", "#22").Replace(":", "#3A");
    }

    /// <summary>
    /// Builds an OBS window specification string from its parts.
    /// Used by window capture, game capture, and application audio capture sources.
    /// </summary>
    /// <param name="title">The window title (may be empty).</param>
    /// <param name="className">The window class name (may be empty).</param>
    /// <param name="executable">The executable file name, e.g. "Discord.exe" (may be empty).</param>
    /// <returns>The encoded spec, e.g. "Discord:Chrome_WidgetWin_1:Discord.exe".</returns>
    public static string Build(string title, string className, string executable)
    {
        return $"{Encode(title)}:{Encode(className)}:{Encode(executable)}";
    }
}
