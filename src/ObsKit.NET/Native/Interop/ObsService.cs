using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS service functions.
/// </summary>
internal static partial class ObsService
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Creation and Release

    /// <summary>
    /// Creates a new service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsServiceHandle obs_service_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings,
        ObsDataHandle hotkeyData);

    /// <summary>
    /// Creates a private service (not added to global list).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_create_private")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsServiceHandle obs_service_create_private(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings);

    /// <summary>
    /// Releases a reference to a service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_service_release(ObsServiceHandle service);

    /// <summary>
    /// Adds a reference to a service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_service_addref(ObsServiceHandle service);

    /// <summary>
    /// Gets an additional reference to a service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsServiceHandle obs_service_get_ref(ObsServiceHandle service);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the service name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_name(ObsServiceHandle service);

    /// <summary>
    /// Gets the service type ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_type(ObsServiceHandle service);

    /// <summary>
    /// Gets the service ID (same as type).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_id(ObsServiceHandle service);

    /// <summary>
    /// Gets the display name for a service type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_display_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_display_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the protocol used by the service (e.g., "RTMP", "RTMPS", "SRT").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_protocol")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_protocol(ObsServiceHandle service);

    /// <summary>
    /// Gets the output type required by the service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_output_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_output_type(ObsServiceHandle service);

    #endregion

    #region Connection Info

    /// <summary>
    /// Gets connection information from the service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_connect_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_connect_info(ObsServiceHandle service, uint type);

    /// <summary>
    /// Gets the URL from the service (legacy, prefer get_connect_info).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_url")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_url(ObsServiceHandle service);

    /// <summary>
    /// Gets the stream key from the service (legacy, prefer get_connect_info).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_key")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_key(ObsServiceHandle service);

    /// <summary>
    /// Gets the username from the service (legacy, prefer get_connect_info).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_username")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_username(ObsServiceHandle service);

    /// <summary>
    /// Gets the password from the service (legacy, prefer get_connect_info).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_password")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_service_get_password(ObsServiceHandle service);

    /// <summary>
    /// Checks if the service can attempt to connect.
    /// </summary>
    public static bool obs_service_can_try_to_connect(ObsServiceHandle service)
        => obs_service_can_try_to_connect_native(service) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_service_can_try_to_connect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_service_can_try_to_connect_native(ObsServiceHandle service);

    #endregion

    #region Settings

    /// <summary>
    /// Gets the service settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_service_get_settings(ObsServiceHandle service);

    /// <summary>
    /// Updates the service settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_update")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_service_update(ObsServiceHandle service, ObsDataHandle settings);

    /// <summary>
    /// Gets the default settings for a service type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_defaults")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_service_defaults(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    #endregion

    #region Encoder Settings

    /// <summary>
    /// Applies service-recommended encoder settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_apply_encoder_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_service_apply_encoder_settings(
        ObsServiceHandle service,
        ObsDataHandle videoEncoderSettings,
        ObsDataHandle audioEncoderSettings);

    #endregion

    #region Bitrate and Resolution Limits

    /// <summary>
    /// Gets the maximum video bitrate supported by the service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_max_bitrate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_service_get_max_bitrate(
        ObsServiceHandle service,
        out int videoBitrate,
        out int audioBitrate);

    /// <summary>
    /// Gets the maximum FPS supported by the service.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_max_fps")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_service_get_max_fps(ObsServiceHandle service, out int fps);

    #endregion

    #region Supported Codecs

    /// <summary>
    /// Gets the supported video codecs for the service.
    /// Returns a null-terminated array of codec strings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_supported_video_codecs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_service_get_supported_video_codecs(ObsServiceHandle service);

    /// <summary>
    /// Gets the supported audio codecs for the service.
    /// Returns a null-terminated array of codec strings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_service_get_supported_audio_codecs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_service_get_supported_audio_codecs(ObsServiceHandle service);

    #endregion
}

/// <summary>
/// Connection info types for obs_service_get_connect_info.
/// </summary>
internal static class ServiceConnectInfo
{
    public const uint ServerUrl = 0;
    public const uint StreamId = 2;
    public const uint StreamKey = 2; // Alias for StreamId
    public const uint Username = 4;
    public const uint Password = 6;
    public const uint EncryptPassphrase = 8;
    public const uint BearerToken = 10;
}
