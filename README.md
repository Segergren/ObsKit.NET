# ObsKit.NET

A modern .NET 9 wrapper for OBS Studio, providing a fluent C# API for video recording, streaming, and replay buffer functionality.

## Features

- **Cross-Platform** - Windows, Linux, and macOS support
- **Fluent API** - Clean, chainable configuration
- **Recording** - Record video to MP4, MKV, FLV, and more
- **Replay Buffer** - Keep a rolling buffer of the last N seconds
- **Sources** - Monitor capture, window capture, game capture, images, media files
- **Encoders** - x264, NVENC (H.264/HEVC), AAC audio
- **Headless Operation** - Run without GUI dependencies

## Requirements

- .NET 9.0 or later
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

recording.Stop();
Console.WriteLine($"Recorded {recording.TotalFrames} frames");
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

// Game capture (Windows only)
using var game = new GameCapture("Game", GameCapture.CaptureMode.AnyFullscreen);

// Image and media
using var image = ImageSource.FromFile("logo.png");
using var media = new MediaSource("Video", "video.mp4").SetLooping(true);
```

## Encoders

```csharp
// Video - x264 (CPU)
var encoder = VideoEncoder.CreateX264("Video", bitrate: 6000);

// Video - NVENC (NVIDIA GPU)
var encoder = VideoEncoder.CreateNvencH264("Video", bitrate: 6000);
var encoder = VideoEncoder.CreateNvencHevc("Video", bitrate: 6000);

// Audio - AAC
var encoder = AudioEncoder.CreateAac("Audio", bitrate: 192);
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "OBS runtime not found" | Ensure `obs.dll` / `libobs.so.0` / `libobs.0.dylib` is in the correct location |
| "Failed to find 'default.effect'" | The `data/libobs/` folder is missing |
| "Source ID 'xxx' not found" | Required plugin not loaded |
| Recording fails | Ensure `obs-ffmpeg-mux.exe` exists (Windows) and output path is writable |
| Module loading hangs | Use `.ForHeadlessOperation()` to exclude GUI modules |

## License

This project wraps OBS Studio which is licensed under GPLv2. See the [OBS Studio license](https://github.com/obsproject/obs-studio/blob/master/COPYING) for details.
