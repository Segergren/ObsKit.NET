# ObsKit.NET

A modern .NET 10 wrapper for OBS Studio, providing a fluent C# API for video recording, streaming, and replay buffer functionality.

## Features

- **Cross-Platform** - Windows, Linux, and macOS support
- **Fluent API** - Clean, chainable configuration
- **Streaming** - Stream to Twitch, YouTube, Facebook, custom RTMP servers, or WHIP/WebRTC for sub-second latency
- **Raw Data Taps** - Zero-copy callbacks for video frames and audio samples (previews, waveforms, voice detection, custom processing)
- **Recording** - Record video to Hybrid MP4 (crash-resilient), MP4, MKV, FLV, and more, with chapter markers and file splitting
- **Replay Buffer** - Keep a rolling buffer of the last N seconds, with awaitable saves
- **Preview Display** - Render the live canvas (or one source) directly into your app's window
- **Multiple Canvases** - Record a second view simultaneously, e.g. a vertical 9:16 mix (OBS 31+)
- **Sources** - Monitor capture, window capture, game capture (with game audio), webcam, application audio, microphone/desktop audio, images, media files, text, solid color, browser overlays
- **Filters** - Noise gate, noise suppression, gain, compressor, limiter, expander, crop, color correction, chroma key, sharpness, scaling, render delay
- **Scene Transitions** - Animate the program output between scenes (fade, cut, slide, swipe, wipe, stinger), with auto or manual scrubbing
- **Encoders** - x264, NVENC, AMF, QuickSync, VideoToolbox (H.264/HEVC/AV1), AAC/Opus/FLAC audio, with runtime capability discovery
- **Virtual Camera** - Expose the canvas as a system camera device
- **Audio Tooling** - Per-track routing, live level meters, dB volume and curve-aware faders, sync offset, balance, monitoring device selection
- **Property Introspection** - Enumerate any source's configurable properties (types, ranges, option lists) to build dynamic config UIs or discover devices/resolutions
- **Headless Operation** - Run without GUI dependencies

## Requirements

