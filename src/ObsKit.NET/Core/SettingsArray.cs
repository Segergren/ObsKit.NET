using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// An ordered list of <see cref="Settings"/> objects (obs_data_array_t). Used by libobs for
/// bulk source persistence (<c>Obs.SaveSources</c>), filter backups, hotkey bindings, and
/// array-valued settings such as editable lists.
/// </summary>
/// <remarks>
/// Items returned by <see cref="Get"/> and <see cref="ToList"/> hold their own reference and
/// must be disposed. Adding an item copies a reference into the array; the caller keeps
/// ownership of the object it passed in.
/// </remarks>
public sealed class SettingsArray : ObsObject
{
    /// <summary>
    /// Creates a new empty array.
    /// </summary>
    public SettingsArray() : base(ObsData.obs_data_array_create())
    {
    }

    internal SettingsArray(ObsDataArrayHandle handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    internal new ObsDataArrayHandle Handle => (ObsDataArrayHandle)base.Handle;

    /// <summary>
    /// Creates an array of objects that each hold a single <c>"value"</c> string, which is
    /// OBS's editable-list convention.
    /// </summary>
    /// <param name="values">The strings to store.</param>
    public static SettingsArray FromStrings(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var array = new SettingsArray();
        foreach (var value in values)
        {
            using var item = new Settings().Set("value", value);
            array.Add(item);
        }
        return array;
    }

    /// <summary>Gets the number of items.</summary>
    public int Count => (int)ObsData.obs_data_array_count(Handle);

    /// <summary>
    /// Gets the item at <paramref name="index"/>. The returned object holds its own
    /// reference; dispose it when done.
    /// </summary>
    /// <param name="index">Zero-based index.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the array.</exception>
    public Settings Get(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        var handle = ObsData.obs_data_array_item(Handle, (nuint)index);
        if (handle.IsNull)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new Settings(handle);
    }

    /// <summary>Appends an item (the array takes its own reference).</summary>
    public SettingsArray Add(Settings item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ObsData.obs_data_array_push_back(Handle, item.Handle);
        return this;
    }

    /// <summary>Appends every item of another array.</summary>
    public SettingsArray AddRange(SettingsArray items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ObsData.obs_data_array_push_back_array(Handle, items.Handle);
        return this;
    }

    /// <summary>Inserts an item at <paramref name="index"/> (the array takes its own reference).</summary>
    public SettingsArray Insert(int index, Settings item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ObsData.obs_data_array_insert(Handle, (nuint)index, item.Handle);
        return this;
    }

    /// <summary>Removes the item at <paramref name="index"/>.</summary>
    public SettingsArray RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ObsData.obs_data_array_erase(Handle, (nuint)index);
        return this;
    }

    /// <summary>
    /// Snapshots every item into a list. Each element holds its own reference; dispose them
    /// when done.
    /// </summary>
    public List<Settings> ToList()
    {
        var count = ObsData.obs_data_array_count(Handle);
        var result = new List<Settings>((int)count);
        for (nuint i = 0; i < count; i++)
        {
            var handle = ObsData.obs_data_array_item(Handle, i);
            if (!handle.IsNull)
                result.Add(new Settings(handle));
        }
        return result;
    }

    /// <summary>
    /// Reads the <c>"value"</c> string of every item (the inverse of <see cref="FromStrings"/>).
    /// Items without a value are skipped.
    /// </summary>
    public IReadOnlyList<string> ToStrings()
    {
        var count = ObsData.obs_data_array_count(Handle);
        var result = new List<string>((int)count);
        for (nuint i = 0; i < count; i++)
        {
            var item = ObsData.obs_data_array_item(Handle, i);
            if (item.IsNull)
                continue;
            try
            {
                var value = ObsData.obs_data_get_string(item, "value");
                if (!string.IsNullOrEmpty(value))
                    result.Add(value);
            }
            finally
            {
                ObsData.obs_data_release(item);
            }
        }
        return result;
    }

    /// <summary>
    /// Returns a JSON array string of the items (each rendered with <see cref="Settings.ToJson"/>).
    /// </summary>
    public string ToJson()
    {
        var items = ToList();
        try
        {
            return "[" + string.Join(",", items.Select(s => s.ToJson() ?? "{}")) + "]";
        }
        finally
        {
            foreach (var item in items)
                item.Dispose();
        }
    }

    // obs_data_array is reference-counted independently of the OBS core, like obs_data.
    /// <inheritdoc/>
    protected override bool ReleaseRequiresObs => false;

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsData.obs_data_array_release((ObsDataArrayHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"SettingsArray[{Count}]";
}
