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

    /// <summary>
    /// Enumerates all sources including private ones.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_all_sources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_all_sources(EnumSourceCallback callback, nint data);

    /// <summary>
    /// Gets a public source by name. Returns an incremented reference.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_source_by_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_get_source_by_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Duplicates a source. Returns an incremented reference — either a new source,
    /// or the same source when it cannot be duplicated (scenes when not creating a
    /// private copy, and sources with the DO_NOT_DUPLICATE output flag).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_duplicate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_source_duplicate(
        ObsSourceHandle source,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string? desiredName,
        byte createPrivate);

    /// <summary>
    /// Gets the base width for a source (not taking into account filtering).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_base_width")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_base_width(ObsSourceHandle source);

    /// <summary>
    /// Gets the base height for a source (not taking into account filtering).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_base_height")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_base_height(ObsSourceHandle source);

    /// <summary>
    /// Gets the source type id without any versioning suffix (e.g. "color_source"
    /// for "color_source_v3").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_unversioned_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_source_get_unversioned_id(ObsSourceHandle source);

    /// <summary>
    /// Gets the source's private settings (an incremented obs_data reference).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_private_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_source_get_private_settings(ObsSourceHandle source);

    /// <summary>
    /// Serializes a source (type, name, uuid, settings, filters, volume, mixers,
    /// sync offset, flags, deinterlacing, monitoring, hotkeys) into a new obs_data
    /// object owned by the caller.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_save_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_save_source(ObsSourceHandle source);

    /// <summary>
    /// Creates a public source from data produced by obs_save_source.
    /// Returns a new reference owned by the caller.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_load_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_load_source(ObsDataHandle data);

    /// <summary>
    /// Creates a private source from data produced by obs_save_source.
    /// Returns a new reference owned by the caller.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_load_private_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_load_private_source(ObsDataHandle data);

    /// <summary>
    /// Increments the source's "showing" state — lazy sources (e.g. game/window
    /// capture) start capturing as if displayed, without being rendered.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_inc_showing")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_inc_showing(ObsSourceHandle source);

    /// <summary>
    /// Decrements the source's "showing" state.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_dec_showing")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_dec_showing(ObsSourceHandle source);

    /// <summary>
    /// Increments the source's "active" state (as if displayed in the program
    /// output); also implies showing.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_inc_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_inc_active(ObsSourceHandle source);

    /// <summary>
    /// Decrements the source's "active" state.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_dec_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_dec_active(ObsSourceHandle source);

    /// <summary>
    /// Gets a weak reference to the source (an incremented weak reference owned by the caller).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_weak_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_source_get_weak_source(ObsSourceHandle source);

    /// <summary>
    /// Releases a weak source reference.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_weak_source_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_weak_source_release(nint weak);

    /// <summary>
    /// Gets whether the source a weak reference points to has been destroyed.
    /// </summary>
    public static bool obs_weak_source_expired(nint weak) => obs_weak_source_expired_native(weak) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_weak_source_expired")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_weak_source_expired_native(nint weak);

    /// <summary>
    /// Gets a strong reference from a weak reference, or null if the source was destroyed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_weak_source_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_weak_source_get_source(nint weak);

    #endregion

    #region Capabilities, Kind and Visibility

    /// <summary>
    /// Gets the capability flags of the source instance (OBS_SOURCE_VIDEO, OBS_SOURCE_AUDIO, ...).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_output_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_output_flags(ObsSourceHandle source);

    public static bool obs_source_is_scene(ObsSourceHandle source) => obs_source_is_scene_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_is_scene")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_is_scene_native(ObsSourceHandle source);

    public static bool obs_source_is_group(ObsSourceHandle source) => obs_source_is_group_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_is_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_is_group_native(ObsSourceHandle source);

    public static bool obs_source_type_is_scene(string id) => obs_source_type_is_scene_native(id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_type_is_scene")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_type_is_scene_native([MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    public static bool obs_source_type_is_group(string id) => obs_source_type_is_group_native(id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_type_is_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_type_is_group_native([MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    public static bool obs_source_is_hidden(ObsSourceHandle source) => obs_source_is_hidden_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_is_hidden")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_is_hidden_native(ObsSourceHandle source);

    public static void obs_source_set_hidden(ObsSourceHandle source, bool hidden)
        => obs_source_set_hidden_native(source, hidden ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_source_set_hidden")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_source_set_hidden_native(ObsSourceHandle source, byte hidden);

    public static bool obs_source_configurable(ObsSourceHandle source) => obs_source_configurable_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_configurable")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_configurable_native(ObsSourceHandle source);

    /// <summary>
    /// Gets the OBS version (packed major.minor.patch) the source was last saved with.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_last_obs_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_source_get_last_obs_version(ObsSourceHandle source);

    /// <summary>
    /// Sets the flags a source starts with before user flags are applied.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_default_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_default_flags(ObsSourceHandle source, uint flags);

    #endregion

    #region Settings, Properties and Rendering Hints

    /// <summary>
    /// Replaces the settings wholesale (clears, then applies) and updates the source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_reset_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_reset_settings(ObsSourceHandle source, ObsDataHandle settings);

    /// <summary>
    /// Emits the "update_properties" signal so property UIs refresh.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_update_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_update_properties(ObsSourceHandle source);

    /// <summary>
    /// Sets the rotation (degrees, multiples of 90) applied to async video frames. Declared as
    /// a C <c>long</c>; a native-sized integer covers both 32-bit (Windows) and 64-bit ABIs.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_set_async_rotation")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_set_async_rotation(ObsSourceHandle source, nint rotation);

    public static bool obs_source_get_texcoords_centered(ObsSourceHandle source) => obs_source_get_texcoords_centered_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_get_texcoords_centered")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_get_texcoords_centered_native(ObsSourceHandle source);

    /// <summary>
    /// Gets the color space the source renders in, choosing among <paramref name="preferredSpaces"/>
    /// (pointer to <paramref name="count"/> gs_color_space values, may be null with count 0).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_color_space")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial GsColorSpace obs_source_get_color_space(ObsSourceHandle source, nuint count, nint preferredSpaces);

    /// <summary>
    /// Gets a strong reference to the canvas the source belongs to (release when done), or null.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_canvas")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsCanvasHandle obs_source_get_canvas(ObsSourceHandle source);

    #endregion

    #region Audio State

    [LibraryImport(Lib, EntryPoint = "obs_source_get_speaker_layout")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SpeakerLayout obs_source_get_speaker_layout(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_get_audio_timestamp")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_source_get_audio_timestamp(ObsSourceHandle source);

    public static bool obs_source_audio_pending(ObsSourceHandle source) => obs_source_audio_pending_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_audio_pending")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_audio_pending_native(ObsSourceHandle source);

    public static bool obs_source_audio_active(ObsSourceHandle source) => obs_source_audio_active_native(source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_source_audio_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_source_audio_active_native(ObsSourceHandle source);

    public static void obs_source_set_audio_active(ObsSourceHandle source, bool active)
        => obs_source_set_audio_active_native(source, active ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_source_set_audio_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_source_set_audio_active_native(ObsSourceHandle source, byte active);

    #endregion

    #region Source Trees

    /// <summary>
    /// Enumerates the sources a composite source (scene, group, transition) is actively
    /// showing, one level deep. Callback sources are borrowed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_enum_active_sources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_enum_active_sources(ObsSourceHandle source, EnumFilterCallback callback, nint param);

    /// <summary>
    /// Enumerates the whole active child tree, depth first (children before their parent).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_enum_active_tree")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_enum_active_tree(ObsSourceHandle source, EnumFilterCallback callback, nint param);

    /// <summary>
    /// Enumerates the whole child tree including hidden items, depth first.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_enum_full_tree")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_enum_full_tree(ObsSourceHandle source, EnumFilterCallback callback, nint param);

    #endregion

    #region Filter Backup

    /// <summary>
    /// Serializes every filter on the source into a new array (release when done).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_backup_filters")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataArrayHandle obs_source_backup_filters(ObsSourceHandle source);

    /// <summary>
    /// Restores filters from a backup array: existing filters with the same name are updated
    /// and reordered, missing ones are recreated, others are removed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_restore_filters")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_restore_filters(ObsSourceHandle source, ObsDataArrayHandle array);

    #endregion

    #region Missing Files

    /// <summary>
    /// Gets a new missing-files collection for the source (destroy with obs_missing_files_destroy).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_missing_files")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_source_get_missing_files(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_missing_files_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_missing_files_count(nint files);

    /// <summary>
    /// Gets a borrowed file entry (owned by the collection).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_missing_files_get_file")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_missing_files_get_file(nint files, int idx);

    [LibraryImport(Lib, EntryPoint = "obs_missing_files_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_missing_files_destroy(nint files);

    [LibraryImport(Lib, EntryPoint = "obs_missing_file_get_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_missing_file_get_path(nint file);

    [LibraryImport(Lib, EntryPoint = "obs_missing_file_get_source_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_missing_file_get_source_name(nint file);

    /// <summary>
    /// Points the owning source at a replacement path (invokes the plugin's replace callback).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_missing_file_issue_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_missing_file_issue_callback(nint file, [MarshalUsing(typeof(Utf8StringMarshaler))] string newPath);

    #endregion

    #region Async Frames

    /// <summary>
    /// Takes the newest undisplayed async video frame (adds a reference; release with
    /// obs_source_release_frame). Returns null if none is pending. Points at an
    /// <see cref="ObsSourceFrameNative"/>.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_get_frame")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_source_get_frame(ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_source_release_frame")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_release_frame(ObsSourceHandle source, nint frame);

    #endregion

    public static bool obs_weak_source_references_source(nint weak, ObsSourceHandle source)
        => obs_weak_source_references_source_native(weak, source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_weak_source_references_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_weak_source_references_source_native(nint weak, ObsSourceHandle source);
}
