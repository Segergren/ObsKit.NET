namespace ObsKit.NET.Signals;

/// <summary>
/// Signals emitted by OBS sources.
/// </summary>
public enum SourceSignal
{
    /// <summary>Emitted when a source is destroyed.</summary>
    Destroy,
    /// <summary>Emitted when a source is removed.</summary>
    Remove,
    /// <summary>Emitted when source settings are updated.</summary>
    Update,
    /// <summary>Emitted when source is saved.</summary>
    Save,
    /// <summary>Emitted when source is loaded.</summary>
    Load,
    /// <summary>Emitted when source becomes active.</summary>
    Activate,
    /// <summary>Emitted when source becomes inactive.</summary>
    Deactivate,
    /// <summary>Emitted when source is shown.</summary>
    Show,
    /// <summary>Emitted when source is hidden.</summary>
    Hide,
    /// <summary>Emitted when source is muted. Calldata: muted (bool).</summary>
    Mute,
    /// <summary>Emitted when push-to-mute state changes. Calldata: enabled (bool).</summary>
    PushToMuteChanged,
    /// <summary>Emitted when push-to-mute delay changes. Calldata: delay (int).</summary>
    PushToMuteDelay,
    /// <summary>Emitted when push-to-talk state changes. Calldata: enabled (bool).</summary>
    PushToTalkChanged,
    /// <summary>Emitted when push-to-talk delay changes. Calldata: delay (int).</summary>
    PushToTalkDelay,
    /// <summary>Emitted when source is enabled/disabled. Calldata: enabled (bool).</summary>
    Enable,
    /// <summary>Emitted when source is renamed. Calldata: new_name (string), prev_name (string).</summary>
    Rename,
    /// <summary>Emitted when source volume changes. Calldata: volume (float).</summary>
    Volume,
    /// <summary>Emitted when source properties are updated.</summary>
    UpdateProperties,
    /// <summary>Emitted when source flags change. Calldata: flags (int).</summary>
    UpdateFlags,
    /// <summary>Emitted when audio sync offset changes. Calldata: offset (int).</summary>
    AudioSync,
    /// <summary>Emitted when audio balance changes. Calldata: balance (float).</summary>
    AudioBalance,
    /// <summary>Emitted when audio mixer assignment changes. Calldata: mixers (int).</summary>
    AudioMixers,
    /// <summary>Emitted when audio monitoring type changes. Calldata: type (int).</summary>
    AudioMonitoring,
    /// <summary>Emitted when audio monitoring is activated.</summary>
    AudioActivate,
    /// <summary>Emitted when audio monitoring is deactivated.</summary>
    AudioDeactivate,
    /// <summary>Emitted when a filter is added. Calldata: filter (ptr).</summary>
    FilterAdd,
    /// <summary>Emitted when a filter is removed. Calldata: filter (ptr).</summary>
    FilterRemove,
    /// <summary>Emitted when filters are reordered.</summary>
    ReorderFilters,
    /// <summary>Emitted when a transition starts.</summary>
    TransitionStart,
    /// <summary>Emitted when transition video stops.</summary>
    TransitionVideoStop,
    /// <summary>Emitted when transition stops.</summary>
    TransitionStop,
    /// <summary>Emitted when media starts playing.</summary>
    MediaPlay,
    /// <summary>Emitted when media is paused.</summary>
    MediaPause,
    /// <summary>Emitted when media is restarted.</summary>
    MediaRestart,
    /// <summary>Emitted when media stops.</summary>
    MediaStopped,
    /// <summary>Emitted for next media track.</summary>
    MediaNext,
    /// <summary>Emitted for previous media track.</summary>
    MediaPrevious,
    /// <summary>Emitted when media starts.</summary>
    MediaStarted,
    /// <summary>Emitted when media ends.</summary>
    MediaEnded,
    /// <summary>Emitted when window/game is hooked (game capture). Calldata: title (string), class (string), executable (string).</summary>
    Hooked,
    /// <summary>Emitted when window/game is unhooked (game capture).</summary>
    Unhooked,
    /// <summary>Emitted when slideshow slide changes. Calldata: index (int), path (string).</summary>
    SlideChanged
}

/// <summary>
/// Signals emitted by OBS outputs.
/// </summary>
public enum OutputSignal
{
    /// <summary>Emitted when output starts.</summary>
    Start,
    /// <summary>Emitted when output stops. Calldata: code (int).</summary>
    Stop,
    /// <summary>Emitted when output is paused.</summary>
    Pause,
    /// <summary>Emitted when output is unpaused.</summary>
    Unpause,
    /// <summary>Emitted when output is starting (pre-start).</summary>
    Starting,
    /// <summary>Emitted when output is stopping (pre-stop).</summary>
    Stopping,
    /// <summary>Emitted when output is activated.</summary>
    Activate,
    /// <summary>Emitted when output is deactivated.</summary>
    Deactivate,
    /// <summary>Emitted when attempting to reconnect.</summary>
    Reconnect,
    /// <summary>Emitted when reconnection succeeds.</summary>
    ReconnectSuccess,
    /// <summary>Emitted when output file changes (ffmpeg mux). Calldata: next_file (string).</summary>
    FileChanged,
    /// <summary>Emitted when replay buffer is saved.</summary>
    Saved,
    /// <summary>Emitted when there is a writing error.</summary>
    WritingError
}

