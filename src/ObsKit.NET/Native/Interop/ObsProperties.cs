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

    /// <summary>Gets the default properties for an encoder type id.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_encoder_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_get_encoder_properties(
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

    /// <summary>Invokes a button property's callback (e.g. a plugin's "refresh" button).</summary>
    public static bool obs_property_button_clicked(nint property, nint obj)
        => obs_property_button_clicked_native(property, obj) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_property_button_clicked")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_property_button_clicked_native(nint property, nint obj);

    // ---- Enumeration & metadata ----

    /// <summary>Gets the first property in a properties object (for iteration), or null.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_properties_first")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_properties_first(nint props);

    /// <summary>
    /// Advances <paramref name="property"/> to the next property. Returns true while a
    /// property remains; sets the handle to null at the end.
    /// </summary>
    public static bool obs_property_next(ref nint property) => obs_property_next_native(ref property) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_property_next")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_property_next_native(ref nint property);

    /// <summary>Gets the internal name (settings key) of a property.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_property_name(nint property);

    /// <summary>Gets the localized display description of a property.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_description")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_property_description(nint property);

    /// <summary>Gets the long/help description of a property, if any.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_long_description")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_property_long_description(nint property);

    /// <summary>Gets the property type.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_get_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_property_get_type(nint property);

    public static bool obs_property_enabled(nint property) => obs_property_enabled_native(property) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_property_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_property_enabled_native(nint property);

    public static bool obs_property_visible(nint property) => obs_property_visible_native(property) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_property_visible")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_property_visible_native(nint property);

    [LibraryImport(Lib, EntryPoint = "obs_property_int_min")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_property_int_min(nint property);

    [LibraryImport(Lib, EntryPoint = "obs_property_int_max")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_property_int_max(nint property);

    [LibraryImport(Lib, EntryPoint = "obs_property_int_step")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_property_int_step(nint property);

    [LibraryImport(Lib, EntryPoint = "obs_property_float_min")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double obs_property_float_min(nint property);

    [LibraryImport(Lib, EntryPoint = "obs_property_float_max")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double obs_property_float_max(nint property);

    [LibraryImport(Lib, EntryPoint = "obs_property_float_step")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double obs_property_float_step(nint property);

    /// <summary>Gets the combo (list) format: 1=int, 2=float, 3=string, 4=bool.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_property_list_format")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_property_list_format(nint property);
}
