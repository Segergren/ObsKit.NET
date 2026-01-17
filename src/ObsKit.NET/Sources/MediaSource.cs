using ObsKit.NET.Core;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents a media source for playing video and audio files.
/// </summary>
public sealed class MediaSource : Source
{
    /// <summary>
    /// The source type ID for media source.
    /// </summary>
    public const string SourceTypeId = "ffmpeg_source";

    /// <summary>
    /// Creates a media source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="localFile">Path to the media file.</param>
    /// <param name="loop">Whether to loop the media.</param>
    public MediaSource(string name, string? localFile = null, bool loop = false)
        : base(SourceTypeId, name)
    {
        ApplySettings(localFile, null, loop);
    }

    /// <summary>
    /// Creates a media source from a local file.
    /// </summary>
    /// <param name="filePath">Path to the media file.</param>
    /// <param name="name">Optional source name.</param>
    /// <param name="loop">Whether to loop the media.</param>
    /// <returns>A media source.</returns>
    public static MediaSource FromFile(string filePath, string? name = null, bool loop = false)
    {
        return new MediaSource(name ?? Path.GetFileName(filePath), filePath, loop);
    }

    /// <summary>
    /// Creates a media source from a URL.
    /// </summary>
    /// <param name="url">URL of the media.</param>
    /// <param name="name">Optional source name.</param>
    /// <param name="loop">Whether to loop the media.</param>
    /// <returns>A media source.</returns>
    public static MediaSource FromUrl(string url, string? name = null, bool loop = false)
    {
        var source = new MediaSource(name ?? "Media Source");
        source.SetUrl(url);
        source.SetLoop(loop);
        return source;
    }

    private void ApplySettings(string? localFile, string? url, bool loop)
    {
        Update(s =>
        {
            if (!string.IsNullOrEmpty(localFile))
            {
                s.Set("local_file", localFile);
                s.Set("is_local_file", true);
            }
            else if (!string.IsNullOrEmpty(url))
            {
                s.Set("input", url);
                s.Set("is_local_file", false);
            }

            s.Set("looping", loop);
        });
    }

    /// <summary>
    /// Sets the local file path.
    /// </summary>
    /// <param name="filePath">Path to the media file.</param>
    public MediaSource SetFile(string filePath)
    {
        Update(s =>
        {
            s.Set("local_file", filePath);
            s.Set("is_local_file", true);
        });
        return this;
    }

    /// <summary>
    /// Sets the URL input.
    /// </summary>
    /// <param name="url">URL of the media.</param>
    public MediaSource SetUrl(string url)
    {
        Update(s =>
        {
            s.Set("input", url);
            s.Set("is_local_file", false);
        });
        return this;
    }

    /// <summary>
    /// Sets whether to loop the media.
    /// </summary>
    /// <param name="loop">Whether to loop.</param>
    public MediaSource SetLoop(bool loop)
    {
        Update(s => s.Set("looping", loop));
        return this;
    }

    /// <summary>
    /// Sets whether to restart playback when becoming active.
    /// </summary>
    /// <param name="restart">Whether to restart on active.</param>
    public MediaSource SetRestartOnActive(bool restart)
    {
        Update(s => s.Set("restart_on_activate", restart));
        return this;
    }

    /// <summary>
    /// Sets the playback speed percentage.
    /// </summary>
    /// <param name="speedPercent">Speed percentage (100 = normal speed).</param>
    public MediaSource SetSpeed(int speedPercent)
    {
        Update(s => s.Set("speed_percent", speedPercent));
        return this;
    }

    /// <summary>
    /// Sets whether to show nothing when playback ends.
    /// </summary>
    /// <param name="hideOnEnd">Whether to hide on end.</param>
    public MediaSource SetHideOnEnd(bool hideOnEnd)
    {
        Update(s => s.Set("clear_on_media_end", hideOnEnd));
        return this;
    }

    /// <summary>
    /// Sets whether to close the file when inactive.
    /// </summary>
    /// <param name="close">Whether to close on inactive.</param>
    public MediaSource SetCloseOnInactive(bool close)
    {
        Update(s => s.Set("close_when_inactive", close));
        return this;
    }

    /// <summary>
    /// Sets buffering settings for network streams.
    /// </summary>
    /// <param name="bufferingMb">Buffer size in megabytes.</param>
    public MediaSource SetBuffering(int bufferingMb)
    {
        Update(s => s.Set("buffering_mb", bufferingMb));
        return this;
    }

    /// <summary>
    /// Sets hardware decoding preference.
    /// </summary>
    /// <param name="useHardware">Whether to use hardware decoding.</param>
    public MediaSource SetHardwareDecoding(bool useHardware)
    {
        Update(s => s.Set("hw_decode", useHardware));
        return this;
    }
}
