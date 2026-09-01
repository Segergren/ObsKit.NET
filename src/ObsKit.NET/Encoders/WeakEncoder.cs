using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// A weak reference to an encoder (obs_weak_encoder_t) that does not keep it alive.
/// Obtain via <see cref="VideoEncoder.GetWeakReference"/> or <see cref="AudioEncoder.GetWeakReference"/>.
/// </summary>
public sealed class WeakEncoder : IDisposable
{
    private nint _weak;
    private bool _disposed;

    internal WeakEncoder(nint weak)
    {
        _weak = weak;
    }

    /// <summary>Gets whether the encoder has been destroyed.</summary>
    public bool IsExpired
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return ObsCore.obs_weak_object_expired(_weak);
        }
    }

    /// <summary>Gets whether this weak reference points at <paramref name="encoder"/>.</summary>
    public bool References(VideoEncoder encoder)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(encoder);
        return ObsEncoder.obs_weak_encoder_references_encoder(_weak, encoder.Handle);
    }

    /// <summary>Gets whether this weak reference points at <paramref name="encoder"/>.</summary>
    public bool References(AudioEncoder encoder)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(encoder);
        return ObsEncoder.obs_weak_encoder_references_encoder(_weak, encoder.Handle);
    }

    /// <summary>
    /// Attempts to get a strong reference to the encoder as a video encoder.
    /// </summary>
    /// <returns>The encoder (dispose when done), or null if it was destroyed or is not a video encoder.</returns>
    public VideoEncoder? TryGetVideoEncoder()
    {
        var handle = Upgrade();
        if (handle.IsNull)
            return null;
        if (ObsEncoder.obs_encoder_get_type(handle) != ObsEncoderType.Video)
        {
            ObsEncoder.obs_encoder_release(handle);
            return null;
        }
        return new VideoEncoder(handle, ownsHandle: true);
    }

    /// <summary>
    /// Attempts to get a strong reference to the encoder as an audio encoder.
    /// </summary>
    /// <returns>The encoder (dispose when done), or null if it was destroyed or is not an audio encoder.</returns>
    public AudioEncoder? TryGetAudioEncoder()
    {
        var handle = Upgrade();
        if (handle.IsNull)
            return null;
        if (ObsEncoder.obs_encoder_get_type(handle) != ObsEncoderType.Audio)
        {
            ObsEncoder.obs_encoder_release(handle);
            return null;
        }
        return new AudioEncoder(handle, mixerIdx: (int)ObsEncoder.obs_encoder_get_mixer_index(handle), ownsHandle: true);
    }

    private ObsEncoderHandle Upgrade()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return ObsEncoder.obs_weak_encoder_get_encoder(_weak);
    }

    /// <summary>Releases the weak reference.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~WeakEncoder()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        if (_weak != nint.Zero)
            ObsEncoder.obs_weak_encoder_release(_weak);
        _weak = nint.Zero;
    }
}
