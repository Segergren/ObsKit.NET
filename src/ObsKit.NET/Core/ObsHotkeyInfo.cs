namespace ObsKit.NET.Core;

/// <summary>
/// What kind of object registered a hotkey (obs_hotkey_registerer_type).
/// </summary>
public enum ObsHotkeyRegistererType
{
    /// <summary>Registered by the frontend application.</summary>
    Frontend = 0,
    /// <summary>Registered by a source.</summary>
    Source,
    /// <summary>Registered by an output.</summary>
    Output,
    /// <summary>Registered by an encoder.</summary>
    Encoder,
    /// <summary>Registered by a service.</summary>
    Service,
}

/// <summary>
/// Describes a hotkey registered with OBS.
/// </summary>
/// <param name="Id">The hotkey id (valid until the registering object is destroyed).</param>
/// <param name="Name">The internal hotkey name (e.g. "hotkey_start").</param>
/// <param name="Description">The localized description.</param>
/// <param name="RegistererType">What kind of object registered the hotkey.</param>
public sealed record ObsHotkeyInfo(ulong Id, string Name, string? Description, ObsHotkeyRegistererType RegistererType);
