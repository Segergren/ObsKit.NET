using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Video;

/// <summary>
/// Remuxes a media file into another container without re-encoding (e.g. MKV to MP4 after a
/// crash-safe recording), using libobs's built-in FFmpeg remuxer. Does not require the OBS
/// core to be started, only the libobs binary to be present.
/// </summary>
public static class MediaRemux
{
    /// <summary>
    /// Remuxes <paramref name="inputPath"/> into <paramref name="outputPath"/> (container chosen
    /// from the output extension), blocking until done.
    /// </summary>
    /// <param name="inputPath">The source file.</param>
    /// <param name="outputPath">The destination file (overwritten).</param>
    /// <param name="progress">Receives progress in percent (0-100).</param>
    /// <param name="cancellationToken">Aborts the remux; the partial output is left on disk.</param>
    /// <returns>True if the whole file was remuxed; false on failure or cancellation.</returns>
    /// <exception cref="FileNotFoundException">The input file does not exist.</exception>
    /// <exception cref="InvalidOperationException">The input could not be opened or the output could not be created.</exception>
    public static bool Remux(string inputPath, string outputPath, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputPath);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input file not found.", inputPath);

        LibraryLoader.Initialize();

        if (!MediaIo.media_remux_job_create(out var job, inputPath, outputPath) || job == nint.Zero)
            throw new InvalidOperationException($"Failed to open '{inputPath}' for remuxing into '{outputPath}'.");

        try
        {
            MediaIo.RemuxProgressCallback callback = (_, percent) =>
            {
                try
                {
                    progress?.Report(percent);
                }
                catch
                {
                    // Never let exceptions cross the native boundary.
                }
                return cancellationToken.IsCancellationRequested ? (byte)0 : (byte)1;
            };
            var ok = MediaIo.media_remux_job_process(job, callback, nint.Zero);
            GC.KeepAlive(callback);
            return ok && !cancellationToken.IsCancellationRequested;
        }
        finally
        {
            MediaIo.media_remux_job_destroy(job);
        }
    }

    /// <summary>
    /// Runs <see cref="Remux"/> on a thread-pool thread.
    /// </summary>
    public static Task<bool> RemuxAsync(string inputPath, string outputPath, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        => Task.Run(() => Remux(inputPath, outputPath, progress, cancellationToken), CancellationToken.None);
}
