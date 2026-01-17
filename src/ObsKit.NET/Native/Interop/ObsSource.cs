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
    /// Adds a reference to a source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_addref(ObsSourceHandle source);

    /// <summary>
    /// Gets an additional reference to a source.
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
