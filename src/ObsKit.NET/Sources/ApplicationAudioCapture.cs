using ObsKit.NET.Native.Interop;
using ObsKit.NET.Platform;
using ObsKit.NET.Signals;

namespace ObsKit.NET.Sources;

/// <summary>
/// Captures audio from a specific application (Windows only).
/// Uses WASAPI process loopback to capture audio from the target window's
/// process tree, independent of which output device the application uses.
/// Requires Windows 10 2004 or later.
/// Automatically tracks hook state via the IsHooked property.
/// </summary>
public sealed class ApplicationAudioCapture : Source
{
    private SignalConnection? _hookedConnection;
    private SignalConnection? _unhookedConnection;

    /// <summary>
    /// The source type ID for application audio capture.
    /// </summary>
    public const string SourceTypeId = "wasapi_process_output_capture";

    /// <summary>
    /// How the target window is matched when the original window is gone.
    /// </summary>
    public enum WindowPriority
    {
        /// <summary>Match windows by class name first.</summary>
        WindowClass = 0,

        /// <summary>Match windows by title first.</summary>
        WindowTitle = 1,

        /// <summary>Match windows by executable name first (recommended).</summary>
        Executable = 2
    }

    /// <summary>
    /// Gets whether the capture has hooked the target application's audio.
    /// </summary>
    public bool IsHooked { get; private set; }

    /// <summary>
    /// Gets the window title of the hooked application, or null if not hooked.
    /// </summary>
    public string? HookedWindowTitle { get; private set; }

    /// <summary>
    /// Gets the window class of the hooked application, or null if not hooked.
    /// </summary>
    public string? HookedWindowClass { get; private set; }

    /// <summary>
    /// Gets the executable name of the hooked application, or null if not hooked.
    /// </summary>
    public string? HookedExecutable { get; private set; }

    /// <summary>
    /// Event raised when the capture hooks the target application's audio.
    /// </summary>
    public event Action<ApplicationAudioCapture>? Hooked;

    /// <summary>
    /// Event raised when the capture loses the target application.
    /// </summary>
    public event Action<ApplicationAudioCapture>? Unhooked;

    /// <summary>
    /// Creates an application audio capture source.
    /// </summary>
    /// <param name="name">The source name.</param>
    public ApplicationAudioCapture(string name)
        : base(SourceTypeId, ValidatePlatform(name))
    {
        SubscribeToHookSignals();
    }

    private static string ValidatePlatform(string name)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Application audio capture is only supported on Windows.");

        return name;
    }

    /// <summary>
    /// Creates an application audio capture source targeting a specific window.
    /// </summary>
    /// <param name="window">The window whose process audio should be captured.</param>
    /// <param name="name">Optional source name.</param>
    /// <param name="priority">How the window is matched if it is recreated.</param>
    /// <returns>An application audio capture source.</returns>
    public static ApplicationAudioCapture FromWindow(WindowInfo window, string? name = null,
        WindowPriority priority = WindowPriority.Executable)
    {
        var capture = new ApplicationAudioCapture(name ?? $"{window.ProcessName} Audio");
        capture.SetWindow(window.Title, window.ClassName, window.ProcessName, priority);
        return capture;
    }

    /// <summary>
    /// Creates an application audio capture source targeting any window of an executable.
    /// </summary>
    /// <param name="executable">The executable file name, e.g. "Discord.exe".</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>An application audio capture source.</returns>
    public static ApplicationAudioCapture FromExecutable(string executable, string? name = null)
    {
        var capture = new ApplicationAudioCapture(name ?? $"{executable} Audio");
        capture.SetWindow(string.Empty, string.Empty, executable, WindowPriority.Executable);
        return capture;
    }

    /// <summary>
    /// Sets the target window by its parts. Parts are encoded per OBS rules.
    /// </summary>
    /// <param name="title">The window title (may be empty).</param>
    /// <param name="windowClass">The window class name (may be empty).</param>
    /// <param name="executable">The executable file name (may be empty).</param>
    /// <param name="priority">How the window is matched if it is recreated.</param>
    public ApplicationAudioCapture SetWindow(string title, string windowClass, string executable,
        WindowPriority priority = WindowPriority.Executable)
    {
        return SetWindow(WindowSpec.Build(title, windowClass, executable), priority);
    }

    /// <summary>
    /// Sets the target window using a raw OBS window spec ("Title:Class:Executable").
    /// </summary>
    /// <param name="windowSpec">The encoded window specification.</param>
    /// <param name="priority">How the window is matched if it is recreated.</param>
    public ApplicationAudioCapture SetWindow(string windowSpec, WindowPriority priority = WindowPriority.Executable)
    {
        Update(s => s
            .Set("window", windowSpec)
            .Set("priority", (long)priority));
        return this;
    }

    /// <summary>
    /// Sets how the target window is matched if it is recreated.
    /// </summary>
    /// <param name="priority">The window matching priority.</param>
    public ApplicationAudioCapture SetPriority(WindowPriority priority)
    {
        Update(s => s.Set("priority", (long)priority));
        return this;
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
    /// Disposes the capture and disconnects signal handlers.
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
