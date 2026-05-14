using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS property enumeration. Properties expose the dynamic
/// option lists (device pickers, resolutions, etc.) that source plugins populate.
/// </summary>
internal static partial class ObsProperties
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>Gets the properties for a source instance.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_source_properties(ObsSourceHandle source);

    /// <summary>Gets the default properties for a source type id.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_source_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_get_source_properties(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>Destroys a properties object returned by obs_source_properties.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_properties_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_properties_destroy(nint props);

    /// <summary>Gets a property by name from a properties object.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_properties_get")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_properties_get(
        nint props,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string property);

    /// <summary>Gets the number of items in a list property.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_list_item_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_property_list_item_count(nint property);

    /// <summary>Gets the display name of a list item.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_list_item_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_property_list_item_name(nint property, nuint idx);

    /// <summary>Gets the string value of a list item.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_list_item_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_property_list_item_string(nint property, nuint idx);

    /// <summary>Gets whether a list item is disabled.</summary>
    public static bool obs_property_list_item_disabled(nint property, nuint idx)
        => obs_property_list_item_disabled_native(property, idx) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_property_list_item_disabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_property_list_item_disabled_native(nint property, nuint idx);

    /// <summary>Gets the integer value of a list item (for int-typed list properties).</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_list_item_int")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long obs_property_list_item_int(nint property, nuint idx);
}
