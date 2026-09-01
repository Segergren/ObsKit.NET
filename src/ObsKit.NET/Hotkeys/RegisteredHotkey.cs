using ObsKit.NET.Core;
using System.Runtime.InteropServices;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Hotkeys;

/// <summary>
/// A hotkey registered by this application (see <c>Obs.RegisterHotkey</c> and
/// <c>Source.RegisterHotkey</c>). Bind key combinations with <see cref="Bind"/>;
/// libobs polls global key state on its own hotkey thread, so bound combinations
/// fire system-wide with no extra plumbing. You can also feed custom key events via
/// <c>Obs.InjectHotkeyEvent</c> or fire the action directly with <c>Obs.TriggerHotkey</c>.
/// Keep this object alive for as long as the hotkey should work; dispose it to unregister.
/// </summary>
public sealed class RegisteredHotkey : IDisposable
{
    // Rooted for the registration's lifetime — libobs holds the function pointer.
    private readonly ObsHotkey.HotkeyCallback _nativeCallback;
    private bool _disposed;

    internal RegisteredHotkey(string name, string description, Action<bool> handler, Func<ObsHotkey.HotkeyCallback, nuint> register)
    {
        Name = name;
        Description = description;
        _nativeCallback = (data, id, hotkey, pressed) =>
        {
            try
            {
                handler(pressed != 0);
            }
            catch
            {
                // Don't let exceptions escape into native code
            }
        };

        Id = register(_nativeCallback);
        if (Id == nuint.MaxValue) // OBS_INVALID_HOTKEY_ID
            throw new InvalidOperationException($"Failed to register hotkey '{name}'.");
    }

    /// <summary>Gets the hotkey id.</summary>
    public ulong Id { get; }

    /// <summary>Gets the internal hotkey name.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the user-facing hotkey description.</summary>
    public string Description { get; private set; }

    /// <summary>
    /// Replaces the key combinations bound to this hotkey. Pass none to clear all bindings.
    /// </summary>
    /// <param name="combinations">The key combinations that should trigger the hotkey.</param>
    public void Bind(params ObsKeyCombination[] combinations)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        BindCore((nuint)Id, combinations);
    }

    /// <summary>Removes all key combinations bound to this hotkey.</summary>
    public void ClearBindings() => Bind();

    /// <summary>
    /// Gets the key combinations currently bound to this hotkey.
    /// </summary>
    public IReadOnlyList<ObsKeyCombination> GetBindings()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return GetBindingsForId(Id);
    }

    internal static void BindCore(nuint id, ObsKeyCombination[] combinations)
    {
        if (combinations.Length == 0)
        {
            ObsHotkey.obs_hotkey_load_bindings(id, nint.Zero, 0);
            return;
        }

        var pinned = GCHandle.Alloc(combinations, GCHandleType.Pinned);
        try
        {
            ObsHotkey.obs_hotkey_load_bindings(id, pinned.AddrOfPinnedObject(), (nuint)combinations.Length);
        }
        finally
        {
            pinned.Free();
        }
    }

    internal static IReadOnlyList<ObsKeyCombination> GetBindingsForId(ulong id)
    {
        var result = new List<ObsKeyCombination>();
        ObsHotkey.EnumHotkeyBindingCallback callback = (_, idx, binding) =>
        {
            if (ObsHotkey.obs_hotkey_binding_get_hotkey_id(binding) == (nuint)id)
                result.Add(ObsHotkey.obs_hotkey_binding_get_key_combination(binding));
            return 1;
        };
        ObsHotkey.obs_enum_hotkey_bindings(callback, nint.Zero);
        GC.KeepAlive(callback);
        return result;
    }

    /// <summary>
    /// Unregisters the hotkey. If the object is dropped without being disposed, the
    /// finalizer unregisters it rather than leaving libobs holding a dangling native
    /// function pointer (which would crash on the next matching key event).
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~RegisteredHotkey()
    {
        Dispose(disposing: false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        // obs_hotkey_unregister no-ops (with an internal null-check) after obs_shutdown,
        // so this is safe from the finalizer even during teardown.
        ObsHotkey.obs_hotkey_unregister((nuint)Id);
        GC.KeepAlive(_nativeCallback);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Hotkey: {Name} ({Description})";

    /// <summary>
    /// Saves the current bindings in OBS's hotkey JSON format (one object per binding with a
    /// "key" name and modifier flags), suitable for storing in a settings file and restoring
    /// with <see cref="LoadBindings"/>. Dispose the array when done.
    /// </summary>
    public SettingsArray SaveBindings()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return SaveBindingsForId(Id);
    }

    /// <summary>
    /// Replaces the bindings from an array produced by <see cref="SaveBindings"/> (or by OBS's
    /// own hotkey settings).
    /// </summary>
    public void LoadBindings(SettingsArray bindings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(bindings);
        ObsHotkey.obs_hotkey_load((nuint)Id, bindings.Handle);
    }

    /// <summary>
    /// Changes the internal name and/or user-facing description after registration.
    /// </summary>
    public void Rename(string? name = null, string? description = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (name != null)
        {
            ObsHotkey.obs_hotkey_set_name((nuint)Id, name);
            Name = name;
        }
        if (description != null)
        {
            ObsHotkey.obs_hotkey_set_description((nuint)Id, description);
            Description = description;
        }
    }

    internal static SettingsArray SaveBindingsForId(ulong id)
    {
        var array = ObsHotkey.obs_hotkey_save((nuint)id);
        if (array.IsNull)
            throw new InvalidOperationException("Failed to save hotkey bindings.");
        return new SettingsArray(array, ownsHandle: true);
    }
}