- .NET 10.0 or later
- OBS Studio runtime (see [OBS Runtime Setup](#obs-runtime-setup))

## Quick Start

```csharp
using ObsKit.NET;
using ObsKit.NET.Outputs;
using ObsKit.NET.Sources;

var obsPath = AppContext.BaseDirectory;

// Initialize OBS
using var obs = Obs.Initialize(config => config
    .WithDataPath(Path.Combine(obsPath, "data", "libobs"))
    .WithModulePath(
        Path.Combine(obsPath, "obs-plugins", "64bit"),
        Path.Combine(obsPath, "data", "obs-plugins", "%module%"))
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    .WithAudio(a => a.WithSampleRate(48000)));

Console.WriteLine($"OBS {Obs.Version} initialized");

// Create a scene with monitor capture
using var scene = Obs.Scenes.Create("My Scene");
using var monitor = MonitorCapture.FromPrimary();
scene.AddSource(monitor);
scene.SetAsProgram();

// Set up and start recording
using var recording = new RecordingOutput("My Recording")
    .SetPath("output.mp4")
    .SetFormat(RecordingFormat.Mp4)
    .WithDefaultEncoders(videoBitrate: 6000, audioBitrate: 192);

recording.Start();
Console.WriteLine("Recording... Press any key to stop.");
Console.ReadKey();

Console.WriteLine($"Recorded {recording.TotalFrames} frames");
recording.Stop();   // with Obs.AutoDispose (default), Stop also disposes the output
```

## OBS Runtime Setup

ObsKit.NET requires OBS Studio binaries. Use the setup script to download them:

```bash
./tools/setup-obs-runtime.sh
```

The script will prompt you for version and platform. For manual setup, download OBS from [GitHub Releases](https://github.com/obsproject/obs-studio/releases).

### Windows Structure

```
YourApp/
├── YourApp.exe
├── obs.dll, obs-ffmpeg-mux.exe, *.dll  # From OBS bin/64bit/
├── data/
│   ├── libobs/                          # Shader files
│   └── obs-plugins/                     # Plugin data
└── obs-plugins/64bit/                   # Plugin DLLs
```

### Linux Structure

```
YourApp/
├── YourApp
├── lib/libobs.so.0                      # OBS libraries
├── obs-plugins/                         # Plugin .so files
└── data/libobs/, obs-plugins/           # Data files
```

Run with: `LD_LIBRARY_PATH="$PWD/lib" ./YourApp`

### macOS Structure

```
YourApp/
├── YourApp
└── OBS.app/Contents/
    ├── Frameworks/                      # OBS libraries
    ├── PlugIns/                         # Plugin .so files
    └── Resources/data/                  # Data files
```

Run with: `DYLD_LIBRARY_PATH="$PWD/OBS.app/Contents/Frameworks" ./YourApp`

## Source Types

```csharp
// Monitor capture
using var monitor = MonitorCapture.FromPrimary();
using var monitor = MonitorCapture.FromMonitor(1);

// Window capture
using var window = WindowCapture.FromWindow(WindowCapture.AvailableWindows[0]);

// Game capture (Windows only) — optionally with the game's audio (Windows 10 2004+)
using var game = new GameCapture("Game", GameCapture.CaptureMode.AnyFullscreen)
    .SetCaptureAudio()                                          // game audio without a separate source
    .SetCaptureOverlays()                                       // include Steam/Discord overlays
    .SetHookRate(GameCapture.HookRate.Fast)                     // hook new games faster
    .SetRgb10A2ColorSpace(GameCapture.Rgb10A2ColorSpace.Pq2100); // HDR games

// Hotkey mode: capture whatever window is in the foreground on demand
using var hotkeyGame = new GameCapture("Game", GameCapture.CaptureMode.HotkeyForeground);
hotkeyGame.CaptureForegroundWindow();   // your app decides when (e.g. from its own global hotkey)

// Image and media
using var image = ImageSource.FromFile("logo.png");
using var media = new MediaSource("Video", "video.mp4").SetLooping(true);

// Slideshow (image files or directories; navigate with NextMedia/PreviousMedia)
using var slides = new SlideshowSource("Intermission", @"C:\art")
    .SetSlideTime(TimeSpan.FromSeconds(5))
    .SetTransition(SlideshowSource.SlideTransition.Fade)
    .SetLoop(true);

// Webcam / video capture device (DirectShow on Windows, V4L2 on Linux, AVFoundation on macOS)
foreach (var d in WebcamCapture.ListDevices())
    Console.WriteLine($"  {d.Name}  ->  {d.DeviceId}");
using var webcam = WebcamCapture.FromDeviceName("BRIO")     // partial name match
                   ?? WebcamCapture.FromDefault();          // first device
webcam?.SetCustomResolution(3840, 2160, 30, videoFormat: "MJPEG"); // optional: force 4K30

// Audio capture (WASAPI on Windows, PulseAudio/PipeWire on Linux, Core Audio on macOS)
foreach (var d in AudioInputCapture.ListDevices())     // microphones (also: AudioOutputCapture.ListDevices())
    Console.WriteLine($"  {d.Name}  ->  {d.DeviceId}");
using var mic = AudioInputCapture.FromDefault();
using var desktop = AudioOutputCapture.FromDefault();

// Application audio capture (Windows 10 2004+, WASAPI process loopback)
using var discord = ApplicationAudioCapture.FromExecutable("Discord.exe");
discord.Hooked += s => Console.WriteLine($"Capturing audio from {s.HookedExecutable}");

// Text and solid color
using var label = new TextSource("Label", "LIVE").SetFont("Arial", 64).SetColor(0xFF0000FF);
label.SetTextFromFile(@"C:\overlay\score.txt");   // re-renders whenever the file changes
using var background = new ColorSource("Background", abgr: 0xFF101010);

// Browser overlay (requires the obs-browser plugin in the OBS runtime)
if (BrowserSource.IsAvailable())
{
    using var overlay = new BrowserSource("Overlay", "https://example.com/overlay", 1920, 1080)
        .SetRerouteAudio();                                  // control page audio like a source
    overlay.SendJavascriptEvent("kill", "{\"count\":3}");    // window.addEventListener("kill", ...)
    overlay.Refresh();                                       // reload, bypassing cache

    // Forward input for clickable overlays (coordinates in source pixels)
    overlay.SendMouseMove(640, 360);
    overlay.SendMouseClick(640, 360);                        // press...
    overlay.SendMouseClick(640, 360, buttonUp: true);        // ...and release
}

// Media playback control (any media-capable source)
using var media = new MediaSource("Video", "video.mp4");
media.PauseMedia();
media.MediaTime = TimeSpan.FromSeconds(30);
Console.WriteLine($"{media.MediaState}: {media.MediaTime}/{media.MediaDuration}");
```

### Inspecting source properties

Discover what a source's plugin exposes — for dynamic config UIs or to enumerate valid
option values (device pickers, resolutions, FPS):

```csharp
using var webcam = new WebcamCapture("Cam");

// Quick list of one property's options (display name + value)
foreach (var (name, value) in webcam.GetListPropertyItems("video_device_id"))
    Console.WriteLine($"{name} = {value}");

// Full introspection: every property with type, state, ranges, and list items
foreach (var prop in webcam.GetProperties())
{
    Console.WriteLine($"{prop.Name} ({prop.Type}): {prop.Description}");
    if (prop.IntRange is (int min, int max, int step))
        Console.WriteLine($"  range {min}..{max} step {step}");
    foreach (var item in prop.ListItems)
        Console.WriteLine($"  - {item.Name} = {item.StringValue ?? item.IntValue.ToString()}");
}
```

## Scene Items (overlays)

```csharp
var webcamItem = scene.AddSource(webcam);
webcamItem.SetPosition(1440, 810)
    .SetBounds(ObsBoundsType.ScaleInner, 480, 270)   // fit into a 480x270 box
    .SetScaleFilter(ObsScaleType.Lanczos)            // high-quality downscale
    .SetBlendingMode(ObsBlendingType.Normal)
    .SetShowTransition(TransitionTypes.Fade, TimeSpan.FromMilliseconds(250))
    .SetHideTransition(TransitionTypes.Fade, TimeSpan.FromMilliseconds(250));
webcamItem.BoundsAlignment = ObsAlignment.TopLeft;   // pin within the box (default: Center)

webcamItem.IsVisible = false;   // fades out instead of popping
```

### Groups

Group several items so they can be moved, scaled, and shown/hidden as one unit:

```csharp
var overlay = scene.AddGroup("Overlay");
overlay.AddItem(webcamItem);     // move existing items into the group
overlay.AddItem(scene.AddSource(alertsBrowser));

overlay.SetPosition(0, 0).SetScale(0.5f, 0.5f);   // transforms the whole group
overlay.IsVisible = false;                         // hides every member at once

foreach (var member in overlay.GetGroupItems())
    Console.WriteLine(member.Source.Name);

scene.GetGroup("Overlay")?.Ungroup();              // disband, returning items to the scene
```

## Scene Transitions

Animate the program output between scenes. Assign a transition to an output channel,
seed the starting scene, then animate to a new one:

```csharp
using ObsKit.NET.Sources;

using var transition = Transition.Fade();      // or Transition.Cut(), .Slide(),
                                               // or new Transition(TransitionTypes.Wipe, "Wipe")
transition.Set(introScene);                    // seed the current scene (no animation)
Obs.SetOutputSource(0, transition);            // the transition is now the program source

// Cross-fade to gameplay over 300 ms
transition.Start(gameplayScene, TimeSpan.FromMilliseconds(300));

if (transition.IsTransitioning)
    transition.ForceStop();                    // snap to the destination immediately

// Manual (scrubbed) transitions, e.g. driven by a slider:
transition.Start(gameplayScene, TimeSpan.Zero, ObsTransitionMode.Manual);
transition.SetManualTime(0.5f);                // 0.0 = source A, 1.0 = source B
```

## Audio Filters

Typed wrappers for the built-in OBS audio filters, with OBS defaults:

```csharp
using ObsKit.NET.Filters;

using var noiseGate = new NoiseGateFilter()
    .SetOpenThreshold(-42)
    .SetCloseThreshold(-48)
    .SetHoldTime(200);
mic.AddFilter(noiseGate);

using var suppression = new NoiseSuppressFilter()
    .SetMethod(NoiseSuppressFilter.SuppressionMethod.RnNoise);
mic.AddFilter(suppression);

// Also available: GainFilter, CompressorFilter, LimiterFilter, ExpanderFilter,
// video filters (CropFilter, ColorCorrectionFilter, ChromaKeyFilter, SharpnessFilter,
// ScaleFilter, RenderDelayFilter), and new Filter("any_filter_id", "Name") for everything else.

// Inspect or reorder the filter chain
foreach (var f in mic.GetFilters())
    Console.WriteLine(f.Name);

// Bypass a filter without removing it
suppression.IsEnabled = false;

// Apply the same processing chain to another source
secondMic.CopyFiltersFrom(mic);
```

## Audio Levels & Monitoring

```csharp
using ObsKit.NET.Audio;

// Live level meter for UI VU meters (values in dB)
using var meter = new AudioMeter();
meter.AttachSource(mic);
meter.LevelsUpdated += (m, levels) => Console.WriteLine($"Peak: {levels.Peak[0]:F1} dB");

// Mic sync alignment and stereo balance
mic.AudioSyncOffset = TimeSpan.FromMilliseconds(120);
mic.AudioBalance = 0.5f;

// Route monitored sources to a specific output device
foreach (var (name, id) in Obs.EnumerateAudioMonitoringDevices())
    Console.WriteLine($"{name}: {id}");
Obs.SetAudioMonitoringDevice("default");

// Let the user hear their mic through the monitoring device
mic.MonitoringType = ObsMonitoringType.MonitorAndOutput;

// Set volume in dB directly (not clamped to unity, so it can apply gain)
mic.VolumeDb = -6.0f;

// Push-to-talk: mic stays muted unless its hotkey is held (200 ms release tail)
mic.PushToTalkEnabled = true;
mic.PushToTalkDelay = TimeSpan.FromMilliseconds(200);

// Drive a source's volume from a UI slider with the same curve OBS uses
using var fader = new VolumeFader();   // cubic curve by default
fader.AttachSource(mic);
fader.Deflection = 0.75f;              // slider at 75% -> sets mic volume
Console.WriteLine($"{fader.Db:F1} dB");
```

## Audio Tracks

Assign sources and encoders to audio tracks (1-6) without bitmask math:

```csharp
mic.SetAudioTracks(1, 2);              // full mix + isolated mic track
desktop.SetAudioTracks(1, 3);          // full mix + isolated desktop track
mic.SetAudioTrackEnabled(4);           // add a single track

// One audio encoder per track on the output
recording.WithAudioEncoder(AudioEncoder.CreateAac("Mix", 192, mixerIdx: 0), track: 0);
recording.WithAudioEncoder(AudioEncoder.CreateAac("Mic", 160, mixerIdx: 1), track: 1);
```

## Screenshots

```csharp
// Full source screenshot (returns BGRA pixels, width, height — or null)
var screenshot = source.TakeScreenshot();
// Cropped screenshot (only transfers the crop region from GPU)
var cropped = source.TakeScreenshot(cropX: 960, cropY: 200, cropWidth: 640, cropHeight: 160);
if (screenshot != null)
{
    using var bmp = new Bitmap((int)screenshot.Width, (int)screenshot.Height, (int)(screenshot.Width * 4),
        PixelFormat.Format32bppArgb, Marshal.UnsafeAddrOfPinnedArrayElement(screenshot.Pixels, 0));
    bmp.Save("screenshot.jpg", ImageFormat.Jpeg);
}
```

## Raw Video Frames

Subscribe to the live canvas output. OBS scales/converts each frame on the GPU to your requested format and resolution before invoking the callback on its video thread.

```csharp
using ObsKit.NET;
using ObsKit.NET.Native.Types;

// Get every Nth frame at 480x270 BGRA (e.g. for a low-overhead preview).
using var preview = Obs.SubscribeRawVideo(
    VideoFormat.BGRA, width: 480, height: 270,
    callback: (in RawVideoFrame frame) =>
    {
        ReadOnlySpan<byte> pixels = frame.GetPackedPlane();   // BGRA bytes (may have row padding — see GetLinesize(0))
        // ... encode to JPEG, push over IPC, etc. Don't block — this is OBS's video thread.
    },
    frameRateDivisor: 6); // 60fps canvas → ~10fps callback

// Dispose to stop receiving frames.
```

## Raw Audio

Tap the mixed audio of any track — e.g. for waveform rendering, voice activity detection, or custom processing.

```csharp
using ObsKit.NET.Audio;

using var tap = Obs.SubscribeRawAudio((in RawAudioFrame frame) =>
{
    ReadOnlySpan<float> left = frame.GetFloatPlane(0);   // planar float, one plane per channel
    // ... compute RMS, run VAD, etc. Don't block — this is OBS's audio thread.
}, track: 1);

// Or tap a single source before mixing (e.g. the microphone alone)
using var micTap = mic.SubscribeAudio((in RawAudioFrame frame, bool muted) =>
{
    // voice activity detection, custom noise processing, ...
});

// Dispose to stop receiving audio.
```

## Recording

```csharp
using var recording = new RecordingOutput("My Recording")
    .SetPath("output.mp4")
    .SetFormat(RecordingFormat.HybridMp4)   // crash-resilient MP4 with chapter support (OBS 30.2+)
    .WithBestEncoders(videoBitrate: 12000); // NVENC -> AMF -> QuickSync -> x264

// Typed stop info (disk full, encoder error, ...)
recording.Stopped += (_, e) =>
{
    if (!e.IsSuccess)
        Console.WriteLine($"Recording stopped: {e.Code} ({e.LastError})");
};

recording.Start();

// Chapter markers (Hybrid MP4/MOV only) — great for kill/goal/highlight bookmarks
recording.AddChapter("First blood");
recording.AddChapter();                     // auto-named "Unnamed 2"

recording.Stop();

// Automatic + manual file splitting (file path is generated from the template)
using var splitRecording = new RecordingOutput("Split Recording")
    .SetFormat(RecordingFormat.Mkv)
    .WithFileSplitting(@"C:\Videos", maxTimeSeconds: 30 * 60, extension: "mkv")
    .WithDefaultEncoders();
splitRecording.Start();
splitRecording.SplitFile();                 // start a new file now
```

## Replay Buffer

```csharp
using var replay = new ReplayBuffer(maxSeconds: 60)
    .SetDirectory(@"C:\Videos\Replays")
    .WithDefaultEncoders();
replay.Start();

// Awaitable save — completes when the file has finished writing
string? path = await replay.SaveAsync();

// Clear the buffered footage so the next save only contains new footage
// (saving does not clear OBS's in-memory buffer — without this, two saves
// close together contain overlapping footage)
await replay.ResetAsync();

// Or event-driven
replay.Saved += (_, e) => Console.WriteLine($"Replay saved to {e.Path}");
replay.Save();
```

## Preview Display

Render the live canvas into a window of your app (WinForms, WPF via HwndHost, Avalonia native control host). OBS draws straight into the window's swap chain — no extra encoding, no CPU frame copies.

```csharp
using ObsKit.NET.Video;

using var preview = new PreviewDisplay(panel.Handle,
    width: (uint)panel.ClientSize.Width, height: (uint)panel.ClientSize.Height);

// Preview a single source or a secondary canvas instead of the main canvas
preview.Source = gameCapture;
preview.Canvas = verticalCanvas;   // see "Multiple Canvases"

// Keep the surface in sync with the host control (sizes are physical pixels)
panel.Resize += (_, _) => preview.Resize((uint)panel.ClientSize.Width, (uint)panel.ClientSize.Height);

// Pause rendering while hidden
preview.IsEnabled = false;
```

## Multiple Canvases (OBS 31+)

Compose and record more than one view at once — e.g. a vertical 9:16 mix alongside the main horizontal recording:

```csharp
using ObsKit.NET.Scenes;

using var vertical = Canvas.Create("Vertical", 1080, 1920);
using var verticalScene = vertical.CreateScene("Vertical Scene");
verticalScene.AddSource(game);            // same source, framed for 9:16
vertical.SetScene(verticalScene);

// Or reuse an existing layout: duplicate the main scene and move it over
using var copy = scene.Duplicate("Vertical Copy");
vertical.MoveScene(copy);

using var verticalRecording = new RecordingOutput("Vertical")
    .SetPath("vertical.mp4")
    .WithVideoEncoder(VideoEncoder.CreateBest("Vertical Video", 8000), vertical, takeOwnership: true)
    .WithAudioEncoder(AudioEncoder.CreateAac("Vertical Audio"), takeOwnership: true);
verticalRecording.Start();                // records simultaneously with the main output
```

## Encoders

```csharp
// Best available hardware encoder (NVENC -> AMF -> QuickSync -> x264)
var encoder = VideoEncoder.CreateBest("Video", bitrate: 6000, preferHevc: true);

// Video - x264 (CPU)
var encoder = VideoEncoder.CreateX264("Video", bitrate: 6000);

// Video - NVENC (NVIDIA), AMF (AMD), QuickSync (Intel)
var encoder = VideoEncoder.CreateNvencH264("Video", bitrate: 6000);   // also CreateNvencHevc, CreateNvencAv1
var encoder = VideoEncoder.CreateAmfHevc("Video", bitrate: 6000);     // also CreateAmfH264, CreateAmfAv1
var encoder = VideoEncoder.CreateQsvH264("Video", bitrate: 6000);     // also CreateQsvHevc, CreateQsvAv1

// Audio - AAC, Opus, FLAC (lossless)
var encoder = AudioEncoder.CreateAac("Audio", bitrate: 192);
var encoder = AudioEncoder.CreateFlac("Audio");

// Record at a different resolution than the canvas, scaled on the GPU
encoder.SetGpuScaledSize(1920, 1080);   // e.g. 1440p canvas -> 1080p recording

// Record at a fraction of the canvas frame rate
encoder.FrameRateDivisor = 2;           // 60 FPS canvas -> 30 FPS file
```

### Encoder Discovery

Query which encoders exist on the user's machine before creating one:

```csharp
foreach (var e in EncoderInfo.GetVideoEncoders())
    Console.WriteLine($"{e.Id}: {e.DisplayName} [{e.Codec}, {e.Vendor}, HW={e.IsHardware}, HDR={e.SupportsHdr}]");

if (EncoderInfo.IsAvailable(VideoEncoder.Types.NvencH264)) { /* offer NVENC */ }

// Enumerate the valid options for an encoder property (presets, profiles, ...)
foreach (var (name, value) in EncoderInfo.GetListPropertyItems(VideoEncoder.Types.NvencH264, "preset"))
    Console.WriteLine($"{name}: {value}");

// HDR needs a 10-bit encoder (HEVC/AV1). Find the best match for the user's choice:
var hdrEncoder = EncoderInfo.FindHdrCapable(preferredEncoderId: VideoEncoder.Types.NvencH264);
// → same-vendor HEVC, then same-vendor AV1, then any HEVC/AV1, else null
```

## Virtual Camera

```csharp
// Requires the OBS virtual camera driver (bundled with OBS Studio)
if (VirtualCameraOutput.IsAvailable())
{
    using var virtualCam = new VirtualCameraOutput();
    virtualCam.Start();   // canvas is now visible as a system camera
}
```

## Performance Stats

```csharp
var stats = Obs.GetPerformanceStats();   // equivalent to the OBS stats dock
Console.WriteLine(stats);                // FPS, render time, lagged/skipped frames
if (stats.EncodingLagRatio > 0.05)
    Console.WriteLine("Encoder overloaded — lower the bitrate, resolution, or preset.");
```

## Streaming

```csharp
using ObsKit.NET.Outputs;
using ObsKit.NET.Services;

// Stream to Twitch
using var streaming = new StreamingOutput("My Stream")
    .ToTwitch("your_stream_key")
    .WithDefaultEncoders(videoBitrate: 4500, audioBitrate: 160);

// Stream to YouTube
using var streaming = new StreamingOutput("My Stream")
    .ToYouTube("your_stream_key")
    .WithDefaultEncoders(videoBitrate: 4500, audioBitrate: 160);

// Stream to custom RTMP server
using var streaming = new StreamingOutput("My Stream")
    .ToCustomServer("rtmp://live.example.com/app", "stream_key")
    .WithDefaultEncoders(videoBitrate: 4500, audioBitrate: 160);

// Stream over WHIP/WebRTC for sub-second latency (requires the obs-webrtc plugin; use Opus audio)
using var streaming = new StreamingOutput("My Stream")
    .ToWhipEndpoint("https://example.com/whip", bearerToken: "token");

// Full control with Service class
using var service = Service.CreateCustom("rtmp://live.example.com/app", "stream_key");
using var streaming = new StreamingOutput("My Stream")
    .WithService(service)
    .WithNvencEncoders(videoBitrate: 6000, audioBitrate: 160)
    .WithReconnect(enabled: true, retryDelaySec: 10, maxRetries: 20)
    .WithLowLatencyMode(enabled: true);

// Start streaming
streaming.Start();

// Monitor stream status
Console.WriteLine($"Streaming: {streaming.IsActive}");
Console.WriteLine($"Bytes sent: {streaming.TotalBytes}");
Console.WriteLine($"Frames dropped: {streaming.FramesDropped}");
Console.WriteLine($"Congestion: {streaming.Congestion:P0}");

// Stop streaming
streaming.Stop();
```

## Threading Model

Everything OBS tells you about happens on **OBS's internal threads**, not yours:

- Events (`Stopped`, `Saved`, `Hooked`/`Unhooked`, `LevelsUpdated`) and signal callbacks fire on OBS signal/graphics threads.
- Raw video callbacks run on the video thread; raw audio callbacks run on the audio thread (every ~21 ms at 48 kHz).

Three rules keep you safe:

1. **Never block in a callback.** A slow handler stalls rendering, encoding, or audio for the whole pipeline. Hand work off to a queue/`Channel` and return.
2. **Never touch UI directly.** Marshal to your UI thread first (`Dispatcher.Invoke`, `SynchronizationContext.Post`, `Control.BeginInvoke`).
3. **Copy frame/sample data out if you need it later.** The pointers inside `RawVideoFrame`/`RawAudioFrame` are only valid for the duration of the callback.

Also: don't dispose an object from inside its own callback (e.g. disposing a `SignalConnection` from the handler it invokes) — disconnect waits for in-flight callbacks to finish, so it deadlocks.

Calling ObsKit APIs from any of *your* threads is fine; libobs synchronizes internally.

## Object Lifetime & Ownership

Every wrapper owns a native OBS object and is `IDisposable`. The rules:

- **Dispose in reverse order of creation**: outputs → encoders → sources/scenes → `Obs` context last. `using var obs = Obs.Initialize(...)` at the top of your app handles shutdown correctly.
- **`Obs.AutoDispose` (default `true`)**: outputs are *one-shot* — `output.Stop()` detaches its encoders and disposes the output, so don't reuse the instance afterwards. Set `Obs.AutoDispose = false` if you want to start/stop the same output repeatedly and manage disposal yourself. (`ReplayBuffer.ResetAsync()` is safe under either setting.)
- **`takeOwnership` parameters** (`WithVideoEncoder(encoder, takeOwnership: true)`, `WithService(...)`): when `true`, the output disposes the encoder/service for you; when `false` (default), you keep that responsibility — useful when sharing one encoder between a recording and a replay buffer.
- **Keep subscriptions referenced.** `RawVideoSubscription`, `RawAudioSubscription`, `SourceAudioSubscription`, `SignalConnection`, `AudioMeter`, and `PreviewDisplay` hold native callbacks — store them in a field and dispose them when done; don't let them become unreachable while active.

## Diagnostics

```csharp
// Route OBS's internal log into your logger — the single most useful debugging tool.
// Most failures (missing plugin, bad encoder settings, capture errors) are explained here.
using var obs = Obs.Initialize(config => config
    .WithLogging((level, message) => Log.Information($"[OBS:{level}] {message}"))
    /* ... */);

// Outputs and encoders report failures via return values + LastError, not exceptions
if (!recording.Start())
    Console.WriteLine($"Start failed: {recording.LastError}");

// What actually loaded? (e.g. verify obs-browser / encoder plugins are present)
foreach (var m in Obs.GetLoadedModules())
    Console.WriteLine($"{m.FileName}: {m.Name}");

// Current canvas/output resolution and FPS
var info = Obs.GetVideoInfo();

// Validate an encoder/container combination before starting
if (!recording.SupportedVideoCodecs.Contains("hevc")) { /* fall back to h264 */ }
```

## DPI Awareness (Windows)

When using DXGI Desktop Duplication for monitor capture on Windows, your application must be configured as **per-monitor DPI aware**.

**For Windows Forms / WPF apps**, add to your `.csproj`:

```xml
<PropertyGroup>
  <ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>
</PropertyGroup>
```

**For console apps**, add an `app.manifest` file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
      <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/pm</dpiAware>
    </windowsSettings>
  </application>
</assembly>
```

And reference it in your `.csproj`:

```xml
<PropertyGroup>
  <ApplicationManifest>app.manifest</ApplicationManifest>
</PropertyGroup>
```

Alternatively, use `MonitorCaptureMethod.WindowsGraphicsCapture` which doesn't require DPI awareness.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "OBS runtime not found" | Ensure `obs.dll` / `libobs.so.0` / `libobs.0.dylib` is in the correct location |
| "Failed to find 'default.effect'" | The `data/libobs/` folder is missing |
| "Source ID 'xxx' not found" | Required plugin not loaded |
| Recording fails | Ensure `obs-ffmpeg-mux.exe` exists (Windows) and output path is writable |
| Module loading hangs | Use `.ForHeadlessOperation()` to exclude GUI modules |
| DXGI DuplicateOutput1 DPI error | See [DPI Awareness](#dpi-awareness-windows) section above |

## License

This project wraps OBS Studio which is licensed under GPLv2. See the [OBS Studio license](https://github.com/obsproject/obs-studio/blob/master/COPYING) for details.
