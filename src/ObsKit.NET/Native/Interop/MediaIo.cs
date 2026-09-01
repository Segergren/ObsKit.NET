using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for libobs media-io utilities: file remuxing, software video scaling and
/// audio resampling. None of these require the OBS core to be started.
/// </summary>
internal static partial class MediaIo
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Remux

    /// <summary>
    /// Progress callback for <c>media_remux_job_process</c>: return 0 to abort.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte RemuxProgressCallback(nint data, float percent);

    public static bool media_remux_job_create(out nint job, string inFilename, string outFilename)
        => media_remux_job_create_native(out job, inFilename, outFilename) != 0;

    [LibraryImport(Lib, EntryPoint = "media_remux_job_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte media_remux_job_create_native(
        out nint job,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string inFilename,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string outFilename);

    public static bool media_remux_job_process(nint job, RemuxProgressCallback callback, nint data)
        => media_remux_job_process_native(job, callback, data) != 0;

    [LibraryImport(Lib, EntryPoint = "media_remux_job_process")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte media_remux_job_process_native(nint job, RemuxProgressCallback callback, nint data);

    [LibraryImport(Lib, EntryPoint = "media_remux_job_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void media_remux_job_destroy(nint job);

    #endregion

    #region Video Scaler

    /// <summary>
    /// Creates a scaler/converter. Returns 0 on success, -1 (bad conversion) or -2 (failed).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "video_scaler_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int video_scaler_create(out nint scaler, in VideoScaleInfo dst, in VideoScaleInfo src, VideoScaleType type);

    [LibraryImport(Lib, EntryPoint = "video_scaler_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void video_scaler_destroy(nint scaler);

    /// <summary>
    /// Scales/converts one frame. All arrays hold MAX_AV_PLANES (8) entries.
    /// </summary>
    public static bool video_scaler_scale(nint scaler, nint output, nint outLinesize, nint input, nint inLinesize)
        => video_scaler_scale_native(scaler, output, outLinesize, input, inLinesize) != 0;

    [LibraryImport(Lib, EntryPoint = "video_scaler_scale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte video_scaler_scale_native(nint scaler, nint output, nint outLinesize, nint input, nint inLinesize);

    #endregion

    #region Audio Resampler

    [LibraryImport(Lib, EntryPoint = "audio_resampler_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint audio_resampler_create(in ResampleInfo dst, in ResampleInfo src);

    [LibraryImport(Lib, EntryPoint = "audio_resampler_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void audio_resampler_destroy(nint resampler);

    /// <summary>
    /// Resamples a block of audio. <paramref name="output"/> receives MAX_AV_PLANES pointers into
    /// the resampler's internal buffer (valid until the next call); <paramref name="input"/>
    /// holds MAX_AV_PLANES input plane pointers.
    /// </summary>
    public static bool audio_resampler_resample(nint resampler, nint output, out uint outFrames, out ulong tsOffset, nint input, uint inFrames)
        => audio_resampler_resample_native(resampler, output, out outFrames, out tsOffset, input, inFrames) != 0;

    [LibraryImport(Lib, EntryPoint = "audio_resampler_resample")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte audio_resampler_resample_native(nint resampler, nint output, out uint outFrames, out ulong tsOffset, nint input, uint inFrames);

    #endregion
}
