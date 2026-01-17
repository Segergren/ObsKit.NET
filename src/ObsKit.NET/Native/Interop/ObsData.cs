using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS data/settings functions.
/// </summary>
internal static partial class ObsData
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Creation and Release

    /// <summary>
    /// Creates a new OBS data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_data_create();

    /// <summary>
    /// Creates an OBS data object from JSON string.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_create_from_json")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_data_create_from_json(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string jsonString);

    /// <summary>
    /// Creates an OBS data object from a JSON file.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_create_from_json_file")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_data_create_from_json_file(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string jsonFile);

    /// <summary>
    /// Releases a reference to an OBS data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_release(ObsDataHandle data);

    /// <summary>
    /// Adds a reference to an OBS data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_addref(ObsDataHandle data);

    #endregion

    #region Setters

    /// <summary>
    /// Sets a string value in the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_string(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string val);

    /// <summary>
    /// Sets an integer value in the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_int")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_int(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        long val);

    /// <summary>
    /// Sets a double value in the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_double")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_double(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        double val);

    /// <summary>
    /// Sets a boolean value in the data object.
    /// </summary>
    public static void obs_data_set_bool(ObsDataHandle data, string name, bool val)
        => obs_data_set_bool_native(data, name, val ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_data_set_bool")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_data_set_bool_native(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        byte val);

    /// <summary>
    /// Sets a nested data object value.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_obj")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_obj(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle obj);

    /// <summary>
    /// Sets an array value in the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_array")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_array(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataArrayHandle array);

    #endregion

    #region Getters

    /// <summary>
    /// Gets a string value from the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_data_get_string(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets an integer value from the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_int")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long obs_data_get_int(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets a double value from the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_double")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double obs_data_get_double(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets a boolean value from the data object.
    /// </summary>
    public static bool obs_data_get_bool(ObsDataHandle data, string name)
        => obs_data_get_bool_native(data, name) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_data_get_bool")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_data_get_bool_native(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets a nested data object value.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_obj")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_data_get_obj(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets an array value from the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_array")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataArrayHandle obs_data_get_array(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    #endregion

    #region Default Setters

    /// <summary>
    /// Sets the default string value for a property.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_default_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_default_string(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string val);

    /// <summary>
    /// Sets the default integer value for a property.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_default_int")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_default_int(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        long val);

    /// <summary>
    /// Sets the default double value for a property.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_set_default_double")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_set_default_double(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        double val);

    /// <summary>
    /// Sets the default boolean value for a property.
    /// </summary>
    public static void obs_data_set_default_bool(ObsDataHandle data, string name, bool val)
        => obs_data_set_default_bool_native(data, name, val ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_data_set_default_bool")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_data_set_default_bool_native(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        byte val);

    #endregion

    #region JSON

    /// <summary>
    /// Gets the data object as a JSON string.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_json")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_data_get_json(ObsDataHandle data);

    /// <summary>
    /// Gets the data object as a pretty-printed JSON string.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_get_json_pretty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_data_get_json_pretty(ObsDataHandle data);

    /// <summary>
    /// Saves the data object to a JSON file.
    /// </summary>
    public static bool obs_data_save_json(ObsDataHandle data, string file)
        => obs_data_save_json_native(data, file) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_data_save_json")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_data_save_json_native(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string file);

    /// <summary>
    /// Saves the data object to a safe JSON file (atomic write).
    /// </summary>
    public static bool obs_data_save_json_safe(ObsDataHandle data, string file, string tempExt, string backupExt)
        => obs_data_save_json_safe_native(data, file, tempExt, backupExt) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_data_save_json_safe")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_data_save_json_safe_native(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string file,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string tempExt,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string backupExt);

    #endregion

    #region Data Array

    /// <summary>
    /// Creates a new OBS data array.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataArrayHandle obs_data_array_create();

    /// <summary>
    /// Releases a reference to an OBS data array.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_array_release(ObsDataArrayHandle array);

    /// <summary>
    /// Gets the number of items in the array.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_data_array_count(ObsDataArrayHandle array);

    /// <summary>
    /// Gets an item from the array at the specified index.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_item")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_data_array_item(ObsDataArrayHandle array, nuint idx);

    /// <summary>
    /// Pushes an item to the back of the array.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_push_back")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_array_push_back(ObsDataArrayHandle array, ObsDataHandle obj);

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_insert")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_array_insert(ObsDataArrayHandle array, nuint idx, ObsDataHandle obj);

    /// <summary>
    /// Erases an item at the specified index.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_array_erase")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_array_erase(ObsDataArrayHandle array, nuint idx);

    #endregion

    #region Utilities

    /// <summary>
    /// Applies the values from one data object to another.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_apply")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_apply(ObsDataHandle target, ObsDataHandle applyData);

    /// <summary>
    /// Clears all items from the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_clear")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_clear(ObsDataHandle data);

    /// <summary>
    /// Erases an item by name from the data object.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_data_erase")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_data_erase(
        ObsDataHandle data,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    #endregion
}