/// <summary>
/// A pair of mutually-exclusive application hotkeys (e.g. start/stop), registered via
/// <c>Obs.RegisterHotkeyPair</c>. When both hotkeys share the same key combination,
/// only the handler whose callback returns true consumes the press, so one key can
/// toggle between the two actions. Dispose to unregister both.
/// </summary>
public sealed class RegisteredHotkeyPair : IDisposable
{
    private readonly ObsHotkey.HotkeyActiveCallback _nativeCallback0;
    private readonly ObsHotkey.HotkeyActiveCallback _nativeCallback1;
    private bool _disposed;

    internal RegisteredHotkeyPair(
        string primaryName, string primaryDescription,
        string secondaryName, string secondaryDescription,
        Func<bool, bool> primaryHandler, Func<bool, bool> secondaryHandler)
    {
        PrimaryName = primaryName;
        SecondaryName = secondaryName;

        _nativeCallback0 = (data, id, hotkey, pressed) => Invoke(primaryHandler, pressed);
        _nativeCallback1 = (data, id, hotkey, pressed) => Invoke(secondaryHandler, pressed);

        Id = ObsHotkey.obs_hotkey_pair_register_frontend(
            primaryName, primaryDescription,
            secondaryName, secondaryDescription,
            _nativeCallback0, _nativeCallback1,
            nint.Zero, nint.Zero);

        if (Id == nuint.MaxValue) // OBS_INVALID_HOTKEY_PAIR_ID
            throw new InvalidOperationException($"Failed to register hotkey pair '{primaryName}'/'{secondaryName}'.");
    }

    private static byte Invoke(Func<bool, bool> handler, byte pressed)
    {
        try
        {
            return handler(pressed != 0) ? (byte)1 : (byte)0;
        }
        catch
        {
            // Don't let exceptions escape into native code
            return 0;
        }
    }

    /// <summary>Gets the hotkey pair id.</summary>
    public ulong Id { get; }

    /// <summary>Gets the internal name of the primary (first) hotkey.</summary>
    public string PrimaryName { get; private set; }

    /// <summary>Gets the internal name of the secondary (second) hotkey.</summary>
    public string SecondaryName { get; private set; }

    /// <summary>
    /// Replaces the key combinations bound to the primary hotkey. Pass none to clear.
    /// </summary>
    /// <param name="combinations">The key combinations that should trigger the hotkey.</param>
    public void BindPrimary(params ObsKeyCombination[] combinations) => BindByName(PrimaryName, combinations);

