using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// One selectable item of a list/combo property.
/// </summary>
/// <param name="Name">The localized display name.</param>
/// <param name="StringValue">The value for string-format lists (null otherwise).</param>
/// <param name="IntValue">The value for int-format lists (0 otherwise).</param>
/// <param name="IsDisabled">Whether the item is shown but not selectable.</param>
public sealed record ObsPropertyListItem(string Name, string? StringValue, long IntValue, bool IsDisabled);

/// <summary>
/// Describes a single configurable property of a source or encoder, as exposed by its
/// plugin. Use <see cref="ObsKit.NET.Sources.Source.GetProperties"/> to introspect the full
/// set — useful for building dynamic configuration UIs or discovering valid option values
/// (device pickers, resolutions, etc.).
/// </summary>
public sealed record ObsPropertyInfo
{
    /// <summary>The internal name — the settings key used with <see cref="Settings"/>.</summary>
    public required string Name { get; init; }

    /// <summary>The localized display label, if any.</summary>
    public string? Description { get; init; }

    /// <summary>The longer help/tooltip text, if any.</summary>
    public string? LongDescription { get; init; }

    /// <summary>The property kind.</summary>
    public ObsPropertyType Type { get; init; }

    /// <summary>Whether the property is currently enabled (editable).</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Whether the property is currently visible.</summary>
    public bool IsVisible { get; init; }

    /// <summary>For <see cref="ObsPropertyType.Int"/> properties: the (min, max, step) range; null otherwise.</summary>
    public (int Min, int Max, int Step)? IntRange { get; init; }

    /// <summary>For <see cref="ObsPropertyType.Float"/> properties: the (min, max, step) range; null otherwise.</summary>
    public (double Min, double Max, double Step)? FloatRange { get; init; }

    /// <summary>For <see cref="ObsPropertyType.List"/> properties: the value format of the items.</summary>
    public ObsPropertyListFormat ListFormat { get; init; }

    /// <summary>For <see cref="ObsPropertyType.List"/> properties: the selectable items (empty otherwise).</summary>
    public IReadOnlyList<ObsPropertyListItem> ListItems { get; init; } = Array.Empty<ObsPropertyListItem>();
}
