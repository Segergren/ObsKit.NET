using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// Wrapper for OBS settings (obs_data_t).
/// Provides a fluent API for configuring OBS objects.
/// </summary>
public sealed class Settings : ObsObject
{
    /// <summary>
    /// Creates a new empty settings object.
    /// </summary>
    public Settings() : base(ObsData.obs_data_create())
    {
    }

    internal Settings(ObsDataHandle handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    /// <summary>
    /// Creates settings from a JSON string.
    /// </summary>
    public static Settings FromJson(string json)
    {
        var handle = ObsData.obs_data_create_from_json(json);
        if (handle.IsNull)
            throw new ArgumentException("Invalid JSON string", nameof(json));
        return new Settings(handle);
    }

    /// <summary>
    /// Creates settings from a JSON file.
    /// </summary>
    public static Settings FromJsonFile(string path)
    {
        var handle = ObsData.obs_data_create_from_json_file(path);
        if (handle.IsNull)
            throw new FileNotFoundException("Settings file not found or invalid", path);
        return new Settings(handle);
    }

    internal new ObsDataHandle Handle => (ObsDataHandle)base.Handle;

    #region Setters (Fluent API)

    /// <summary>Sets a string value.</summary>
    public Settings Set(string name, string value)
    {
        ObsData.obs_data_set_string(Handle, name, value);
        return this;
    }

    /// <summary>Sets an integer value.</summary>
    public Settings Set(string name, long value)
    {
        ObsData.obs_data_set_int(Handle, name, value);
        return this;
    }

    /// <summary>Sets a double value.</summary>
    public Settings Set(string name, double value)
    {
        ObsData.obs_data_set_double(Handle, name, value);
        return this;
    }

    /// <summary>Sets a boolean value.</summary>
    public Settings Set(string name, bool value)
    {
        ObsData.obs_data_set_bool(Handle, name, value);
        return this;
    }

    /// <summary>Sets a nested settings object.</summary>
    public Settings Set(string name, Settings value)
    {
        ObsData.obs_data_set_obj(Handle, name, value.Handle);
        return this;
    }

    /// <summary>
    /// Sets a string-array value using OBS's editable-list convention, where each entry is
    /// stored as an object with a <c>"value"</c> key. This matches properties such as
    /// slideshow file lists and other <c>OBS_EDITABLE_LIST</c> properties.
    /// </summary>
    /// <param name="name">The setting key.</param>
    /// <param name="values">The string values to store.</param>
    public Settings SetStringArray(string name, IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var array = ObsData.obs_data_array_create();
        try
        {
            foreach (var value in values)
            {
                var item = ObsData.obs_data_create();
                try
                {
                    ObsData.obs_data_set_string(item, "value", value);
                    ObsData.obs_data_array_push_back(array, item);
                }
                finally
                {
                    ObsData.obs_data_release(item);
                }
            }

            ObsData.obs_data_set_array(Handle, name, array);
        }
        finally
        {
            ObsData.obs_data_array_release(array);
        }

        return this;
    }

    #endregion

    #region Getters

    /// <summary>Gets a string value.</summary>
    public string? GetString(string name) => ObsData.obs_data_get_string(Handle, name);

    /// <summary>Gets an integer value.</summary>
    public long GetInt(string name) => ObsData.obs_data_get_int(Handle, name);

    /// <summary>Gets a double value.</summary>
    public double GetDouble(string name) => ObsData.obs_data_get_double(Handle, name);

    /// <summary>Gets a boolean value.</summary>
    public bool GetBool(string name) => ObsData.obs_data_get_bool(Handle, name);

    /// <summary>Gets a nested settings object.</summary>
    public Settings? GetObject(string name)
    {
        var handle = ObsData.obs_data_get_obj(Handle, name);
        return handle.IsNull ? null : new Settings(handle);
    }

    /// <summary>
    /// Reads a string-array value stored using OBS's editable-list convention (each entry
    /// is an object with a <c>"value"</c> key). Returns an empty list if the key is absent
    /// or not an array.
    /// </summary>
    /// <param name="name">The setting key.</param>
    public IReadOnlyList<string> GetStringArray(string name)
    {
        var result = new List<string>();

        var array = ObsData.obs_data_get_array(Handle, name);
        if (array.IsNull)
            return result;

        try
        {
            var count = ObsData.obs_data_array_count(array);
            for (nuint i = 0; i < count; i++)
            {
                var item = ObsData.obs_data_array_item(array, i);
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
        }
        finally
        {
            ObsData.obs_data_array_release(array);
        }

        return result;
    }

    #endregion

    #region Default Setters

    /// <summary>Sets a default string value.</summary>
    public Settings SetDefault(string name, string value)
    {
        ObsData.obs_data_set_default_string(Handle, name, value);
        return this;
    }

    /// <summary>Sets a default integer value.</summary>
    public Settings SetDefault(string name, long value)
    {
        ObsData.obs_data_set_default_int(Handle, name, value);
        return this;
    }

    /// <summary>Sets a default double value.</summary>
    public Settings SetDefault(string name, double value)
    {
        ObsData.obs_data_set_default_double(Handle, name, value);
        return this;
    }

    /// <summary>Sets a default boolean value.</summary>
    public Settings SetDefault(string name, bool value)
    {
        ObsData.obs_data_set_default_bool(Handle, name, value);
        return this;
    }

    #endregion

    #region JSON

    /// <summary>Converts settings to a JSON string.</summary>
    public string? ToJson() => ObsData.obs_data_get_json(Handle);

    /// <summary>Converts settings to a formatted JSON string.</summary>
    public string? ToJsonPretty() => ObsData.obs_data_get_json_pretty(Handle);

    /// <summary>Saves settings to a JSON file.</summary>
    public bool SaveToFile(string path) => ObsData.obs_data_save_json(Handle, path);

    /// <summary>
    /// Saves settings to a JSON file using atomic write (temp file, then rename).
    /// </summary>
    public bool SaveToFileSafe(string path, string tempExt = ".tmp", string backupExt = ".bak")
        => ObsData.obs_data_save_json_safe(Handle, path, tempExt, backupExt);

    #endregion

    #region Utilities

    /// <summary>Applies settings from another settings object.</summary>
    public Settings Apply(Settings other)
    {
        ObsData.obs_data_apply(Handle, other.Handle);
        return this;
    }

    /// <summary>Clears all settings.</summary>
    public Settings Clear()
    {
        ObsData.obs_data_clear(Handle);
        return this;
    }

    /// <summary>Removes a setting by name.</summary>
    public Settings Remove(string name)
    {
        ObsData.obs_data_erase(Handle, name);
        return this;
    }

    #endregion

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsData.obs_data_release((ObsDataHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => ToJsonPretty() ?? "{}";
}
