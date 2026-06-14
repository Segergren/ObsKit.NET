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
    /// How often game capture attempts to hook newly started games.
    /// </summary>
    public enum HookRate
    {
        /// <summary>Check every 4 seconds (lowest overhead).</summary>
        Slow = 0,
        /// <summary>Check every 2 seconds (default).</summary>
        Normal = 1,
        /// <summary>Check every second.</summary>
        Fast = 2,
        /// <summary>Check every 0.2 seconds (hooks fastest, highest overhead).</summary>
        Fastest = 3
    }

    /// <summary>
    /// How 10-bit (RGB10A2) game output is interpreted.
    /// </summary>
    public enum Rgb10A2ColorSpace
    {
        /// <summary>Treat RGB10A2 frames as SDR sRGB (default).</summary>
        Srgb,
        /// <summary>Treat RGB10A2 frames as HDR Rec. 2100 PQ.</summary>
        Pq2100
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
        : base(SourceTypeId, ValidatePlatform(name))
    {
        ApplySettings(mode, null, captureCursor);
        SubscribeToHookSignals();
    }

    private static string ValidatePlatform(string name)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Game capture is only supported on Windows.");

        return name;
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
    /// <param name="windowId">The window identifier (format: WindowTitle:WindowClass:ProcessName.exe, each part encoded). Build it with <see cref="ObsKit.NET.Platform.WindowSpec"/> rather than concatenating by hand.</param>
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
    /// <param name="windowId">The window identifier (format: WindowTitle:WindowClass:ProcessName.exe, each part encoded). Build it with <see cref="ObsKit.NET.Platform.WindowSpec"/> rather than concatenating by hand.</param>
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
    /// Sets whether to also capture the hooked game's audio (requires Windows 10 2004+).
    /// OBS internally creates an application audio capture bound to the hooked process
    /// and mixes it into this source's audio output — no separate
    /// <see cref="ApplicationAudioCapture"/> source is needed.
    /// </summary>
    /// <param name="capture">Whether to capture the game's audio.</param>
    public GameCapture SetCaptureAudio(bool capture = true)
    {
        Update(s => s.Set("capture_audio", capture));
        return this;
    }

    /// <summary>
    /// Sets whether to capture third-party overlays drawn on top of the game
    /// (e.g. Steam or Discord overlays).
    /// </summary>
    /// <param name="capture">Whether to capture overlays.</param>
    public GameCapture SetCaptureOverlays(bool capture = true)
    {
        Update(s => s.Set("capture_overlays", capture));
        return this;
    }

    /// <summary>
    /// Sets how often game capture attempts to hook newly started games.
    /// </summary>
    /// <param name="rate">The hook check rate.</param>
    public GameCapture SetHookRate(HookRate rate)
    {
        Update(s => s.Set("hook_rate", (long)rate));
        return this;
    }

    /// <summary>
    /// Sets how 10-bit (RGB10A2) game output is interpreted. Use
    /// <see cref="Rgb10A2ColorSpace.Pq2100"/> when capturing HDR games.
    /// </summary>
    /// <param name="colorSpace">The color space of RGB10A2 frames.</param>
    public GameCapture SetRgb10A2ColorSpace(Rgb10A2ColorSpace colorSpace)
    {
        Update(s => s.Set("rgb10a2_space", colorSpace == Rgb10A2ColorSpace.Pq2100 ? "2100pq" : "srgb"));
        return this;
    }

    /// <summary>
    /// In <see cref="CaptureMode.HotkeyForeground"/> mode, starts capturing the current
    /// foreground window (equivalent to pressing the OBS "Capture foreground window" hotkey).
    /// </summary>
    /// <returns>True if the hotkey was found and triggered.</returns>
    public bool CaptureForegroundWindow() => Obs.TriggerHotkey("hotkey_start", this);

    /// <summary>
    /// In <see cref="CaptureMode.HotkeyForeground"/> mode, deactivates the current capture.
    /// </summary>
    /// <returns>True if the hotkey was found and triggered.</returns>
    public bool DeactivateCapture() => Obs.TriggerHotkey("hotkey_stop", this);

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
