using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// Shared logic for introspecting an OBS properties object (obs_properties_t) into the
/// public <see cref="ObsPropertyInfo"/> model. Used by both source and encoder introspection.
/// </summary>
internal static class ObsPropertyReader
{
    /// <summary>
    /// Reads every property from a properties handle in display order, then destroys the
    /// handle. Returns an empty list if <paramref name="props"/> is null.
    /// </summary>
    public static IReadOnlyList<ObsPropertyInfo> ReadAllAndDestroy(nint props)
    {
        var result = new List<ObsPropertyInfo>();
        if (props == 0)
            return result;

        try
        {
            for (var p = ObsProperties.obs_properties_first(props); p != 0; ObsProperties.obs_property_next(ref p))
                result.Add(ReadProperty(p));
        }
        finally
        {
            ObsProperties.obs_properties_destroy(props);
        }

        return result;
    }

    private static ObsPropertyInfo ReadProperty(nint p)
    {
        var type = (ObsPropertyType)ObsProperties.obs_property_get_type(p);

        (int, int, int)? intRange = type == ObsPropertyType.Int
            ? (ObsProperties.obs_property_int_min(p), ObsProperties.obs_property_int_max(p), ObsProperties.obs_property_int_step(p))
            : null;

        (double, double, double)? floatRange = type == ObsPropertyType.Float
            ? (ObsProperties.obs_property_float_min(p), ObsProperties.obs_property_float_max(p), ObsProperties.obs_property_float_step(p))
            : null;

        var listFormat = ObsPropertyListFormat.Invalid;
        IReadOnlyList<ObsPropertyListItem> listItems = Array.Empty<ObsPropertyListItem>();
        if (type == ObsPropertyType.List)
        {
            listFormat = (ObsPropertyListFormat)ObsProperties.obs_property_list_format(p);
            var count = ObsProperties.obs_property_list_item_count(p);
            var items = new List<ObsPropertyListItem>((int)count);
            for (nuint i = 0; i < count; i++)
            {
                items.Add(new ObsPropertyListItem(
                    ObsProperties.obs_property_list_item_name(p, i) ?? string.Empty,
                    ObsProperties.obs_property_list_item_string(p, i),
                    ObsProperties.obs_property_list_item_int(p, i),
                    ObsProperties.obs_property_list_item_disabled(p, i)));
            }
            listItems = items;
        }

        return new ObsPropertyInfo
        {
            Name = ObsProperties.obs_property_name(p) ?? string.Empty,
            Description = ObsProperties.obs_property_description(p),
            LongDescription = ObsProperties.obs_property_long_description(p),
            Type = type,
            IsEnabled = ObsProperties.obs_property_enabled(p),
            IsVisible = ObsProperties.obs_property_visible(p),
            IntRange = intRange,
            FloatRange = floatRange,
            ListFormat = listFormat,
            ListItems = listItems,
        };
    }
}
