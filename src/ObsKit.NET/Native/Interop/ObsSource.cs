using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS source functions.
/// </summary>
internal static partial class ObsSource
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Creation and Release

    /// <summary>
    /// Creates a new source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_source_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings,
        ObsDataHandle hotkeyData);

    /// <summary>
    /// Creates a private source (not saved with scenes).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_create_private")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_source_create_private(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings);

    /// <summary>
    /// Releases a reference to a source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_release(ObsSourceHandle source);

    /// <summary>
    /// Returns an owning reference to the source (the same handle), or null if the source is
    /// being destroyed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_source_get_ref(ObsSourceHandle source);

    /// <summary>
    /// Removes a source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_remove(ObsSourceHandle source);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the source name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_source_get_name(ObsSourceHandle source);

    /// <summary>
    /// Gets the source UUID (stable for the lifetime of the source).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_uuid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_source_get_uuid(ObsSourceHandle source);

    /// <summary>
    /// Finds a source by UUID (incremented reference).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_source_by_uuid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_get_source_by_uuid(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string uuid);

    /// <summary>
    /// Sets the source name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_name(
        ObsSourceHandle source,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the source type ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_source_get_id(ObsSourceHandle source);

    /// <summary>
    /// Gets the source display name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_display_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_source_get_display_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the source width.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_width")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_width(ObsSourceHandle source);

    /// <summary>
    /// Gets the source height.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_height")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_height(ObsSourceHandle source);

    #endregion

    #region Settings

    /// <summary>
    /// Gets the source settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_source_get_settings(ObsSourceHandle source);

    /// <summary>
    /// Updates the source settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_update")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_update(ObsSourceHandle source, ObsDataHandle settings);

    /// <summary>
    /// Gets default settings for a source type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_source_defaults")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_get_source_defaults(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    #endregion

    #region Audio

    /// <summary>
    /// Gets the source volume.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_volume")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_source_get_volume(ObsSourceHandle source);

    /// <summary>
    /// Sets the source volume.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_volume")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_volume(ObsSourceHandle source, float volume);

    /// <summary>
    /// Gets the audio mixers for the source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_audio_mixers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_audio_mixers(ObsSourceHandle source);

    /// <summary>
    /// Sets the audio mixers for the source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_audio_mixers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_audio_mixers(ObsSourceHandle source, uint mixers);

    /// <summary>
    /// Gets the audio sync offset in nanoseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_sync_offset")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long obs_source_get_sync_offset(ObsSourceHandle source);

    /// <summary>
    /// Sets the audio sync offset in nanoseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_sync_offset")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_sync_offset(ObsSourceHandle source, long offset);

    /// <summary>
    /// Gets the stereo balance value (0.0 = left, 0.5 = center, 1.0 = right).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_balance_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_source_get_balance_value(ObsSourceHandle source);

    /// <summary>
    /// Sets the stereo balance value (0.0 = left, 0.5 = center, 1.0 = right).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_balance_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_balance_value(ObsSourceHandle source, float balance);

    /// <summary>
    /// Checks if audio is muted.
    /// </summary>
    public static bool obs_source_muted(ObsSourceHandle source) => obs_source_muted_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_muted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_muted_native(ObsSourceHandle source);

    /// <summary>
    /// Sets mute state.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_muted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_muted(ObsSourceHandle source, byte muted);

    // ---- Push-to-talk / push-to-mute ----

    public static bool obs_source_push_to_mute_enabled(ObsSourceHandle source) => obs_source_push_to_mute_enabled_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_push_to_mute_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_push_to_mute_enabled_native(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_enable_push_to_mute")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_enable_push_to_mute(ObsSourceHandle source, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(Lib, EntryPoint = "obs_source_get_push_to_mute_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_source_get_push_to_mute_delay(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_set_push_to_mute_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_push_to_mute_delay(ObsSourceHandle source, ulong delayMs);

    public static bool obs_source_push_to_talk_enabled(ObsSourceHandle source) => obs_source_push_to_talk_enabled_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_push_to_talk_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_push_to_talk_enabled_native(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_enable_push_to_talk")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_enable_push_to_talk(ObsSourceHandle source, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(Lib, EntryPoint = "obs_source_get_push_to_talk_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_source_get_push_to_talk_delay(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_set_push_to_talk_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_push_to_talk_delay(ObsSourceHandle source, ulong delayMs);

    /// <summary>Gets the source category (input, filter, transition, or scene).</summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceType obs_source_get_type(ObsSourceHandle source);

    // ---- Async source latency tuning ----

    public static bool obs_source_async_unbuffered(ObsSourceHandle source) => obs_source_async_unbuffered_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_async_unbuffered")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_async_unbuffered_native(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_set_async_unbuffered")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_async_unbuffered(ObsSourceHandle source, [MarshalAs(UnmanagedType.U1)] bool unbuffered);

    public static bool obs_source_async_decoupled(ObsSourceHandle source) => obs_source_async_decoupled_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_async_decoupled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_async_decoupled_native(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_set_async_decoupled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_async_decoupled(ObsSourceHandle source, [MarshalAs(UnmanagedType.U1)] bool decouple);

    /// <summary>For a filter source, gets the source it is directly attached to (borrowed pointer, not referenced).</summary>
    [LibraryImport(Lib, EntryPoint = "obs_filter_get_parent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_filter_get_parent(ObsSourceHandle filter);

    /// <summary>For a filter source, gets the next target down the filter chain (borrowed pointer, not referenced).</summary>
    [LibraryImport(Lib, EntryPoint = "obs_filter_get_target")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_filter_get_target(ObsSourceHandle filter);

    /// <summary>
    /// Sends a mouse button event to an interactive source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_send_mouse_click")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_send_mouse_click(ObsSourceHandle source,
        ref ObsMouseEventNative mouseEvent, int type, byte mouseUp, uint clickCount);

    /// <summary>
    /// Sends a mouse move event to an interactive source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_send_mouse_move")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_send_mouse_move(ObsSourceHandle source,
        ref ObsMouseEventNative mouseEvent, byte mouseLeave);

    /// <summary>
    /// Sends a mouse wheel event to an interactive source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_send_mouse_wheel")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_send_mouse_wheel(ObsSourceHandle source,
        ref ObsMouseEventNative mouseEvent, int xDelta, int yDelta);

    /// <summary>
    /// Sends a focus or unfocus event to an interactive source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_send_focus")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_send_focus(ObsSourceHandle source, byte focus);

    /// <summary>
    /// Sends a key event to an interactive source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_send_key_click")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_send_key_click(ObsSourceHandle source,
        ref ObsKeyEventNative keyEvent, byte keyUp);

    /// <summary>
    /// Native callback for per-source audio capture
    /// (<c>obs_source_audio_capture_t</c>: param, source, audio_data, muted).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SourceAudioCaptureCallback(nint param, ObsSourceHandle source, nint audioData, byte muted);

    /// <summary>
    /// Adds a callback receiving the source's audio before mixing.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_add_audio_capture_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_add_audio_capture_callback(ObsSourceHandle source,
        SourceAudioCaptureCallback callback, nint param);

    /// <summary>
    /// Removes a previously added audio capture callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_remove_audio_capture_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_remove_audio_capture_callback(ObsSourceHandle source,
        SourceAudioCaptureCallback callback, nint param);

    /// <summary>
    /// Checks if the source is enabled (mainly used to bypass filters).
    /// </summary>
    public static bool obs_source_enabled(ObsSourceHandle source) => obs_source_enabled_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_enabled_native(ObsSourceHandle source);

    /// <summary>
    /// Enables or disables the source (mainly used to bypass filters).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_enabled(ObsSourceHandle source, byte enabled);

    /// <summary>
    /// Gets the source's procedure handler.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_proc_handler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ProcHandlerHandle obs_source_get_proc_handler(ObsSourceHandle source);

    /// <summary>
    /// Sets the deinterlacing mode.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_deinterlace_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_deinterlace_mode(ObsSourceHandle source, ObsDeinterlaceMode mode);

    /// <summary>
    /// Gets the deinterlacing mode.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_deinterlace_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDeinterlaceMode obs_source_get_deinterlace_mode(ObsSourceHandle source);

    /// <summary>
    /// Sets the deinterlacing field order.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_deinterlace_field_order")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_deinterlace_field_order(ObsSourceHandle source, ObsDeinterlaceFieldOrder fieldOrder);

    /// <summary>
    /// Gets the deinterlacing field order.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_deinterlace_field_order")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDeinterlaceFieldOrder obs_source_get_deinterlace_field_order(ObsSourceHandle source);

    /// <summary>
    /// Gets the audio monitoring type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_monitoring_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsMonitoringType obs_source_get_monitoring_type(ObsSourceHandle source);

    /// <summary>
    /// Sets the audio monitoring type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_monitoring_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_monitoring_type(ObsSourceHandle source, ObsMonitoringType type);

    #endregion

    #region Flags

    /// <summary>
    /// Gets source flags.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_flags(ObsSourceHandle source);

    /// <summary>
    /// Sets source flags.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_flags(ObsSourceHandle source, uint flags);

    /// <summary>
    /// Gets output flags for a source type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_source_output_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_get_source_output_flags(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    #endregion

    #region Filters

    /// <summary>
    /// Adds a filter to the source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_filter_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_filter_add(ObsSourceHandle source, ObsSourceHandle filter);

    /// <summary>
    /// Removes a filter from the source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_filter_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_filter_remove(ObsSourceHandle source, ObsSourceHandle filter);

    /// <summary>
    /// Copies all filters from <paramref name="src"/> onto <paramref name="dst"/>.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_copy_filters")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_copy_filters(ObsSourceHandle dst, ObsSourceHandle src);

    /// <summary>
    /// Copies a single existing filter onto <paramref name="dst"/>.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_copy_single_filter")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_copy_single_filter(ObsSourceHandle dst, ObsSourceHandle filter);

    /// <summary>Gets the zero-based index of a filter in the source's filter chain (-1 if not found).</summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_filter_get_index")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_source_filter_get_index(ObsSourceHandle source, ObsSourceHandle filter);

    /// <summary>Moves a filter to an absolute zero-based index in the source's filter chain.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_filter_set_index")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_filter_set_index(ObsSourceHandle source, ObsSourceHandle filter, nuint index);

    /// <summary>
    /// Gets a filter by name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_filter_by_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_source_get_filter_by_name(
        ObsSourceHandle source,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the number of filters.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_filter_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_source_filter_count(ObsSourceHandle source);

    #endregion

    #region Signal Handler

    /// <summary>
    /// Gets the signal handler for the source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_signal_handler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SignalHandlerHandle obs_source_get_signal_handler(ObsSourceHandle source);

    #endregion

    #region State

    /// <summary>
    /// Checks if source is active.
    /// </summary>
    public static bool obs_source_active(ObsSourceHandle source) => obs_source_active_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_active_native(ObsSourceHandle source);

    /// <summary>
    /// Checks if source is showing.
    /// </summary>
    public static bool obs_source_showing(ObsSourceHandle source) => obs_source_showing_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_showing")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_showing_native(ObsSourceHandle source);

    /// <summary>
    /// Checks if source has been removed.
    /// </summary>
    public static bool obs_source_removed(ObsSourceHandle source) => obs_source_removed_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_removed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_removed_native(ObsSourceHandle source);

    #endregion

    #region Enumeration

    /// <summary>
    /// Callback for enumerating sources.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumSourceCallback(nint data, ObsSourceHandle source);

    /// <summary>
    /// Callback for enumerating a source's filters (obs_source_enum_proc_t).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EnumFilterCallback(ObsSourceHandle parent, ObsSourceHandle child, nint param);

    /// <summary>
    /// Enumerates the filters attached to a source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_enum_filters")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_enum_filters(ObsSourceHandle source, EnumFilterCallback callback, nint param);

    /// <summary>
    /// Changes a filter's position in the source's filter chain.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_filter_set_order")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_filter_set_order(ObsSourceHandle source, ObsSourceHandle filter, ObsOrderMovement movement);

    /// <summary>
    /// Plays or pauses media playback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_play_pause")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_media_play_pause(ObsSourceHandle source, [MarshalAs(UnmanagedType.U1)] bool pause);

    /// <summary>
    /// Restarts media playback from the beginning.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_restart")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_media_restart(ObsSourceHandle source);

    /// <summary>
    /// Stops media playback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_stop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_media_stop(ObsSourceHandle source);

    /// <summary>
    /// Skips to the next media item (playlist sources).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_next")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_media_next(ObsSourceHandle source);

    /// <summary>
    /// Skips to the previous media item (playlist sources).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_previous")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_media_previous(ObsSourceHandle source);

    /// <summary>
    /// Gets the media duration in milliseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_get_duration")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long obs_source_media_get_duration(ObsSourceHandle source);

    /// <summary>
    /// Gets the current media playback time in milliseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_get_time")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long obs_source_media_get_time(ObsSourceHandle source);

    /// <summary>
    /// Sets the current media playback time in milliseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_set_time")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_media_set_time(ObsSourceHandle source, long ms);

    /// <summary>
    /// Gets the media playback state.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_media_get_state")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsMediaState obs_source_media_get_state(ObsSourceHandle source);

    /// <summary>
    /// Enumerates all sources.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_sources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_sources(EnumSourceCallback callback, nint data);

    /// <summary>
    /// Enumerates all scenes.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_scenes")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_scenes(EnumSourceCallback callback, nint data);

    #endregion
}
