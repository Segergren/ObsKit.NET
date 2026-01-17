using ObsKit.NET.Core;
using ObsKit.NET.Platform;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents a window capture source.
/// </summary>
public sealed class WindowCapture : Source
{
    /// <summary>
    /// The source type ID for Windows window capture.
    /// </summary>
    public const string WindowsTypeId = "window_capture";

    /// <summary>
    /// The source type ID for Linux window capture (PipeWire).
    /// </summary>
    public const string LinuxTypeId = "pipewire-window-capture-source";

    /// <summary>
    /// The source type ID for macOS window capture.
    /// </summary>
    public const string MacOSTypeId = "window_capture";

    /// <summary>
    /// Gets the platform-appropriate source type ID.
    /// </summary>
    public static string TypeIdForPlatform => OperatingSystem.IsWindows() ? WindowsTypeId :
                                               OperatingSystem.IsLinux() ? LinuxTypeId :
                                               OperatingSystem.IsMacOS() ? MacOSTypeId :
                                               WindowsTypeId;

    /// <summary>
    /// Creates a window capture source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="windowId">The OBS window ID to capture.</param>
    /// <param name="captureCursor">Whether to capture the cursor.</param>
    public WindowCapture(string name, string? windowId = null, bool captureCursor = true)
        : base(TypeIdForPlatform, name)
    {
        ApplySettings(windowId, captureCursor);
    }

    /// <summary>
    /// Creates a window capture source for a specific window title.
    /// </summary>
    /// <param name="windowTitle">The window title to capture.</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>A window capture source.</returns>
    public static WindowCapture FromWindowTitle(string windowTitle, string? name = null)
    {
        return new WindowCapture(name ?? $"Window: {windowTitle}", windowTitle, true);
    }

    /// <summary>
    /// Creates a window capture source from a WindowInfo object.
    /// </summary>
    /// <param name="window">The window to capture.</param>
    /// <param name="captureCursor">Whether to capture the cursor.</param>
    /// <returns>A window capture source.</returns>
    public static WindowCapture FromWindow(WindowInfo window, bool captureCursor = true)
    {
        return new WindowCapture(window.Title, window.ObsId, captureCursor);
    }

    /// <summary>
    /// Creates a window capture source that will prompt for window selection.
    /// </summary>
    /// <param name="name">Optional source name.</param>
    /// <returns>A window capture source.</returns>
    public static WindowCapture Create(string? name = null)
    {
        return new WindowCapture(name ?? "Window Capture");
    }

    /// <summary>
    /// Gets all available windows for capture.
    /// </summary>
    public static IReadOnlyList<WindowInfo> AvailableWindows => Platform.Platform.Windows;

    private void ApplySettings(string? windowId, bool captureCursor)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
            {
                s.Set("cursor", captureCursor);
                if (!string.IsNullOrEmpty(windowId))
                {
                    s.Set("window", windowId);
                }
            }
            else
            {
                s.Set("show_cursor", captureCursor);
            }
        });
    }

    /// <summary>
    /// Sets whether to capture the cursor.
    /// </summary>
    /// <param name="capture">Whether to capture the cursor.</param>
    public WindowCapture SetCaptureCursor(bool capture)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
                s.Set("cursor", capture);
            else
                s.Set("show_cursor", capture);
        });
        return this;
    }

    /// <summary>
    /// Sets the window to capture using OBS window ID format.
    /// On Windows, format is typically: "WindowTitle:ClassName:ProcessName.exe"
    /// </summary>
    /// <param name="windowId">The window identifier.</param>
    public WindowCapture SetWindow(string windowId)
    {
        Update(s => s.Set("window", windowId));
        return this;
    }

    /// <summary>
    /// Sets the window to capture.
    /// </summary>
    /// <param name="window">The window info.</param>
    public WindowCapture SetWindow(WindowInfo window)
    {
        return SetWindow(window.ObsId);
    }

    /// <summary>
    /// Sets whether to use client area only (Windows).
    /// </summary>
    /// <param name="clientArea">Whether to capture only the client area.</param>
    public WindowCapture SetClientArea(bool clientArea)
    {
        if (OperatingSystem.IsWindows())
        {
            Update(s => s.Set("client_area", clientArea));
        }
        return this;
    }
}
