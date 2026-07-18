using ObsKit.NET.Native.Interop;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Audio;

/// <summary>
/// A volume fader (obs_fader_t) that maps between a UI slider position (deflection,
/// 0.0–1.0), decibels, and a linear multiplier using a chosen curve. Attach it to a
/// source to drive that source's volume from a slider, or use it standalone for
/// curve-aware conversions.
/// </summary>
public sealed class VolumeFader : IDisposable
{
    private readonly nint _fader;
    private readonly object _changedLock = new();
    private ObsAudioControls.FaderChangedCallback? _nativeChanged;
    private Action<float>? _changed;
    private bool _disposed;

    /// <summary>
    /// Creates a volume fader.
    /// </summary>
    /// <param name="faderType">The mapping curve (defaults to the cubic curve used by OBS UI volume sliders).</param>
    public VolumeFader(FaderType faderType = FaderType.Cubic)
    {
        _fader = ObsAudioControls.obs_fader_create((int)faderType);
        if (_fader == 0)
            throw new InvalidOperationException("Failed to create volume fader.");
    }

    /// <summary>
    /// Gets or sets the fader level in decibels. 0 dB is unity gain.
    /// </summary>
    public float Db
    {
        get { ObjectDisposedException.ThrowIf(_disposed, this); return ObsAudioControls.obs_fader_get_db(_fader); }
        set { ObjectDisposedException.ThrowIf(_disposed, this); ObsAudioControls.obs_fader_set_db(_fader, value); }
    }

    /// <summary>
    /// Gets or sets the slider deflection (0.0–1.0), mapped through the fader curve.
    /// </summary>
    public float Deflection
    {
        get { ObjectDisposedException.ThrowIf(_disposed, this); return ObsAudioControls.obs_fader_get_deflection(_fader); }
        set { ObjectDisposedException.ThrowIf(_disposed, this); ObsAudioControls.obs_fader_set_deflection(_fader, value); }
    }

    /// <summary>
    /// Gets or sets the linear volume multiplier (the same scale as <see cref="Source.Volume"/>).
    /// </summary>
    public float Multiplier
    {
        get { ObjectDisposedException.ThrowIf(_disposed, this); return ObsAudioControls.obs_fader_get_mul(_fader); }
        set { ObjectDisposedException.ThrowIf(_disposed, this); ObsAudioControls.obs_fader_set_mul(_fader, value); }
    }

    /// <summary>
    /// Attaches the fader to a source. While attached, changing <see cref="Db"/>,
    /// <see cref="Deflection"/>, or <see cref="Multiplier"/> updates the source's volume,
    /// and the fader tracks external volume changes to the source.
    /// </summary>
    /// <param name="source">The source to control.</param>
    /// <returns>True if the source was attached.</returns>
    public bool AttachSource(Source source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(source);
        return ObsAudioControls.obs_fader_attach_source(_fader, source.Handle);
    }

    /// <summary>
    /// Detaches the fader from the currently attached source.
    /// </summary>
    public void DetachSource()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObsAudioControls.obs_fader_detach_source(_fader);
    }

    /// <summary>
    /// Raised with the new level in decibels when the fader's level changes — including
    /// when the attached source's volume is changed elsewhere (another fader, or
    /// <see cref="Source.Volume"/> directly). Invoked on the thread that changed the
    /// volume; keep handlers fast.
    /// </summary>
    public event Action<float> Changed
    {
        add
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            lock (_changedLock)
            {
                if (_nativeChanged == null)
                {
                    ObsAudioControls.FaderChangedCallback native = (_, db) =>
                    {
                        try
                        {
                            _changed?.Invoke(db);
                        }
                        catch
                        {
                            // Don't let exceptions escape into native code
                        }
                    };
                    ObsAudioControls.obs_fader_add_callback(_fader, native, nint.Zero);
                    _nativeChanged = native;
                }
                _changed += value;
            }
        }
        remove
        {
            lock (_changedLock)
            {
                _changed -= value;
            }
        }
    }

    /// <summary>
    /// Detaches the fader and releases the native resource.
    /// </summary>
    public void Dispose()
    {
        ReleaseFader();
        GC.SuppressFinalize(this);
    }

    ~VolumeFader()
    {
        ReleaseFader();
    }

    private void ReleaseFader()
    {
        if (_disposed)
            return;

        _disposed = true;
        // Remove the changed callback before destroying so the native side cannot
        // invoke a collected delegate thunk between the two calls.
        if (_nativeChanged != null)
            ObsAudioControls.obs_fader_remove_callback(_fader, _nativeChanged, nint.Zero);
        ObsAudioControls.obs_fader_detach_source(_fader);
        ObsAudioControls.obs_fader_destroy(_fader);
        GC.KeepAlive(_nativeChanged);
    }
}
