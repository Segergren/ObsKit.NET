using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Groups encoders so that they start in lockstep (obs_encoder_group_t): when an output
/// starts one member, libobs holds its first frames until every member has started, which
/// keeps multi-track audio/video aligned. Used by OBS for multitrack (enhanced) streaming.
/// The group holds a strong reference to each member; dispose it to release them. Members
/// cannot be added or removed while any of them is active.
/// </summary>
public sealed class EncoderGroup : IDisposable
{
    private nint _group;
    private bool _disposed;

    /// <summary>
    /// Creates an empty encoder group.
    /// </summary>
    public EncoderGroup()
    {
        if (!Obs.IsInitialized)
            throw new Exceptions.ObsNotInitializedException();

        _group = ObsEncoder.obs_encoder_group_create();
        if (_group == nint.Zero)
            throw new InvalidOperationException("Failed to create encoder group.");
    }

    /// <summary>Adds a video encoder to the group (moving it out of any previous group).</summary>
    /// <returns>False if the encoder or the group has active encoders.</returns>
    public bool Add(VideoEncoder encoder) => Add(encoder?.Handle ?? throw new ArgumentNullException(nameof(encoder)));

    /// <summary>Adds an audio encoder to the group (moving it out of any previous group).</summary>
    /// <returns>False if the encoder or the group has active encoders.</returns>
    public bool Add(AudioEncoder encoder) => Add(encoder?.Handle ?? throw new ArgumentNullException(nameof(encoder)));

    /// <summary>Removes a video encoder from its group.</summary>
    /// <returns>False if the group has active encoders.</returns>
    public bool Remove(VideoEncoder encoder) => Remove(encoder?.Handle ?? throw new ArgumentNullException(nameof(encoder)));

    /// <summary>Removes an audio encoder from its group.</summary>
    /// <returns>False if the group has active encoders.</returns>
    public bool Remove(AudioEncoder encoder) => Remove(encoder?.Handle ?? throw new ArgumentNullException(nameof(encoder)));

    private bool Add(ObsEncoderHandle encoder)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return ObsEncoder.obs_encoder_set_group(encoder, _group);
    }

    private bool Remove(ObsEncoderHandle encoder)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return ObsEncoder.obs_encoder_set_group(encoder, nint.Zero);
    }

    /// <summary>
    /// Destroys the group and releases its encoder references. If members are still active,
    /// libobs defers the destruction until they have all stopped.
    /// </summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~EncoderGroup()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // Destroying releases the member encoders, which is a use-after-free once the core
        // has shut down and freed them.
        if (_group != nint.Zero && Obs.IsInitialized)
            ObsEncoder.obs_encoder_group_destroy(_group);
        _group = nint.Zero;
    }
}
