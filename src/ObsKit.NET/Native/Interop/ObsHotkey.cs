using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS hotkey enumeration and triggering.
/// </summary>
internal static partial class ObsHotkey
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// Callback for enumerating hotkeys. Return 0 to stop enumerating.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumHotkeyCallback(nint data, nuint id, nint key);

    /// <summary>
    /// Enumerates all registered hotkeys.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_hotkeys")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_hotkeys(EnumHotkeyCallback callback, nint data);

    /// <summary>
    /// Gets the internal name of a hotkey (e.g. "hotkey_start").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_hotkey_get_name(nint key);

    /// <summary>
    /// Gets the localized description of a hotkey.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_description")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_hotkey_get_description(nint key);

    /// <summary>
    /// Gets what kind of object registered the hotkey (frontend/source/output/encoder/service).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_registerer_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_hotkey_get_registerer_type(nint key);

    /// <summary>
    /// Gets a weak reference pointer to the object that registered the hotkey.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_registerer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_hotkey_get_registerer(nint key);

    /// <summary>
    /// Invokes a hotkey's registered callback by id. Only effective while
    /// callback rerouting is enabled.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_trigger_routed_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_trigger_routed_callback(nuint id, byte pressed);

    /// <summary>
    /// Enables routing of hotkey callbacks through obs_hotkey_trigger_routed_callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_enable_callback_rerouting")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_enable_callback_rerouting(byte enable);

    /// <summary>
    /// Gets a strong source reference from a weak source reference (for hotkey registerers).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_weak_source_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsSourceHandle obs_weak_source_get_source(nint weak);

    #region Registration

    /// <summary>
    /// Callback invoked when a registered hotkey is pressed or released.
    /// Fired on the thread that injects the key event (or triggers the callback).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void HotkeyCallback(nint data, nuint id, nint hotkey, byte pressed);

    /// <summary>
    /// Callback for one half of a hotkey pair. Return 1 if the press took effect
    /// (blocks the partner hotkey from also firing on the same combination).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte HotkeyActiveCallback(nint data, nuint id, nint hotkey, byte pressed);

    /// <summary>
    /// Registers an application-level (frontend) hotkey.
    /// Returns OBS_INVALID_HOTKEY_ID (~0) on failure.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_register_frontend")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_register_frontend(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description,
        HotkeyCallback func,
        nint data);

    /// <summary>
    /// Registers a hotkey tied to a source (saved/loaded with the source).
    /// Returns OBS_INVALID_HOTKEY_ID (~0) on failure.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_register_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_register_source(
        Types.ObsSourceHandle source,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description,
        HotkeyCallback func,
        nint data);

    /// <summary>
    /// Registers a hotkey tied to an output.
    /// Returns OBS_INVALID_HOTKEY_ID (~0) on failure.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_register_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_register_output(
        Types.ObsOutputHandle output,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description,
        HotkeyCallback func,
        nint data);

    /// <summary>
    /// Registers a pair of mutually-exclusive frontend hotkeys (e.g. start/stop).
    /// Returns OBS_INVALID_HOTKEY_PAIR_ID (~0) on failure.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_register_frontend")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_pair_register_frontend(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name1,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description1,
        HotkeyActiveCallback func0,
        HotkeyActiveCallback func1,
        nint data0,
        nint data1);

    /// <summary>
    /// Unregisters a hotkey by id.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_unregister")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_unregister(nuint id);

    /// <summary>
    /// Unregisters a hotkey pair by pair id.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_unregister")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_pair_unregister(nuint id);

    #endregion

    #region Bindings/Events

    /// <summary>
    /// Replaces the key combinations bound to a hotkey. Pass num = 0 to clear.
    /// combinations points to a pinned array of obs_key_combination.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_load_bindings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_load_bindings(nuint id, nint combinations, nuint num);

    /// <summary>
    /// Feeds a key press/release event into the hotkey system, which matches it
    /// against bindings and fires the registered callbacks synchronously.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_inject_event")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_inject_event(Types.ObsKeyCombination hotkey, byte pressed);

    /// <summary>
    /// Enables/disables press events while a modifier-matched binding is held in
    /// the background (see obs-hotkey.c for exact semantics).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_enable_background_press")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_enable_background_press(byte enable);

    /// <summary>
    /// Callback for enumerating hotkey bindings. Return 0 to stop enumerating.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumHotkeyBindingCallback(nint data, nuint idx, nint binding);

    /// <summary>
    /// Enumerates all key-combination bindings across all hotkeys.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_hotkey_bindings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_hotkey_bindings(EnumHotkeyBindingCallback callback, nint data);

    /// <summary>
    /// Gets the key combination of a binding.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_binding_get_key_combination")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsKeyCombination obs_hotkey_binding_get_key_combination(nint binding);

    /// <summary>
    /// Gets the id of the hotkey a binding belongs to.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_binding_get_hotkey_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_binding_get_hotkey_id(nint binding);

    #endregion

    #region Key Conversion

    /// <summary>
    /// Gets a key from its OBS name (e.g. "OBS_KEY_F1"). Returns OBS_KEY_NONE for unknown names.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_key_from_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsKey obs_key_from_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the OBS name of a key (e.g. "OBS_KEY_F1").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_key_to_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_key_to_name(Types.ObsKey key);

    /// <summary>
    /// Converts an OS virtual key code to an OBS key.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_key_from_virtual_key")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsKey obs_key_from_virtual_key(int code);

    /// <summary>
    /// Converts an OBS key to an OS virtual key code.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_key_to_virtual_key")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_key_to_virtual_key(Types.ObsKey key);

    /// <summary>
    /// libobs dstr — dynamically allocated string. Free the array with bfree.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DStrNative
    {
        public nint Array;
        public nuint Len;
        public nuint Capacity;
    }

    /// <summary>
    /// Writes a human-readable, localized display string for a key into str.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_key_to_str")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_key_to_str(Types.ObsKey key, ref DStrNative str);

    /// <summary>
    /// Writes a human-readable, localized display string for a key combination
    /// (e.g. "Ctrl + Shift + F1") into str.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_key_combination_to_str")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_key_combination_to_str(Types.ObsKeyCombination key, ref DStrNative str);

    #endregion

    #region Encoder/Service Registration and Pairs on Objects

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_register_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_register_encoder(
        Types.ObsEncoderHandle encoder,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description,
        HotkeyCallback func,
        nint data);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_register_service")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_register_service(
        Types.ObsServiceHandle service,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description,
        HotkeyCallback func,
        nint data);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_register_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_pair_register_source(
        Types.ObsSourceHandle source,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name1,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description1,
        HotkeyActiveCallback func0,
        HotkeyActiveCallback func1,
        nint data0,
        nint data1);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_register_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_pair_register_output(
        Types.ObsOutputHandle output,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name1,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description1,
        HotkeyActiveCallback func0,
        HotkeyActiveCallback func1,
        nint data0,
        nint data1);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_register_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_pair_register_encoder(
        Types.ObsEncoderHandle encoder,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name1,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description1,
        HotkeyActiveCallback func0,
        HotkeyActiveCallback func1,
        nint data0,
        nint data1);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_register_service")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_pair_register_service(
        Types.ObsServiceHandle service,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name1,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description1,
        HotkeyActiveCallback func0,
        HotkeyActiveCallback func1,
        nint data0,
        nint data1);

    #endregion

    #region Names, Descriptions and Pair Partners

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_set_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_set_name(nuint id, [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_set_description")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_set_description(nuint id, [MarshalUsing(typeof(Utf8StringMarshaler))] string description);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_set_names")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_pair_set_names(
        nuint id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name1);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_set_descriptions")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_pair_set_descriptions(
        nuint id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description0,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string description1);

    /// <summary>
    /// Gets the id of the other hotkey in a pair, or OBS_INVALID_HOTKEY_PAIR_ID (nuint.MaxValue).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_pair_partner_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_hotkey_get_pair_partner_id(nint key);

    #endregion

    #region Binding Persistence

    /// <summary>
    /// Saves a hotkey's bindings into a new array (release when done), in OBS's hotkey JSON
    /// format ({"key": "OBS_KEY_F1", "control": true, ...} per binding).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_save")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsDataArrayHandle obs_hotkey_save(nuint id);

    /// <summary>
    /// Replaces a hotkey's bindings from an array in OBS's hotkey JSON format.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_load")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_load(nuint id, Types.ObsDataArrayHandle data);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_save")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_pair_save(nuint id, out Types.ObsDataArrayHandle data0, out Types.ObsDataArrayHandle data1);

    [LibraryImport(Lib, EntryPoint = "obs_hotkey_pair_load")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_pair_load(nuint id, Types.ObsDataArrayHandle data0, Types.ObsDataArrayHandle data1);

    /// <summary>
    /// Saves all hotkeys registered by a source (keyed by hotkey name) into a new data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_save_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsDataHandle obs_hotkeys_save_source(Types.ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_load_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkeys_load_source(Types.ObsSourceHandle source, Types.ObsDataHandle hotkeys);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_save_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsDataHandle obs_hotkeys_save_output(Types.ObsOutputHandle output);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_load_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkeys_load_output(Types.ObsOutputHandle output, Types.ObsDataHandle hotkeys);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_save_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsDataHandle obs_hotkeys_save_encoder(Types.ObsEncoderHandle encoder);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_load_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkeys_load_encoder(Types.ObsEncoderHandle encoder, Types.ObsDataHandle hotkeys);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_save_service")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsDataHandle obs_hotkeys_save_service(Types.ObsServiceHandle service);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_load_service")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkeys_load_service(Types.ObsServiceHandle service, Types.ObsDataHandle hotkeys);

    #endregion

    #region Translations

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_set_audio_hotkeys_translations")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkeys_set_audio_hotkeys_translations(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string mute,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string unmute,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string pushToMute,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string pushToTalk);

    [LibraryImport(Lib, EntryPoint = "obs_hotkeys_set_sceneitem_hotkeys_translations")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkeys_set_sceneitem_hotkeys_translations(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string show,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string hide);

    #endregion
}
