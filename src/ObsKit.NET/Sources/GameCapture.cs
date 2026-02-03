using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Signals;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents a game capture source (Windows-only).
/// Uses GPU-based capture for better performance with games.
/// Automatically tracks hook state via IsHooked property.
/// </summary>
public sealed class GameCapture : Source
{
    private SignalConnection? _hookedConnection;
    private SignalConnection? _unhookedConnection;
    /// <summary>
    /// The source type ID for game capture.
    /// </summary>
    public const string SourceTypeId = "game_capture";

    /// <summary>
    /// Capture modes for game capture.
    /// </summary>
    public enum CaptureMode
    {
        /// <summary>
        /// Capture any fullscreen application.
        /// </summary>
        AnyFullscreen,

        /// <summary>
        /// Capture a specific window.
        /// </summary>
        SpecificWindow,

        /// <summary>
        /// Capture the foreground window with a hotkey.
        /// </summary>
        HotkeyForeground
    }

    /// <summary>
    /// Gets whether the game capture has successfully hooked into a game.
    /// </summary>
    public bool IsHooked { get; private set; }

    /// <summary>
    /// Gets the executable name of the hooked game, or null if not hooked.
    /// </summary>
    public string? HookedExecutable { get; private set; }

    /// <summary>
    /// Gets the window title of the hooked game, or null if not hooked.
    /// </summary>
    public string? HookedWindowTitle { get; private set; }

    /// <summary>
    /// Gets the window class of the hooked game, or null if not hooked.
    /// </summary>
    public string? HookedWindowClass { get; private set; }

    /// <summary>
    /// Event raised when the game capture hooks into a game.
    /// </summary>
    public event Action<GameCapture>? Hooked;

    /// <summary>
    /// Event raised when the game capture unhooks from a game.
    /// </summary>
    public event Action<GameCapture>? Unhooked;

    /// <summary>
    /// Creates a game capture source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="mode">The capture mode.</param>
    /// <param name="captureCursor">Whether to capture the cursor.</param>
    public GameCapture(string name, CaptureMode mode = CaptureMode.AnyFullscreen, bool captureCursor = true)
        : base(SourceTypeId, name)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Game capture is only supported on Windows.");

        ApplySettings(mode, null, captureCursor);
        SubscribeToHookSignals();
    }

    private void SubscribeToHookSignals()
    {
        _hookedConnection = ConnectSignal(SourceSignal.Hooked, OnHooked);
        _unhookedConnection = ConnectSignal(SourceSignal.Unhooked, OnUnhooked);
    }

    private void OnHooked(nint calldata)
    {
        HookedWindowTitle = Calldata.GetString(calldata, "title");
        HookedWindowClass = Calldata.GetString(calldata, "class");
        HookedExecutable = Calldata.GetString(calldata, "executable");
        IsHooked = true;
        Hooked?.Invoke(this);
    }

    private void OnUnhooked(nint calldata)
    {
        IsHooked = false;
        HookedWindowTitle = null;
        HookedWindowClass = null;
        HookedExecutable = null;
        Unhooked?.Invoke(this);
    }

    /// <summary>
    /// Creates a game capture source for any fullscreen application.
    /// </summary>
    /// <param name="name">Optional source name.</param>
    /// <returns>A game capture source.</returns>
    public static GameCapture CaptureFullscreen(string? name = null)
    {
        return new GameCapture(name ?? "Game Capture", CaptureMode.AnyFullscreen);
    }

    /// <summary>
    /// Creates a game capture source for a specific window.
    /// </summary>
    /// <param name="windowId">The window identifier (format: WindowClass:ProcessName.exe:WindowTitle).</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>A game capture source.</returns>
    public static GameCapture CaptureWindow(string windowId, string? name = null)
    {
        var capture = new GameCapture(name ?? "Game Capture", CaptureMode.SpecificWindow, true);
        capture.SetWindow(windowId);
        return capture;
    }

    private void ApplySettings(CaptureMode mode, string? windowId, bool captureCursor)
    {
        Update(s =>
        {
            // capture_mode: "any_fullscreen", "window", "hotkey"
            s.Set("capture_mode", mode switch
            {
                CaptureMode.AnyFullscreen => "any_fullscreen",
                CaptureMode.SpecificWindow => "window",
                CaptureMode.HotkeyForeground => "hotkey",
                _ => "any_fullscreen"
            });

            s.Set("capture_cursor", captureCursor);

            if (!string.IsNullOrEmpty(windowId))
            {
                s.Set("window", windowId);
            }
        });
    }

    /// <summary>
    /// Sets whether to capture the cursor.
    /// </summary>
    /// <param name="capture">Whether to capture the cursor.</param>
    public GameCapture SetCaptureCursor(bool capture)
    {
        Update(s => s.Set("capture_cursor", capture));
        return this;
    }

    /// <summary>
    /// Sets whether to allow transparency.
    /// </summary>
    /// <param name="allow">Whether to allow transparency.</param>
    public GameCapture SetAllowTransparency(bool allow)
    {
        Update(s => s.Set("allow_transparency", allow));
        return this;
    }

    /// <summary>
    /// Sets whether to force scaling.
    /// </summary>
    /// <param name="force">Whether to force scaling.</param>
    public GameCapture SetForceScaling(bool force)
    {
        Update(s => s.Set("force_scaling", force));
        return this;
    }

    /// <summary>
    /// Sets the capture mode.
    /// </summary>
    /// <param name="mode">The capture mode.</param>
    public GameCapture SetCaptureMode(CaptureMode mode)
    {
        Update(s => s.Set("capture_mode", mode switch
        {
            CaptureMode.AnyFullscreen => "any_fullscreen",
            CaptureMode.SpecificWindow => "window",
            CaptureMode.HotkeyForeground => "hotkey",
            _ => "any_fullscreen"
        }));
        return this;
    }

    /// <summary>
    /// Sets the window to capture.
    /// </summary>
    /// <param name="windowId">The window identifier (format: WindowClass:ProcessName.exe:WindowTitle).</param>
    public GameCapture SetWindow(string windowId)
    {
        Update(s => s.Set("window", windowId));
        return this;
    }

    /// <summary>
    /// Sets whether to use anti-cheat compatibility hook.
    /// </summary>
    /// <param name="enable">Whether to enable anti-cheat hook.</param>
    public GameCapture SetAntiCheatHook(bool enable)
    {
        Update(s => s.Set("anti_cheat_hook", enable));
        return this;
    }

    /// <summary>
    /// Sets whether to limit the capture framerate.
    /// </summary>
    /// <param name="limit">Whether to limit framerate.</param>
    public GameCapture SetLimitFramerate(bool limit)
    {
        Update(s => s.Set("limit_framerate", limit));
        return this;
    }

    /// <summary>
    /// Disposes the game capture and disconnects signal handlers.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hookedConnection?.Dispose();
            _hookedConnection = null;
            _unhookedConnection?.Dispose();
            _unhookedConnection = null;
        }
        base.Dispose(disposing);
    }
}