    /// <summary>
    /// Replaces the key combinations bound to the secondary hotkey. Pass none to clear.
    /// </summary>
    /// <param name="combinations">The key combinations that should trigger the hotkey.</param>
    public void BindSecondary(params ObsKeyCombination[] combinations) => BindByName(SecondaryName, combinations);

    private void BindByName(string name, ObsKeyCombination[] combinations)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // The pair id is not a hotkey id — resolve the underlying hotkey by its name.
        nuint? found = null;
        ObsHotkey.EnumHotkeyCallback callback = (_, id, key) =>
        {
            if (ObsHotkey.obs_hotkey_get_name(key) != name)
                return 1;
            found = id;
            return 0;
        };
        ObsHotkey.obs_enum_hotkeys(callback, nint.Zero);
        GC.KeepAlive(callback);

        if (found == null)
            throw new InvalidOperationException($"Hotkey '{name}' not found.");

        RegisteredHotkey.BindCore(found.Value, combinations);
    }

    /// <summary>
    /// Unregisters both hotkeys of the pair (see <see cref="RegisteredHotkey.Dispose()"/>
    /// for lifetime notes).
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~RegisteredHotkeyPair()
    {
        Dispose(disposing: false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        ObsHotkey.obs_hotkey_pair_unregister((nuint)Id);
        GC.KeepAlive(_nativeCallback0);
        GC.KeepAlive(_nativeCallback1);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Hotkey pair: {PrimaryName}/{SecondaryName}";

    internal RegisteredHotkeyPair(
        string primaryName, string primaryDescription,
        string secondaryName, string secondaryDescription,
        Func<bool, bool> primaryHandler, Func<bool, bool> secondaryHandler,
        Func<ObsHotkey.HotkeyActiveCallback, ObsHotkey.HotkeyActiveCallback, nuint> register)
    {
        PrimaryName = primaryName;
        SecondaryName = secondaryName;

        _nativeCallback0 = (data, id, hotkey, pressed) => Invoke(primaryHandler, pressed);
        _nativeCallback1 = (data, id, hotkey, pressed) => Invoke(secondaryHandler, pressed);

        Id = register(_nativeCallback0, _nativeCallback1);
        if (Id == nuint.MaxValue) // OBS_INVALID_HOTKEY_PAIR_ID
            throw new InvalidOperationException($"Failed to register hotkey pair '{primaryName}'/'{secondaryName}'.");
    }

    /// <summary>
    /// Saves the bindings of both hotkeys in OBS's hotkey JSON format (see
    /// <see cref="RegisteredHotkey.SaveBindings"/>). Dispose both arrays when done.
    /// </summary>
    public (SettingsArray Primary, SettingsArray Secondary) SaveBindings()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObsHotkey.obs_hotkey_pair_save((nuint)Id, out var d0, out var d1);
        if (d0.IsNull || d1.IsNull)
            throw new InvalidOperationException("Failed to save hotkey pair bindings.");
        return (new SettingsArray(d0, ownsHandle: true), new SettingsArray(d1, ownsHandle: true));
    }

    /// <summary>
    /// Replaces the bindings of both hotkeys from arrays produced by <see cref="SaveBindings"/>.
    /// </summary>
    public void LoadBindings(SettingsArray primary, SettingsArray secondary)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(secondary);
        ObsHotkey.obs_hotkey_pair_load((nuint)Id, primary.Handle, secondary.Handle);
    }

    /// <summary>
    /// Changes the internal names of both hotkeys after registration.
    /// </summary>
    public void Rename(string primaryName, string secondaryName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrEmpty(primaryName);
        ArgumentException.ThrowIfNullOrEmpty(secondaryName);
        ObsHotkey.obs_hotkey_pair_set_names((nuint)Id, primaryName, secondaryName);
        PrimaryName = primaryName;
        SecondaryName = secondaryName;
    }

    /// <summary>
    /// Changes the user-facing descriptions of both hotkeys after registration.
    /// </summary>
    public void SetDescriptions(string primaryDescription, string secondaryDescription)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(primaryDescription);
        ArgumentNullException.ThrowIfNull(secondaryDescription);
        ObsHotkey.obs_hotkey_pair_set_descriptions((nuint)Id, primaryDescription, secondaryDescription);
    }
}
