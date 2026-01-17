namespace ObsKit.NET.Native.Types;

/// <summary>
/// Strongly-typed handle for obs_source_t to prevent accidental misuse.
/// </summary>
public readonly struct ObsSourceHandle : IEquatable<ObsSourceHandle>
{
    public readonly nint Value;

    public ObsSourceHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsSourceHandle Null => default;

    public static implicit operator nint(ObsSourceHandle handle) => handle.Value;
    public static explicit operator ObsSourceHandle(nint ptr) => new(ptr);

    public bool Equals(ObsSourceHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsSourceHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsSourceHandle left, ObsSourceHandle right) => left.Equals(right);
    public static bool operator !=(ObsSourceHandle left, ObsSourceHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsSource(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_scene_t.
/// </summary>
public readonly struct ObsSceneHandle : IEquatable<ObsSceneHandle>
{
    public readonly nint Value;

    public ObsSceneHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsSceneHandle Null => default;

    public static implicit operator nint(ObsSceneHandle handle) => handle.Value;
    public static explicit operator ObsSceneHandle(nint ptr) => new(ptr);

    public bool Equals(ObsSceneHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsSceneHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsSceneHandle left, ObsSceneHandle right) => left.Equals(right);
    public static bool operator !=(ObsSceneHandle left, ObsSceneHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsScene(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_sceneitem_t.
/// </summary>
public readonly struct ObsSceneItemHandle : IEquatable<ObsSceneItemHandle>
{
    public readonly nint Value;

    public ObsSceneItemHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsSceneItemHandle Null => default;

    public static implicit operator nint(ObsSceneItemHandle handle) => handle.Value;
    public static explicit operator ObsSceneItemHandle(nint ptr) => new(ptr);

    public bool Equals(ObsSceneItemHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsSceneItemHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsSceneItemHandle left, ObsSceneItemHandle right) => left.Equals(right);
    public static bool operator !=(ObsSceneItemHandle left, ObsSceneItemHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsSceneItem(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_output_t.
/// </summary>
public readonly struct ObsOutputHandle : IEquatable<ObsOutputHandle>
{
    public readonly nint Value;

    public ObsOutputHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsOutputHandle Null => default;

    public static implicit operator nint(ObsOutputHandle handle) => handle.Value;
    public static explicit operator ObsOutputHandle(nint ptr) => new(ptr);

    public bool Equals(ObsOutputHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsOutputHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsOutputHandle left, ObsOutputHandle right) => left.Equals(right);
    public static bool operator !=(ObsOutputHandle left, ObsOutputHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsOutput(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_encoder_t.
/// </summary>
public readonly struct ObsEncoderHandle : IEquatable<ObsEncoderHandle>
{
    public readonly nint Value;

    public ObsEncoderHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsEncoderHandle Null => default;

    public static implicit operator nint(ObsEncoderHandle handle) => handle.Value;
    public static explicit operator ObsEncoderHandle(nint ptr) => new(ptr);

    public bool Equals(ObsEncoderHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsEncoderHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsEncoderHandle left, ObsEncoderHandle right) => left.Equals(right);
    public static bool operator !=(ObsEncoderHandle left, ObsEncoderHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsEncoder(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_data_t.
/// </summary>
public readonly struct ObsDataHandle : IEquatable<ObsDataHandle>
{
    public readonly nint Value;

    public ObsDataHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsDataHandle Null => default;

    public static implicit operator nint(ObsDataHandle handle) => handle.Value;
    public static explicit operator ObsDataHandle(nint ptr) => new(ptr);

    public bool Equals(ObsDataHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsDataHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsDataHandle left, ObsDataHandle right) => left.Equals(right);
    public static bool operator !=(ObsDataHandle left, ObsDataHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsData(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_data_array_t.
/// </summary>
public readonly struct ObsDataArrayHandle : IEquatable<ObsDataArrayHandle>
{
    public readonly nint Value;

    public ObsDataArrayHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsDataArrayHandle Null => default;

    public static implicit operator nint(ObsDataArrayHandle handle) => handle.Value;
    public static explicit operator ObsDataArrayHandle(nint ptr) => new(ptr);

    public bool Equals(ObsDataArrayHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsDataArrayHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsDataArrayHandle left, ObsDataArrayHandle right) => left.Equals(right);
    public static bool operator !=(ObsDataArrayHandle left, ObsDataArrayHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsDataArray(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for obs_service_t.
/// </summary>
public readonly struct ObsServiceHandle : IEquatable<ObsServiceHandle>
{
    public readonly nint Value;

    public ObsServiceHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ObsServiceHandle Null => default;

    public static implicit operator nint(ObsServiceHandle handle) => handle.Value;
    public static explicit operator ObsServiceHandle(nint ptr) => new(ptr);

    public bool Equals(ObsServiceHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ObsServiceHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ObsServiceHandle left, ObsServiceHandle right) => left.Equals(right);
    public static bool operator !=(ObsServiceHandle left, ObsServiceHandle right) => !left.Equals(right);
    public override string ToString() => $"ObsService(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for signal_handler_t.
/// </summary>
public readonly struct SignalHandlerHandle : IEquatable<SignalHandlerHandle>
{
    public readonly nint Value;

    public SignalHandlerHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static SignalHandlerHandle Null => default;

    public static implicit operator nint(SignalHandlerHandle handle) => handle.Value;
    public static explicit operator SignalHandlerHandle(nint ptr) => new(ptr);

    public bool Equals(SignalHandlerHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is SignalHandlerHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(SignalHandlerHandle left, SignalHandlerHandle right) => left.Equals(right);
    public static bool operator !=(SignalHandlerHandle left, SignalHandlerHandle right) => !left.Equals(right);
    public override string ToString() => $"SignalHandler(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for proc_handler_t.
/// </summary>
public readonly struct ProcHandlerHandle : IEquatable<ProcHandlerHandle>
{
    public readonly nint Value;

    public ProcHandlerHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static ProcHandlerHandle Null => default;

    public static implicit operator nint(ProcHandlerHandle handle) => handle.Value;
    public static explicit operator ProcHandlerHandle(nint ptr) => new(ptr);

    public bool Equals(ProcHandlerHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ProcHandlerHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ProcHandlerHandle left, ProcHandlerHandle right) => left.Equals(right);
    public static bool operator !=(ProcHandlerHandle left, ProcHandlerHandle right) => !left.Equals(right);
    public override string ToString() => $"ProcHandler(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for video_t.
/// </summary>
public readonly struct VideoHandle : IEquatable<VideoHandle>
{
    public readonly nint Value;

    public VideoHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static VideoHandle Null => default;

    public static implicit operator nint(VideoHandle handle) => handle.Value;
    public static explicit operator VideoHandle(nint ptr) => new(ptr);

    public bool Equals(VideoHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is VideoHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(VideoHandle left, VideoHandle right) => left.Equals(right);
    public static bool operator !=(VideoHandle left, VideoHandle right) => !left.Equals(right);
    public override string ToString() => $"Video(0x{Value:X})";
}

/// <summary>
/// Strongly-typed handle for audio_t.
/// </summary>
public readonly struct AudioHandle : IEquatable<AudioHandle>
{
    public readonly nint Value;

    public AudioHandle(nint value) => Value = value;

    public bool IsNull => Value == 0;
    public static AudioHandle Null => default;

    public static implicit operator nint(AudioHandle handle) => handle.Value;
    public static explicit operator AudioHandle(nint ptr) => new(ptr);

    public bool Equals(AudioHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is AudioHandle h && Equals(h);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(AudioHandle left, AudioHandle right) => left.Equals(right);
    public static bool operator !=(AudioHandle left, AudioHandle right) => !left.Equals(right);
    public override string ToString() => $"Audio(0x{Value:X})";
}
