using System.Runtime.InteropServices;
using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Services;

/// <summary>
/// Represents an OBS streaming service (obs_service_t).
/// Services configure streaming destinations such as Twitch, YouTube, or custom RTMP servers.
/// </summary>
public class Service : ObsObject
{
    /// <summary>Known service type IDs.</summary>
    public static class Types
    {
        /// <summary>Common RTMP services (Twitch, YouTube, Facebook, etc.).</summary>
        public const string RtmpCommon = "rtmp_common";

        /// <summary>Custom RTMP server configuration.</summary>
        public const string RtmpCustom = "rtmp_custom";
    }

    /// <summary>
    /// Creates a new streaming service.
    /// </summary>
    /// <param name="typeId">The service type ID (e.g., "rtmp_common", "rtmp_custom").</param>
    /// <param name="name">The display name for this service instance.</param>
    /// <param name="settings">Optional settings for the service.</param>
    /// <param name="hotkeyData">Optional hotkey data.</param>
    public Service(string typeId, string name, Settings? settings = null, Settings? hotkeyData = null)
        : base(CreateService(typeId, name, settings, hotkeyData))
    {
        TypeId = typeId;
    }

    /// <summary>
    /// Internal constructor for wrapping an existing service handle.
    /// </summary>
    internal Service(ObsServiceHandle handle, string? typeId = null, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        TypeId = typeId ?? ObsService.obs_service_get_type(handle);
    }

