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
    /// Converts an OS virtual key code to an OBS key using the <em>host platform's</em> native
    /// mapping. The meaning of <paramref name="virtualKeyCode"/> therefore depends on the running
    /// OS: a Win32 VK_* code on Windows, an X11 keysym on Linux, a Carbon virtual key on macOS.
    /// If you hold Win32 VK codes on every OS (e.g. a browser's <c>KeyboardEvent.keyCode</c>),
    /// use <see cref="FromWindowsVirtualKey"/> instead, which is host-independent.
    /// </summary>
    /// <param name="virtualKeyCode">The host platform's virtual key code.</param>
    /// <returns>The key, or <see cref="ObsKey.None"/> if the code has no mapping.</returns>
    public static ObsKey FromVirtualKey(int virtualKeyCode) => ObsHotkey.obs_key_from_virtual_key(virtualKeyCode);

    /// <summary>
    /// Converts a <strong>Win32 virtual-key code</strong> (a <c>VK_*</c> value) to an OBS key,
    /// independent of the host OS. Use this when your key codes are always Win32 values regardless
    /// of platform - most commonly a browser's <c>KeyboardEvent.keyCode</c>. Unlike
    /// <see cref="FromVirtualKey"/> this uses no platform-specific native call, so e.g. <c>0x78</c>
    /// resolves to <see cref="ObsKey.F9"/> on Linux and macOS just as it does on Windows.
    /// </summary>
    /// <param name="windowsVirtualKeyCode">The Win32 VK_* code.</param>
    /// <returns>The key, or <see cref="ObsKey.None"/> if the code has no mapping.</returns>
    public static ObsKey FromWindowsVirtualKey(int windowsVirtualKeyCode)
    {
        int vk = windowsVirtualKeyCode;

        if (vk >= 0x41 && vk <= 0x5A) return ObsKey.A + (vk - 0x41);    // A–Z
        if (vk >= 0x30 && vk <= 0x39) return ObsKey.D0 + (vk - 0x30);   // 0–9 (top row)
        if (vk >= 0x70 && vk <= 0x87) return ObsKey.F1 + (vk - 0x70);   // F1–F24
        if (vk >= 0x60 && vk <= 0x69) return ObsKey.Num0 + (vk - 0x60); // numpad 0–9

        return vk switch
        {
            0x08 => ObsKey.Backspace,
            0x09 => ObsKey.Tab,
            0x0D => ObsKey.Return,
            0x10 => ObsKey.Shift,
            0x11 => ObsKey.Control,
            0x12 => ObsKey.Alt,
            0x13 => ObsKey.Pause,
            0x14 => ObsKey.Capslock,
            0x1B => ObsKey.Escape,
            0x20 => ObsKey.Space,
            0x21 => ObsKey.Pageup,
            0x22 => ObsKey.Pagedown,
            0x23 => ObsKey.End,
            0x24 => ObsKey.Home,
            0x25 => ObsKey.Left,
            0x26 => ObsKey.Up,
            0x27 => ObsKey.Right,
            0x28 => ObsKey.Down,
            0x2C => ObsKey.Print,
            0x2D => ObsKey.Insert,
            0x2E => ObsKey.Delete,
            0x5B or 0x5C => ObsKey.Meta, // VK_LWIN / VK_RWIN
            0x6A => ObsKey.Numasterisk,
            0x6B => ObsKey.Numplus,
            0x6D => ObsKey.Numminus,
            0x6E => ObsKey.Numperiod,
            0x6F => ObsKey.Numslash,
            0x90 => ObsKey.Numlock,
            0x91 => ObsKey.Scrolllock,
            0xBA => ObsKey.Semicolon,    // VK_OEM_1   ;:
            0xBB => ObsKey.Plus,         // VK_OEM_PLUS  =+
            0xBC => ObsKey.Comma,        // VK_OEM_COMMA  ,<
            0xBD => ObsKey.Minus,        // VK_OEM_MINUS  -_
            0xBE => ObsKey.Period,       // VK_OEM_PERIOD  .>
            0xBF => ObsKey.Slash,        // VK_OEM_2   /?
            0xC0 => ObsKey.Asciitilde,   // VK_OEM_3   `~
            0xDB => ObsKey.Bracketleft,  // VK_OEM_4   [{
            0xDC => ObsKey.Backslash,    // VK_OEM_5   \|
            0xDD => ObsKey.Bracketright, // VK_OEM_6   ]}
            0xDE => ObsKey.Apostrophe,   // VK_OEM_7   '"
            _ => ObsKey.None
        };
    }

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
