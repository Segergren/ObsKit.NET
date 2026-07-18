using System.Runtime.InteropServices;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// Conversions between <see cref="ObsKey"/> values, OBS key names, OS virtual
/// key codes, and human-readable display strings.
/// </summary>
public static class ObsKeys
{
    /// <summary>
    /// Gets a key from its OBS name (e.g. "OBS_KEY_F1").
    /// </summary>
    /// <param name="name">The OBS key name.</param>
    /// <returns>The key, or <see cref="ObsKey.None"/> if the name is unknown.</returns>
    public static ObsKey FromName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return ObsHotkey.obs_key_from_name(name);
    }

    /// <summary>
    /// Gets the OBS name of a key (e.g. "OBS_KEY_F1"), suitable for persisting bindings.
    /// </summary>
    /// <param name="key">The key.</param>
    public static string? ToName(ObsKey key) => ObsHotkey.obs_key_to_name(key);

    /// <summary>
    /// Converts an OS virtual key code (e.g. a Win32 VK_* code from a keyboard hook)
    /// to an OBS key.
    /// </summary>
    /// <param name="virtualKeyCode">The platform virtual key code.</param>
    /// <returns>The key, or <see cref="ObsKey.None"/> if the code has no mapping.</returns>
    public static ObsKey FromVirtualKey(int virtualKeyCode) => ObsHotkey.obs_key_from_virtual_key(virtualKeyCode);

    /// <summary>
    /// Converts an OBS key to the platform's virtual key code.
    /// </summary>
    /// <param name="key">The key.</param>
    public static int ToVirtualKey(ObsKey key) => ObsHotkey.obs_key_to_virtual_key(key);

    /// <summary>
    /// Gets a human-readable, localized display string for a key (e.g. "F1", "Space").
    /// </summary>
    /// <param name="key">The key.</param>
    public static string GetDisplayString(ObsKey key)
    {
        var dstr = default(ObsHotkey.DStrNative);
        ObsHotkey.obs_key_to_str(key, ref dstr);
        return TakeDStr(ref dstr);
    }

    /// <summary>
    /// Gets a human-readable, localized display string for a key combination
    /// (e.g. "Ctrl + Shift + F1").
    /// </summary>
    /// <param name="combination">The key combination.</param>
    public static string GetDisplayString(ObsKeyCombination combination)
    {
        var dstr = default(ObsHotkey.DStrNative);
        ObsHotkey.obs_key_combination_to_str(combination, ref dstr);
        return TakeDStr(ref dstr);
    }

    private static string TakeDStr(ref ObsHotkey.DStrNative dstr)
    {
        if (dstr.Array == nint.Zero)
            return string.Empty;

        try
        {
            return Marshal.PtrToStringUTF8(dstr.Array) ?? string.Empty;
        }
        finally
        {
            ObsSignal.bfree(dstr.Array);
            dstr = default;
        }
    }
}