/// <summary>
/// Signals emitted by OBS scenes.
/// </summary>
public enum SceneSignal
{
    /// <summary>Emitted when scene item is added. Calldata: item (ptr).</summary>
    ItemAdd,
    /// <summary>Emitted when scene item is removed. Calldata: item (ptr).</summary>
    ItemRemove,
    /// <summary>Emitted when scene items are reordered.</summary>
    Reorder,
    /// <summary>Emitted when scene is refreshed.</summary>
    Refresh,
    /// <summary>Emitted when scene item visibility changes. Calldata: item (ptr), visible (bool).</summary>
    ItemVisible,
    /// <summary>Emitted when scene item is selected. Calldata: item (ptr).</summary>
    ItemSelect,
    /// <summary>Emitted when scene item is deselected. Calldata: item (ptr).</summary>
    ItemDeselect,
    /// <summary>Emitted when scene item transform changes. Calldata: item (ptr).</summary>
    ItemTransform,
    /// <summary>Emitted when scene item lock state changes. Calldata: item (ptr), locked (bool).</summary>
    ItemLocked
}

/// <summary>
/// Provides extension methods for signal enums.
/// </summary>
public static class SignalExtensions
{
    /// <summary>
    /// Converts a SourceSignal enum value to its OBS signal string name.
    /// </summary>
    public static string ToSignalName(this SourceSignal signal) => signal switch
    {
        SourceSignal.Destroy => "destroy",
        SourceSignal.Remove => "remove",
        SourceSignal.Update => "update",
        SourceSignal.Save => "save",
        SourceSignal.Load => "load",
        SourceSignal.Activate => "activate",
        SourceSignal.Deactivate => "deactivate",
        SourceSignal.Show => "show",
        SourceSignal.Hide => "hide",
        SourceSignal.Mute => "mute",
        SourceSignal.PushToMuteChanged => "push_to_mute_changed",
        SourceSignal.PushToMuteDelay => "push_to_mute_delay",
        SourceSignal.PushToTalkChanged => "push_to_talk_changed",
        SourceSignal.PushToTalkDelay => "push_to_talk_delay",
        SourceSignal.Enable => "enable",
        SourceSignal.Rename => "rename",
        SourceSignal.Volume => "volume",
        SourceSignal.UpdateProperties => "update_properties",
        SourceSignal.UpdateFlags => "update_flags",
        SourceSignal.AudioSync => "audio_sync",
        SourceSignal.AudioBalance => "audio_balance",
        SourceSignal.AudioMixers => "audio_mixers",
        SourceSignal.AudioMonitoring => "audio_monitoring",
        SourceSignal.AudioActivate => "audio_activate",
        SourceSignal.AudioDeactivate => "audio_deactivate",
        SourceSignal.FilterAdd => "filter_add",
        SourceSignal.FilterRemove => "filter_remove",
        SourceSignal.ReorderFilters => "reorder_filters",
        SourceSignal.TransitionStart => "transition_start",
        SourceSignal.TransitionVideoStop => "transition_video_stop",
        SourceSignal.TransitionStop => "transition_stop",
        SourceSignal.MediaPlay => "media_play",
        SourceSignal.MediaPause => "media_pause",
        SourceSignal.MediaRestart => "media_restart",
        SourceSignal.MediaStopped => "media_stopped",
        SourceSignal.MediaNext => "media_next",
        SourceSignal.MediaPrevious => "media_previous",
        SourceSignal.MediaStarted => "media_started",
        SourceSignal.MediaEnded => "media_ended",
        SourceSignal.Hooked => "hooked",
        SourceSignal.Unhooked => "unhooked",
        SourceSignal.SlideChanged => "slide_changed",
        _ => throw new ArgumentOutOfRangeException(nameof(signal), signal, null)
    };

    /// <summary>
    /// Converts an OutputSignal enum value to its OBS signal string name.
    /// </summary>
    public static string ToSignalName(this OutputSignal signal) => signal switch
    {
        OutputSignal.Start => "start",
        OutputSignal.Stop => "stop",
        OutputSignal.Pause => "pause",
        OutputSignal.Unpause => "unpause",
        OutputSignal.Starting => "starting",
        OutputSignal.Stopping => "stopping",
        OutputSignal.Activate => "activate",
        OutputSignal.Deactivate => "deactivate",
        OutputSignal.Reconnect => "reconnect",
        OutputSignal.ReconnectSuccess => "reconnect_success",
        OutputSignal.FileChanged => "file_changed",
        OutputSignal.Saved => "saved",
        OutputSignal.WritingError => "writing_error",
        _ => throw new ArgumentOutOfRangeException(nameof(signal), signal, null)
    };

    /// <summary>
    /// Converts a SceneSignal enum value to its OBS signal string name.
    /// </summary>
    public static string ToSignalName(this SceneSignal signal) => signal switch
    {
        SceneSignal.ItemAdd => "item_add",
        SceneSignal.ItemRemove => "item_remove",
        SceneSignal.Reorder => "reorder",
        SceneSignal.Refresh => "refresh",
        SceneSignal.ItemVisible => "item_visible",
        SceneSignal.ItemSelect => "item_select",
        SceneSignal.ItemDeselect => "item_deselect",
        SceneSignal.ItemTransform => "item_transform",
        SceneSignal.ItemLocked => "item_locked",
        _ => throw new ArgumentOutOfRangeException(nameof(signal), signal, null)
    };
}