    private static nint CreateService(string typeId, string name, Settings? settings, Settings? hotkeyData)
    {
        ThrowIfNotInitialized();
        var handle = ObsService.obs_service_create(
            typeId,
            name,
            settings?.Handle ?? default,
            hotkeyData?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create service of type '{typeId}'");

        return handle;
    }

    /// <summary>
    /// Creates a custom RTMP service for streaming to a custom server.
    /// </summary>
    /// <param name="serverUrl">The RTMP server URL (e.g., "rtmp://live.example.com/app").</param>
    /// <param name="streamKey">The stream key for authentication.</param>
    /// <param name="name">Optional display name for the service.</param>
    /// <param name="username">Optional username for authentication.</param>
    /// <param name="password">Optional password for authentication.</param>
    /// <returns>A configured custom RTMP service.</returns>
    public static Service CreateCustom(
        string serverUrl,
        string streamKey,
        string name = "Custom Streaming Service",
        string? username = null,
        string? password = null)
    {
        using var settings = new Settings();
        settings.Set("server", serverUrl);
        settings.Set("key", streamKey);

        if (!string.IsNullOrEmpty(username))
            settings.Set("username", username);
        if (!string.IsNullOrEmpty(password))
            settings.Set("password", password);

        return new Service(Types.RtmpCustom, name, settings);
    }

    /// <summary>
    /// Creates a Twitch streaming service.
    /// </summary>
    /// <param name="streamKey">Your Twitch stream key.</param>
    /// <param name="server">Server URL or "auto" for automatic selection.</param>
    /// <param name="name">Optional display name for the service.</param>
    /// <returns>A configured Twitch service.</returns>
    public static Service CreateTwitch(string streamKey, string server = "auto", string name = "Twitch")
    {
        using var settings = new Settings();
        settings.Set("service", "Twitch");
        settings.Set("server", server);
        settings.Set("key", streamKey);

        return new Service(Types.RtmpCommon, name, settings);
    }

    /// <summary>
    /// Creates a YouTube streaming service.
    /// </summary>
    /// <param name="streamKey">Your YouTube stream key.</param>
    /// <param name="server">Server URL or use default YouTube ingest.</param>
    /// <param name="name">Optional display name for the service.</param>
    /// <returns>A configured YouTube service.</returns>
    public static Service CreateYouTube(string streamKey, string? server = null, string name = "YouTube")
    {
        using var settings = new Settings();
        settings.Set("service", "YouTube - RTMPS");
        settings.Set("server", server ?? "rtmps://a.rtmps.youtube.com:443/live2");
        settings.Set("key", streamKey);

        return new Service(Types.RtmpCommon, name, settings);
    }

    /// <summary>
    /// Creates a Facebook Live streaming service.
    /// </summary>
    /// <param name="streamKey">Your Facebook stream key.</param>
    /// <param name="server">Server URL or use default Facebook ingest.</param>
    /// <param name="name">Optional display name for the service.</param>
    /// <returns>A configured Facebook Live service.</returns>
    public static Service CreateFacebook(string streamKey, string? server = null, string name = "Facebook Live")
    {
        using var settings = new Settings();
        settings.Set("service", "Facebook Live");
        settings.Set("server", server ?? "rtmps://live-api-s.facebook.com:443/rtmp/");
        settings.Set("key", streamKey);

        return new Service(Types.RtmpCommon, name, settings);
    }

    /// <summary>
    /// Gets the internal handle for P/Invoke calls.
    /// </summary>
    internal new ObsServiceHandle Handle => (ObsServiceHandle)base.Handle;

    /// <summary>
    /// Gets the service type ID.
    /// </summary>
    public string? TypeId { get; }

    /// <summary>
    /// Gets the service name.
    /// </summary>
    public string? Name => ObsService.obs_service_get_name(Handle);

    /// <summary>
    /// Gets the display name for this service type.
    /// </summary>
    public string? DisplayName => TypeId != null ? ObsService.obs_service_get_display_name(TypeId) : null;

    /// <summary>
    /// Gets the protocol used by the service (e.g., "RTMP", "RTMPS", "SRT").
    /// </summary>
    public string? Protocol => ObsService.obs_service_get_protocol(Handle);

    /// <summary>
    /// Gets the output type required by this service.
    /// </summary>
    public string? OutputType => ObsService.obs_service_get_output_type(Handle);

    /// <summary>
    /// Gets whether the service has sufficient information to attempt a connection.
    /// </summary>
    public bool CanConnect => ObsService.obs_service_can_try_to_connect(Handle);

    /// <summary>
    /// Gets the server URL configured for this service.
    /// </summary>
    public string? Url => ObsService.obs_service_get_connect_info(Handle, ServiceConnectInfo.ServerUrl)
                          ?? ObsService.obs_service_get_url(Handle);

    /// <summary>
    /// Gets the stream key configured for this service.
    /// </summary>
    public string? StreamKey => ObsService.obs_service_get_connect_info(Handle, ServiceConnectInfo.StreamKey)
                                ?? ObsService.obs_service_get_key(Handle);

    /// <summary>
    /// Gets the username configured for this service (if any).
    /// </summary>
    public string? Username => ObsService.obs_service_get_connect_info(Handle, ServiceConnectInfo.Username)
                               ?? ObsService.obs_service_get_username(Handle);

    /// <summary>
    /// Gets the password configured for this service (if any).
    /// </summary>
    public string? Password => ObsService.obs_service_get_connect_info(Handle, ServiceConnectInfo.Password)
                               ?? ObsService.obs_service_get_password(Handle);

    /// <summary>
    /// Gets the maximum video bitrate supported by the service (in kbps).
    /// Returns 0 if no limit is specified.
    /// </summary>
    public int MaxVideoBitrate
    {
        get
        {
            ObsService.obs_service_get_max_bitrate(Handle, out var video, out _);
            return video;
        }
    }

    /// <summary>
    /// Gets the maximum audio bitrate supported by the service (in kbps).
    /// Returns 0 if no limit is specified.
    /// </summary>
    public int MaxAudioBitrate
    {
        get
        {
            ObsService.obs_service_get_max_bitrate(Handle, out _, out var audio);
            return audio;
        }
    }

    /// <summary>
    /// Gets the maximum FPS supported by the service.
    /// Returns 0 if no limit is specified.
    /// </summary>
    public int MaxFps
    {
        get
        {
            ObsService.obs_service_get_max_fps(Handle, out var fps);
            return fps;
        }
    }

    /// <summary>
    /// Gets the supported video codecs for the service.
    /// </summary>
    public string[] SupportedVideoCodecs => GetCodecArray(ObsService.obs_service_get_supported_video_codecs(Handle));

    /// <summary>
    /// Gets the supported audio codecs for the service.
    /// </summary>
    public string[] SupportedAudioCodecs => GetCodecArray(ObsService.obs_service_get_supported_audio_codecs(Handle));

    private static string[] GetCodecArray(nint ptr)
    {
        if (ptr == 0)
            return [];

        var codecs = new List<string>();
        var offset = 0;

        while (true)
        {
            var codecPtr = Marshal.ReadIntPtr(ptr, offset);
            if (codecPtr == 0)
                break;

            var codec = Marshal.PtrToStringUTF8(codecPtr);
            if (!string.IsNullOrEmpty(codec))
                codecs.Add(codec);

            offset += nint.Size;
        }

        return [.. codecs];
    }

    #region Settings

    /// <summary>
    /// Gets the current settings for this service.
    /// </summary>
    public Settings GetSettings()
    {
        var handle = ObsService.obs_service_get_settings(Handle);
        return new Settings(handle, ownsHandle: true);
    }

    /// <summary>
    /// Updates the service with new settings.
    /// </summary>
    /// <param name="settings">The settings to apply.</param>
    public void Update(Settings settings)
    {
        ObsService.obs_service_update(Handle, settings.Handle);
    }

    /// <summary>
    /// Updates the service with settings configured via a builder action.
    /// </summary>
    /// <param name="configure">Action to configure the settings.</param>
    public void Update(Action<Settings> configure)
    {
        using var settings = new Settings();
        configure(settings);
        Update(settings);
    }

    /// <summary>
    /// Sets the server URL for this service.
    /// </summary>
    /// <param name="url">The server URL.</param>
    /// <returns>This service for method chaining.</returns>
    public Service SetServer(string url)
    {
        Update(s => s.Set("server", url));
        return this;
    }

    /// <summary>
    /// Sets the stream key for this service.
    /// </summary>
    /// <param name="key">The stream key.</param>
    /// <returns>This service for method chaining.</returns>
    public Service SetStreamKey(string key)
    {
        Update(s => s.Set("key", key));
        return this;
    }

    /// <summary>
    /// Sets the username for this service.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>This service for method chaining.</returns>
    public Service SetUsername(string username)
    {
        Update(s => s.Set("username", username));
        return this;
    }

    /// <summary>
    /// Sets the password for this service.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>This service for method chaining.</returns>
    public Service SetPassword(string password)
    {
        Update(s => s.Set("password", password));
        return this;
    }

    #endregion

    #region Encoder Settings

    /// <summary>
    /// Applies service-recommended settings to the given encoder settings.
    /// This can help ensure compatibility with the streaming service.
    /// </summary>
    /// <param name="videoEncoderSettings">Video encoder settings to modify.</param>
    /// <param name="audioEncoderSettings">Audio encoder settings to modify.</param>
    public void ApplyEncoderSettings(Settings? videoEncoderSettings, Settings? audioEncoderSettings)
    {
        ObsService.obs_service_apply_encoder_settings(
            Handle,
            videoEncoderSettings?.Handle ?? default,
            audioEncoderSettings?.Handle ?? default);
    }

    #endregion

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsService.obs_service_release((ObsServiceHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Service[{TypeId}]: {Name} -> {Url}";
}
