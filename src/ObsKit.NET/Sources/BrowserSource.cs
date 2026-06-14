using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Sources;

/// <summary>
/// Renders a web page (browser overlay) using the obs-browser plugin (CEF).
/// Useful for HTML/JS overlays, alerts, and animated widgets composited into
/// recordings or streams.
/// </summary>
/// <remarks>
/// Requires the obs-browser plugin to be present in the OBS runtime;
/// check <see cref="IsAvailable"/> before creating instances.
/// </remarks>
public sealed class BrowserSource : Source
{
    /// <summary>
    /// The source type ID for browser sources.
    /// </summary>
    public const string SourceTypeId = "browser_source";

    /// <summary>
    /// Gets whether the obs-browser plugin is loaded and browser sources can be created.
    /// </summary>
    public static bool IsAvailable() => Obs.EnumerateInputTypes().Contains(SourceTypeId);

    /// <summary>
    /// Creates a browser source showing a web page.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="url">The page URL (e.g. "https://example.com/overlay").</param>
    /// <param name="width">The page width in pixels.</param>
    /// <param name="height">The page height in pixels.</param>
    /// <exception cref="NotSupportedException">The obs-browser plugin is not loaded.</exception>
    public BrowserSource(string name, string url, int width = 800, int height = 600)
        : base(Create(ValidatePlugin(name), url, width, height), SourceTypeId, ownsHandle: true)
    {
    }

    private static ObsSourceHandle Create(string name, string url, int width, int height)
    {
        ThrowIfNotInitialized();
        // obs_source_create takes its own reference to the settings (obs_data_addref), so we must
        // dispose our create-time reference; otherwise it leaks until finalization.
        using var settings = BuildInitialSettings(url, width, height);
        var handle = ObsSource.obs_source_create(SourceTypeId, name, settings.Handle, default);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create source of type '{SourceTypeId}'");
        return handle;
    }

    private static string ValidatePlugin(string name)
    {
        if (!IsAvailable())
            throw new NotSupportedException(
                "The obs-browser plugin is not loaded. Ensure obs-browser is present in the OBS runtime's obs-plugins directory.");

        return name;
    }

    private static Settings BuildInitialSettings(string url, int width, int height)
    {
        var settings = new Settings();
        settings.Set("url", url);
        settings.Set("width", width);
        settings.Set("height", height);
        return settings;
    }

    /// <summary>
    /// Creates a browser source showing a local HTML file.
    /// </summary>
    /// <param name="path">The path to the local HTML file.</param>
    /// <param name="width">The page width in pixels.</param>
    /// <param name="height">The page height in pixels.</param>
    /// <param name="name">Optional source name.</param>
    public static BrowserSource FromFile(string path, int width = 800, int height = 600, string? name = null)
    {
        var source = new BrowserSource(name ?? "Browser", string.Empty, width, height);
        source.SetLocalFile(path);
        return source;
    }

    /// <summary>
    /// Sets the URL to display and switches to URL mode.
    /// </summary>
    /// <param name="url">The page URL.</param>
    public BrowserSource SetUrl(string url)
    {
        Update(s => s
            .Set("is_local_file", false)
            .Set("url", url));
        return this;
    }

    /// <summary>
    /// Sets a local HTML file to display and switches to local-file mode.
    /// </summary>
    /// <param name="path">The path to the local HTML file.</param>
    public BrowserSource SetLocalFile(string path)
    {
        Update(s => s
            .Set("is_local_file", true)
            .Set("local_file", path));
        return this;
    }

    /// <summary>
    /// Sets the page size in pixels.
    /// </summary>
    /// <param name="width">The page width.</param>
    /// <param name="height">The page height.</param>
    public BrowserSource SetSize(int width, int height)
    {
        Update(s => s
            .Set("width", width)
            .Set("height", height));
        return this;
    }

    /// <summary>
    /// Renders the page at a fixed frame rate instead of the canvas frame rate.
    /// </summary>
    /// <param name="fps">The frame rate (OBS default is 30).</param>
    public BrowserSource SetCustomFrameRate(int fps)
    {
        Update(s => s
            .Set("fps_custom", true)
            .Set("fps", fps));
        return this;
    }

    /// <summary>
    /// Renders the page at the canvas frame rate (default).
    /// </summary>
    public BrowserSource UseCanvasFrameRate()
    {
        Update(s => s.Set("fps_custom", false));
        return this;
    }

    /// <summary>
    /// Sets custom CSS injected into the page. OBS's default makes the page
    /// background transparent with zero margins.
    /// </summary>
    /// <param name="css">The CSS to inject.</param>
    public BrowserSource SetCss(string css)
    {
        Update(s => s.Set("css", css));
        return this;
    }

    /// <summary>
    /// Routes the page's audio through this source (so it gets volume, mute, and
    /// track routing) instead of playing directly on the desktop.
    /// </summary>
    /// <param name="reroute">Whether to control the page's audio via this source.</param>
    public BrowserSource SetRerouteAudio(bool reroute = true)
    {
        Update(s => s.Set("reroute_audio", reroute));
        return this;
    }

    /// <summary>
    /// Sets whether the browser shuts down when the source is hidden (saves memory,
    /// but reloads the page on show).
    /// </summary>
    /// <param name="shutdown">Whether to shut down when hidden.</param>
    public BrowserSource SetShutdownWhenHidden(bool shutdown)
    {
        Update(s => s.Set("shutdown", shutdown));
        return this;
    }

    /// <summary>
    /// Sets whether the page refreshes when the source becomes active
    /// (requires shutdown-when-hidden).
    /// </summary>
    /// <param name="restart">Whether to refresh on activation.</param>
    public BrowserSource SetRestartWhenActive(bool restart)
    {
        Update(s => s.Set("restart_when_active", restart));
        return this;
    }

    /// <summary>
    /// Reloads the page, bypassing the cache.
    /// </summary>
    public void Refresh()
    {
        var props = ObsProperties.obs_source_properties(Handle);
        if (props == 0)
            return;

        try
        {
            var button = ObsProperties.obs_properties_get(props, "refreshnocache");
            if (button != 0)
                ObsProperties.obs_property_button_clicked(button, Handle);
        }
        finally
        {
            ObsProperties.obs_properties_destroy(props);
        }
    }

    /// <summary>
    /// Dispatches a custom JavaScript event to the page. The page can listen via
    /// <c>window.addEventListener(eventName, e => ... e.detail ...)</c>.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="jsonData">JSON for the event's <c>detail</c> payload (e.g. "{\"kills\":3}").</param>
    public void SendJavascriptEvent(string eventName, string jsonData = "{}")
    {
        var procHandler = ObsSource.obs_source_get_proc_handler(Handle);
        if (procHandler == 0)
            return;

        var calldata = ObsSignal.calldata_create();
        try
        {
            ObsSignal.calldata_set_string(calldata, "eventName", eventName);
            ObsSignal.calldata_set_string(calldata, "jsonString", jsonData);
            ObsSignal.proc_handler_call(procHandler, "javascript_event", calldata);
        }
        finally
        {
            ObsSignal.calldata_destroy(calldata);
        }
    }
}
