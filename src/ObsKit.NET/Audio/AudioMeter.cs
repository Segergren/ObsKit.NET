using System.Runtime.InteropServices;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Audio;

/// <summary>
/// Fader curve types (obs_fader_type).
/// </summary>
public enum FaderType
{
    /// <summary>Simple cubic mapping (x³), common and fast.</summary>
    Cubic = 0,

    /// <summary>IEC 60-268-18 compliant fader with linear dB segments.</summary>
    Iec = 1,

    /// <summary>Logarithmic fader.</summary>
    Log = 2
}

/// <summary>
/// Peak measurement types (obs_peak_meter_type).
/// </summary>
public enum PeakMeterType
{
    /// <summary>Maximum of all samples (fast, less accurate).</summary>
    SamplePeak = 0,

    /// <summary>True peak using inter-sample estimation (accurate, more CPU).</summary>
    TruePeak = 1
}

/// <summary>
/// Audio levels reported by an <see cref="AudioMeter"/>, one value per channel, in dB.
/// Silence is reported as a very large negative value (-∞ equivalent).
/// </summary>
/// <param name="Magnitude">The RMS magnitude per channel in dB.</param>
/// <param name="Peak">The peak per channel in dB.</param>
/// <param name="InputPeak">The pre-fader input peak per channel in dB.</param>
public readonly record struct AudioLevels(
    IReadOnlyList<float> Magnitude,
    IReadOnlyList<float> Peak,
    IReadOnlyList<float> InputPeak);

/// <summary>
/// A volume meter (obs_volmeter) that reports live audio levels for a source.
/// Attach it to any audio source to drive level meters in a UI.
/// </summary>
public sealed class AudioMeter : IDisposable
{
    private readonly nint _volmeter;
    private readonly ObsAudioControls.VolmeterUpdatedCallback _callback;
    private bool _disposed;

    /// <summary>
    /// Creates an audio meter.
    /// </summary>
    /// <param name="faderType">The fader curve used for level mapping.</param>
    public AudioMeter(FaderType faderType = FaderType.Log)
    {
        _volmeter = ObsAudioControls.obs_volmeter_create((int)faderType);
        if (_volmeter == 0)
            throw new InvalidOperationException("Failed to create volume meter.");

        _callback = OnVolmeterUpdated;
        ObsAudioControls.obs_volmeter_add_callback(_volmeter, _callback, 0);
    }

    /// <summary>
    /// Event raised when new audio levels are available (about every 50 ms while
    /// the attached source plays audio). Raised on an OBS audio thread — do not block.
    /// </summary>
    public event Action<AudioMeter, AudioLevels>? LevelsUpdated;

    /// <summary>
    /// Gets the number of audio channels of the attached source.
    /// </summary>
    public int ChannelCount => _disposed ? 0 : ObsAudioControls.obs_volmeter_get_nr_channels(_volmeter);

    /// <summary>
    /// Attaches the meter to a source.
    /// </summary>
    /// <param name="source">The source to meter.</param>
    /// <returns>True if the source was attached.</returns>
    public bool AttachSource(Source source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return ObsAudioControls.obs_volmeter_attach_source(_volmeter, source.Handle);
    }

    /// <summary>
    /// Detaches the meter from the currently attached source.
    /// </summary>
    public void DetachSource()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObsAudioControls.obs_volmeter_detach_source(_volmeter);
    }

    /// <summary>
    /// Sets the peak measurement type.
    /// </summary>
    /// <param name="type">The peak meter type.</param>
    public AudioMeter SetPeakMeterType(PeakMeterType type)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObsAudioControls.obs_volmeter_set_peak_meter_type(_volmeter, (int)type);
        return this;
    }

    /// <summary>
    /// Converts a volume multiplier (e.g. <see cref="Source.Volume"/>) to decibels.
    /// </summary>
    public static float MulToDb(float mul) => ObsAudioControls.obs_mul_to_db(mul);

    /// <summary>
    /// Converts decibels to a volume multiplier (e.g. for <see cref="Source.Volume"/>).
    /// </summary>
    public static float DbToMul(float db) => ObsAudioControls.obs_db_to_mul(db);

    private void OnVolmeterUpdated(nint param, nint magnitude, nint peak, nint inputPeak)
    {
        var handler = LevelsUpdated;
        if (handler == null || _disposed)
            return;

        var channels = Math.Clamp(ChannelCount, 0, ObsAudioControls.MaxAudioChannels);
        if (channels == 0)
            return;

        var magnitudes = new float[channels];
        var peaks = new float[channels];
        var inputPeaks = new float[channels];
        Marshal.Copy(magnitude, magnitudes, 0, channels);
        Marshal.Copy(peak, peaks, 0, channels);
        Marshal.Copy(inputPeak, inputPeaks, 0, channels);

        handler(this, new AudioLevels(magnitudes, peaks, inputPeaks));
    }

    /// <summary>
    /// Detaches the meter and releases the native volmeter.
    /// </summary>
    public void Dispose()
    {
        ReleaseVolmeter();
        GC.SuppressFinalize(this);
    }

    ~AudioMeter()
    {
        ReleaseVolmeter();
    }

    private void ReleaseVolmeter()
    {
        if (_disposed)
            return;

        _disposed = true;
        ObsAudioControls.obs_volmeter_remove_callback(_volmeter, _callback, 0);
        ObsAudioControls.obs_volmeter_detach_source(_volmeter);
        ObsAudioControls.obs_volmeter_destroy(_volmeter);
    }
}
