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

    /// <summary>
    /// Creates settings from a JSON file, falling back to its backup (path + backupExt,
    /// as written by <see cref="SaveToFileSafe"/>) when the main file is missing or corrupt.
    /// </summary>
    /// <param name="path">The JSON file path.</param>
    /// <param name="backupExt">The backup extension used when the file was saved.</param>
    public static Settings FromJsonFileSafe(string path, string backupExt = ".bak")
    {
        var handle = ObsData.obs_data_create_from_json_file_safe(path, backupExt);
        if (handle.IsNull)
            throw new FileNotFoundException("Settings file (and backup) not found or invalid", path);
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

    #region Default Getters / Introspection

    /// <summary>Gets the default string value for a key.</summary>
    public string? GetDefaultString(string name) => ObsData.obs_data_get_default_string(Handle, name);

    /// <summary>Gets the default integer value for a key.</summary>
    public long GetDefaultInt(string name) => ObsData.obs_data_get_default_int(Handle, name);

    /// <summary>Gets the default double value for a key.</summary>
    public double GetDefaultDouble(string name) => ObsData.obs_data_get_default_double(Handle, name);

    /// <summary>Gets the default boolean value for a key.</summary>
    public bool GetDefaultBool(string name) => ObsData.obs_data_get_default_bool(Handle, name);

    /// <summary>
    /// Gets whether a key has an explicitly set value (as opposed to only a default).
    /// </summary>
    public bool HasValue(string name) => ObsData.obs_data_has_user_value(Handle, name);

    /// <summary>
    /// Gets whether a key has a default value.
    /// </summary>
    public bool HasDefaultValue(string name) => ObsData.obs_data_has_default_value(Handle, name);

    /// <summary>
    /// Removes the explicitly set value for a key, reverting it to its default.
    /// (<see cref="Remove"/> removes both the value and the default.)
    /// </summary>
    public Settings Unset(string name)
    {
        ObsData.obs_data_unset_user_value(Handle, name);
        return this;
    }

    /// <summary>
    /// Removes the default value for a key.
    /// </summary>
    public Settings UnsetDefault(string name)
    {
        ObsData.obs_data_unset_default_value(Handle, name);
        return this;
    }

    /// <summary>
    /// Creates a new settings object containing only this object's default values.
    /// Dispose it when done.
    /// </summary>
    public Settings GetDefaults() => new(ObsData.obs_data_get_defaults(Handle), ownsHandle: true);

    /// <summary>
    /// Gets the names and value types of all entries with explicitly set values.
    /// </summary>
    public IReadOnlyList<(string Name, ObsDataType Type)> GetEntries()
    {
        var entries = new List<(string, ObsDataType)>();
        // obs_data_first references the item; obs_data_item_next releases it and
        // references the next, nulling the pointer past the last item — so a full
        // iteration needs no explicit release.
        var item = ObsData.obs_data_first(Handle);
        for (; item != nint.Zero; ObsData.obs_data_item_next(ref item))
        {
            var name = ObsData.obs_data_item_get_name(item);
            if (name != null)
                entries.Add((name, ObsData.obs_data_item_gettype(item)));
        }
        return entries;
    }

    /// <summary>
    /// Gets the names of all entries with explicitly set values.
    /// </summary>
    public IReadOnlyList<string> GetKeys()
    {
        var entries = GetEntries();
        var keys = new List<string>(entries.Count);
        foreach (var (name, _) in entries)
            keys.Add(name);
        return keys;
    }

    #endregion

    #region JSON

    /// <summary>Converts settings to a JSON string.</summary>
    public string? ToJson() => ObsData.obs_data_get_json(Handle);

    /// <summary>Converts settings to a formatted JSON string.</summary>
    public string? ToJsonPretty() => ObsData.obs_data_get_json_pretty(Handle);

    /// <summary>
    /// Converts settings to a JSON string, including entries that only have default values.
    /// </summary>
    public string? ToJsonWithDefaults() => ObsData.obs_data_get_json_with_defaults(Handle);

    /// <summary>
    /// Converts settings to a formatted JSON string, including entries that only have
    /// default values.
    /// </summary>
    public string? ToJsonPrettyWithDefaults() => ObsData.obs_data_get_json_pretty_with_defaults(Handle);

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

    // obs_data is reference-counted independently of the OBS core (it exists before obs_startup and
    // after obs_shutdown, and obs_data_release never touches the global obs), so it must always be
    // released — including from a finalizer that runs after the core is shut down.
    /// <inheritdoc/>
    protected override bool ReleaseRequiresObs => false;

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsData.obs_data_release((ObsDataHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => ToJsonPretty() ?? "{}";

    #region Arrays, Vectors and Frame Rates

    /// <summary>Sets an array value (this object takes its own reference to the array).</summary>
    public Settings Set(string name, SettingsArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ObsData.obs_data_set_array(Handle, name, value.Handle);
        return this;
    }

    /// <summary>Sets a 2D vector value (stored as an object with x/y keys).</summary>
    public Settings Set(string name, Vec2 value)
    {
        ObsData.obs_data_set_vec2(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a 3D vector value (stored as an object with x/y/z keys).</summary>
    public Settings Set(string name, Vec3 value)
    {
        ObsData.obs_data_set_vec3(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a 4D vector value (stored as an object with x/y/z/w keys).</summary>
    public Settings Set(string name, Vec4 value)
    {
        ObsData.obs_data_set_vec4(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a quaternion value (stored as an object with x/y/z/w keys).</summary>
    public Settings Set(string name, Quat value)
    {
        ObsData.obs_data_set_quat(Handle, name, in value);
        return this;
    }

    /// <summary>
    /// Sets a frame-rate value in the format used by <c>OBS_PROPERTY_FRAME_RATE</c> properties:
    /// either a numerator/denominator pair or, when <paramref name="option"/> is given, a
    /// named option such as <c>"Match Output FPS"</c>.
    /// </summary>
    public Settings SetFramesPerSecond(string name, MediaFramesPerSecond fps, string? option = null)
    {
        ObsData.obs_data_set_frames_per_second(Handle, name, fps, option);
        return this;
    }

    /// <summary>
    /// Gets an array value. The returned array holds its own reference; dispose it when done.
    /// Returns null if the key is absent or not an array.
    /// </summary>
    public SettingsArray? GetArray(string name)
    {
        var handle = ObsData.obs_data_get_array(Handle, name);
        return handle.IsNull ? null : new SettingsArray(handle);
    }

    /// <summary>Gets a 2D vector value (zero if absent).</summary>
    public Vec2 GetVec2(string name)
    {
        ObsData.obs_data_get_vec2(Handle, name, out var value);
        return value;
    }

    /// <summary>Gets a 3D vector value (zero if absent).</summary>
    public Vec3 GetVec3(string name)
    {
        ObsData.obs_data_get_vec3(Handle, name, out var value);
        return value;
    }

    /// <summary>Gets a 4D vector value (zero if absent).</summary>
    public Vec4 GetVec4(string name)
    {
        ObsData.obs_data_get_vec4(Handle, name, out var value);
        return value;
    }

    /// <summary>Gets a quaternion value (zero if absent).</summary>
    public Quat GetQuat(string name)
    {
        ObsData.obs_data_get_quat(Handle, name, out var value);
        return value;
    }

    /// <summary>
    /// Reads a frame-rate value written by <see cref="SetFramesPerSecond"/> (or an OBS
    /// frame-rate property). Exactly one of <paramref name="fps"/> (valid) or
    /// <paramref name="option"/> (non-null) is populated when this returns true.
    /// </summary>
    public bool TryGetFramesPerSecond(string name, out MediaFramesPerSecond fps, out string? option)
    {
        var found = ObsData.obs_data_get_frames_per_second(Handle, name, out fps, out var optionPtr);
        option = optionPtr != nint.Zero ? System.Runtime.InteropServices.Marshal.PtrToStringUTF8(optionPtr) : null;
        return found;
    }

    #endregion

    #region Default Objects, Arrays and Vectors

    /// <summary>Sets a default nested object.</summary>
    public Settings SetDefault(string name, Settings value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ObsData.obs_data_set_default_obj(Handle, name, value.Handle);
        return this;
    }

    /// <summary>Sets a default array value.</summary>
    public Settings SetDefault(string name, SettingsArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ObsData.obs_data_set_default_array(Handle, name, value.Handle);
        return this;
    }

    /// <summary>Sets a default 2D vector value.</summary>
    public Settings SetDefault(string name, Vec2 value)
    {
        ObsData.obs_data_set_default_vec2(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a default 3D vector value.</summary>
    public Settings SetDefault(string name, Vec3 value)
    {
        ObsData.obs_data_set_default_vec3(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a default 4D vector value.</summary>
    public Settings SetDefault(string name, Vec4 value)
    {
        ObsData.obs_data_set_default_vec4(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a default quaternion value.</summary>
    public Settings SetDefault(string name, Quat value)
    {
        ObsData.obs_data_set_default_quat(Handle, name, in value);
        return this;
    }

    /// <summary>Sets a default frame-rate value (see <see cref="SetFramesPerSecond"/>).</summary>
    public Settings SetDefaultFramesPerSecond(string name, MediaFramesPerSecond fps, string? option = null)
    {
        ObsData.obs_data_set_default_frames_per_second(Handle, name, fps, option);
        return this;
    }

    /// <summary>Gets a default nested object (dispose when done), or null if none.</summary>
    public Settings? GetDefaultObject(string name)
    {
        var handle = ObsData.obs_data_get_default_obj(Handle, name);
        return handle.IsNull ? null : new Settings(handle);
    }

    /// <summary>Gets a default array value (dispose when done), or null if none.</summary>
    public SettingsArray? GetDefaultArray(string name)
    {
        var handle = ObsData.obs_data_get_default_array(Handle, name);
        return handle.IsNull ? null : new SettingsArray(handle);
    }

    /// <summary>Gets a default 2D vector value (zero if absent).</summary>
    public Vec2 GetDefaultVec2(string name)
    {
        ObsData.obs_data_get_default_vec2(Handle, name, out var value);
        return value;
    }

    /// <summary>Gets a default 3D vector value (zero if absent).</summary>
    public Vec3 GetDefaultVec3(string name)
    {
        ObsData.obs_data_get_default_vec3(Handle, name, out var value);
        return value;
    }

    /// <summary>Gets a default 4D vector value (zero if absent).</summary>
    public Vec4 GetDefaultVec4(string name)
    {
        ObsData.obs_data_get_default_vec4(Handle, name, out var value);
        return value;
    }

    /// <summary>Gets a default quaternion value (zero if absent).</summary>
    public Quat GetDefaultQuat(string name)
    {
        ObsData.obs_data_get_default_quat(Handle, name, out var value);
        return value;
    }

    /// <summary>Reads a default frame-rate value (see <see cref="TryGetFramesPerSecond"/>).</summary>
    public bool TryGetDefaultFramesPerSecond(string name, out MediaFramesPerSecond fps, out string? option)
    {
        var found = ObsData.obs_data_get_default_frames_per_second(Handle, name, out fps, out var optionPtr);
        option = optionPtr != nint.Zero ? System.Runtime.InteropServices.Marshal.PtrToStringUTF8(optionPtr) : null;
        return found;
    }

    #endregion

    #region JSON Extras

    /// <summary>
    /// Gets the JSON produced by the most recent <see cref="ToJson"/>/<see cref="ToJsonPretty"/>
    /// call without re-serializing, or null if none has been produced yet.
    /// </summary>
    public string? LastJson => ObsData.obs_data_get_last_json(Handle);

    /// <summary>
    /// Saves pretty-printed JSON atomically: writes to <c>path + tempExt</c>, keeps the previous
    /// file as <c>path + backupExt</c>, then renames into place (see <see cref="SaveToFileSafe"/>).
    /// </summary>
    public bool SaveToFilePrettySafe(string path, string tempExt = ".tmp", string backupExt = ".bak")
        => ObsData.obs_data_save_json_pretty_safe(Handle, path, tempExt, backupExt);

    #endregion
}
