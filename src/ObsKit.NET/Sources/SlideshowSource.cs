using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Sources;

/// <summary>
/// Cycles through a list of images (or image directories) with transitions.
/// Use the media controls inherited from <see cref="Source"/>
/// (<see cref="Source.NextMedia"/>, <see cref="Source.PreviousMedia"/>,
/// <see cref="Source.PauseMedia"/>, ...) to navigate, especially in manual mode.
/// </summary>
public sealed class SlideshowSource : Source
{
    /// <summary>
    /// The source type ID for slideshows.
    /// </summary>
    public const string SourceTypeId = "slideshow_v2";

    /// <summary>
    /// Transition styles between slides.
    /// </summary>
    public enum SlideTransition
    {
        /// <summary>Instant cut.</summary>
        Cut,
        /// <summary>Cross-fade (default).</summary>
        Fade,
        /// <summary>Swipe.</summary>
        Swipe,
        /// <summary>Slide.</summary>
        Slide
    }

    /// <summary>
    /// How playback behaves when the source's visibility changes.
    /// </summary>
    public enum PlaybackBehavior
    {
        /// <summary>Always play, even when not visible (default).</summary>
        AlwaysPlay,
        /// <summary>Stop when hidden, restart from the first slide when shown.</summary>
        StopRestart,
        /// <summary>Pause when hidden, resume when shown.</summary>
        PauseUnpause
    }

    /// <summary>
    /// Creates a slideshow source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="files">Optional initial image files or directories.</param>
    public SlideshowSource(string name = "Slideshow", params string[] files)
        : base(SourceTypeId, name)
    {
        if (files.Length > 0)
            SetFiles(files);
    }

    /// <summary>
    /// Sets the slideshow's image files, replacing the previous list.
    /// Entries can be image files or directories (directories are scanned for images).
    /// </summary>
    /// <param name="paths">The image file or directory paths.</param>
    public SlideshowSource SetFiles(IEnumerable<string> paths)
    {
        Update(s => s.SetStringArray("files", paths));
        return this;
    }

    /// <summary>
    /// Sets how long each slide is shown.
    /// </summary>
    /// <param name="duration">The time per slide (OBS default 2 seconds, minimum 50 ms).</param>
    public SlideshowSource SetSlideTime(TimeSpan duration)
    {
        Update(s => s.Set("slide_time", (long)duration.TotalMilliseconds));
        return this;
    }

    /// <summary>
    /// Sets the transition between slides.
    /// </summary>
    /// <param name="transition">The transition style.</param>
    /// <param name="speed">Optional transition duration (OBS default 700 ms).</param>
    public SlideshowSource SetTransition(SlideTransition transition, TimeSpan? speed = null)
    {
        Update(s =>
        {
            s.Set("transition", transition switch
            {
                SlideTransition.Cut => "cut",
                SlideTransition.Swipe => "swipe",
                SlideTransition.Slide => "slide",
                _ => "fade"
            });

            if (speed != null)
                s.Set("transition_speed", (long)speed.Value.TotalMilliseconds);
        });
        return this;
    }

    // slideshow_v2 (the mk2 rewrite) replaced the independent legacy "loop"/"randomize" booleans
    // with a single "playback_mode" combo (once/loop/random) and only reads the legacy keys via a
    // one-time migration, so writing them is silently ignored after the first update. Track both
    // intents and resolve them to "playback_mode" on every change. mk2's default is "loop".
    private bool _loop = true;
    private bool _randomize;

    // once = play through then stop; loop = loop in order; random = loop in random order.
    private string ResolvePlaybackMode() => _randomize ? "random" : (_loop ? "loop" : "once");

    /// <summary>
    /// Sets whether the slideshow loops back to the first slide.
    /// </summary>
    /// <param name="loop">Whether to loop.</param>
    public SlideshowSource SetLoop(bool loop)
    {
        _loop = loop;
        Update(s => s.Set("playback_mode", ResolvePlaybackMode()));
        return this;
    }

    /// <summary>
    /// Sets whether slides play in random order. In slideshow_v2 random order always loops, so
    /// enabling randomize takes precedence over <see cref="SetLoop"/>.
    /// </summary>
    /// <param name="randomize">Whether to randomize.</param>
    public SlideshowSource SetRandomize(bool randomize)
    {
        _randomize = randomize;
        Update(s => s.Set("playback_mode", ResolvePlaybackMode()));
        return this;
    }

    /// <summary>
    /// Sets whether the source hides when the slideshow finishes (non-looping only).
    /// </summary>
    /// <param name="hide">Whether to hide when done.</param>
    public SlideshowSource SetHideWhenDone(bool hide)
    {
        Update(s => s.Set("hide", hide));
        return this;
    }

    /// <summary>
    /// Sets how playback reacts to the source being shown/hidden.
    /// </summary>
    /// <param name="behavior">The playback behavior.</param>
    public SlideshowSource SetPlaybackBehavior(PlaybackBehavior behavior)
    {
        Update(s => s.Set("playback_behavior", behavior switch
        {
            PlaybackBehavior.StopRestart => "stop_restart",
            PlaybackBehavior.PauseUnpause => "pause_unpause",
            _ => "always_play"
        }));
        return this;
    }

    /// <summary>
    /// Sets whether slides only advance manually
    /// (via <see cref="Source.NextMedia"/>/<see cref="Source.PreviousMedia"/>).
    /// </summary>
    /// <param name="manual">True for manual navigation, false for automatic (default).</param>
    public SlideshowSource SetManualMode(bool manual)
    {
        Update(s => s.Set("slide_mode", manual ? "mode_manual" : "mode_auto"));
        return this;
    }
}
