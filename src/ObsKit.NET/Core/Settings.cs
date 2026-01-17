using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// Wrapper for OBS settings (obs_data_t).
/// Provides a fluent API for configuring OBS objects.
/// </summary>
public sealed class Settings : ObsObject
{
    public Settings() : base(ObsData.obs_data_create())
    {
    }

    internal Settings(ObsDataHandle handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    public static Settings FromJson(string json)
    {
        var handle = ObsData.obs_data_create_from_json(json);
        if (handle.IsNull)
            throw new ArgumentException("Invalid JSON string", nameof(json));
        return new Settings(handle);
    }

    public static Settings FromJsonFile(string path)
    {
        var handle = ObsData.obs_data_create_from_json_file(path);
        if (handle.IsNull)
            throw new FileNotFoundException("Settings file not found or invalid", path);
        return new Settings(handle);
    }

    internal new ObsDataHandle Handle => (ObsDataHandle)base.Handle;

    #region Setters (Fluent API)

    public Settings Set(string name, string value)
    {
        ObsData.obs_data_set_string(Handle, name, value);
        return this;
    }

    public Settings Set(string name, long value)
    {
        ObsData.obs_data_set_int(Handle, name, value);
        return this;
    }

    public Settings Set(string name, double value)
    {
        ObsData.obs_data_set_double(Handle, name, value);
        return this;
    }

    public Settings Set(string name, bool value)
    {
        ObsData.obs_data_set_bool(Handle, name, value);
        return this;
    }

    public Settings Set(string name, Settings value)
    {
        ObsData.obs_data_set_obj(Handle, name, value.Handle);
        return this;
    }

    #endregion

    #region Getters

    public string? GetString(string name) => ObsData.obs_data_get_string(Handle, name);

    public long GetInt(string name) => ObsData.obs_data_get_int(Handle, name);

    public double GetDouble(string name) => ObsData.obs_data_get_double(Handle, name);

    public bool GetBool(string name) => ObsData.obs_data_get_bool(Handle, name);

    public Settings? GetObject(string name)
    {
        var handle = ObsData.obs_data_get_obj(Handle, name);
        return handle.IsNull ? null : new Settings(handle);
    }

    #endregion

    #region Default Setters

    public Settings SetDefault(string name, string value)
    {
        ObsData.obs_data_set_default_string(Handle, name, value);
        return this;
    }

    public Settings SetDefault(string name, long value)
    {
        ObsData.obs_data_set_default_int(Handle, name, value);
        return this;
    }

    public Settings SetDefault(string name, double value)
    {
        ObsData.obs_data_set_default_double(Handle, name, value);
        return this;
    }

    public Settings SetDefault(string name, bool value)
    {
        ObsData.obs_data_set_default_bool(Handle, name, value);
        return this;
    }

    #endregion

    #region JSON

    public string? ToJson() => ObsData.obs_data_get_json(Handle);

    public string? ToJsonPretty() => ObsData.obs_data_get_json_pretty(Handle);

    public bool SaveToFile(string path) => ObsData.obs_data_save_json(Handle, path);

    /// <summary>
    /// Saves settings to a JSON file using atomic write (temp file, then rename).
    /// </summary>
    public bool SaveToFileSafe(string path, string tempExt = ".tmp", string backupExt = ".bak")
        => ObsData.obs_data_save_json_safe(Handle, path, tempExt, backupExt);

    #endregion

    #region Utilities

    public Settings Apply(Settings other)
    {
        ObsData.obs_data_apply(Handle, other.Handle);
        return this;
    }

    public Settings Clear()
    {
        ObsData.obs_data_clear(Handle);
        return this;
    }

    public Settings Remove(string name)
    {
        ObsData.obs_data_erase(Handle, name);
        return this;
    }

    #endregion

    protected override void ReleaseHandle(nint handle)
    {
        ObsData.obs_data_release((ObsDataHandle)handle);
    }

    public override string ToString() => ToJsonPretty() ?? "{}";
}
