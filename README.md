# ObsKit.NET

A modern .NET 9 wrapper for OBS Studio, providing a fluent C# API for video recording, streaming, and replay buffer functionality.

## Features

- **Fluent API** - Clean, chainable configuration
- **Recording** - Record video to MP4, MKV, FLV, and more
- **Replay Buffer** - Keep a rolling buffer of the last N seconds
- **Sources** - Monitor capture, window capture, game capture, images, media files
- **Scenes** - Create and manage scenes with multiple sources
- **Encoders** - x264, NVENC (H.264/HEVC), AAC audio
- **Headless Operation** - Run without GUI dependencies

## Requirements

- .NET 9.0 or later
- Windows 10/11 (64-bit)
- OBS Studio runtime (see setup below)

## OBS Runtime Setup

ObsKit.NET requires OBS Studio binaries to function. These must be set up as a **standalone runtime** alongside your application.

### Option 1: Using the Setup Script (Recommended)

Use the setup script to automatically download and configure the OBS runtime:

**Bash (Git Bash):**
```bash
./tools/setup-obs-runtime.sh 32.0.4 ./obs-runtime
```

Then copy the contents to your application's output directory.

### Option 2: Manual Setup

1. Download OBS Studio from [GitHub Releases](https://github.com/obsproject/obs-studio/releases)
2. Extract and restructure as follows:

**Required Structure:**
```
YourApp/
├── YourApp.exe
├── obs.dll                      # From OBS bin/64bit/
├── obs-ffmpeg-mux.exe           # From OBS bin/64bit/ (required for recording)
├── libobs-d3d11.dll             # From OBS bin/64bit/
├── libobs-opengl.dll            # From OBS bin/64bit/
├── w32-pthreads.dll             # From OBS bin/64bit/
├── avcodec-61.dll               # From OBS bin/64bit/
├── avformat-61.dll              # From OBS bin/64bit/
├── avutil-59.dll                # From OBS bin/64bit/
├── swresample-5.dll             # From OBS bin/64bit/
├── swscale-8.dll                # From OBS bin/64bit/
├── ... (other DLLs from bin/64bit)
├── data/
│   ├── libobs/                  # Shader files (required)
│   └── obs-plugins/             # Plugin data files
└── obs-plugins/
    └── 64bit/                   # Plugin DLLs
        ├── obs-ffmpeg.dll
        ├── obs-x264.dll
        ├── coreaudio-encoder.dll
        ├── win-capture.dll
        ├── win-wasapi.dll
        └── ... (other plugins)
```

**Important:** The `bin/64bit` contents must be in the **root** directory (same as your .exe), not in a `bin/64bit` subfolder. This is because OBS looks for helper executables (like `obs-ffmpeg-mux.exe`) relative to the running process.

**Files to Exclude** (optional, reduces size):
- `obs-browser.dll`, `obs-browser-page.exe` - Browser source (requires Chromium)
- `frontend-tools.dll` - OBS frontend integration
- `obs-websocket.dll` - WebSocket server
- `libcef.dll`, `chrome_elf.dll` - Chromium dependencies
- `locales/` folder - Browser localization

## Quick Start

```csharp
using ObsKit.NET;
using ObsKit.NET.Outputs;
using ObsKit.NET.Sources;

// OBS runtime is expected in the application directory
var obsPath = AppContext.BaseDirectory;

// Initialize OBS
using var obs = Obs.Initialize(config => config
    .WithDataPath(Path.Combine(obsPath, "data", "libobs"))
    .WithModulePath(
        Path.Combine(obsPath, "obs-plugins", "64bit"),
        Path.Combine(obsPath, "data", "obs-plugins", "%module%"))
    .ForHeadlessOperation()  // Excludes browser/frontend modules
    .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    .WithAudio(a => a.WithSampleRate(48000)));

Console.WriteLine($"OBS {Obs.Version} initialized");

// Create a scene with monitor capture
using var scene = Obs.Scenes.Create("My Scene");
using var monitor = MonitorCapture.FromPrimary();
scene.AddSource(monitor);
scene.SetAsProgram(); // Set as output source

// Set up recording
using var recording = new RecordingOutput("My Recording")
    .SetPath("output.mp4")
    .SetFormat("mp4")
    .WithDefaultEncoders(videoBitrate: 6000, audioBitrate: 192);

// Start recording
recording.Start();
Console.WriteLine("Recording... Press any key to stop.");
Console.ReadKey();

// Stop recording
recording.Stop();
Console.WriteLine($"Recorded {recording.TotalFrames} frames");
```

## Configuration Options

### Video Settings

```csharp
.WithVideo(v => v
    .Resolution(1920, 1080)      // Base and output resolution
    .BaseResolution(2560, 1440)  // Canvas resolution
    .OutputResolution(1920, 1080) // Output resolution (scaled)
    .Fps(60)                      // Frame rate
    .Fps(60000, 1001))            // NTSC frame rate (59.94)
```

### Audio Settings

```csharp
.WithAudio(a => a
    .WithSampleRate(48000)        // 44100 or 48000
    .WithSpeakers(SpeakerLayout.Stereo))
```

### Module Exclusions

```csharp
// Exclude specific modules
.ExcludeBrowserSource()    // Excludes obs-browser (Chromium)
.ExcludeFrontendTools()    // Excludes frontend-tools
.ExcludeWebSocket()        // Excludes obs-websocket
.ExcludeModule("custom")   // Exclude by name

// Or use the convenience method for headless apps
.ForHeadlessOperation()    // Excludes browser, frontend, websocket
```

## Source Types

### Monitor Capture

```csharp
// Capture primary monitor
using var monitor = MonitorCapture.FromPrimary();

// Capture specific monitor by index
using var monitor = MonitorCapture.FromIndex(1);
```

### Window Capture

```csharp
// Capture by window title
using var window = WindowCapture.FromTitle("Notepad");

// Capture by process name
using var window = WindowCapture.FromProcess("notepad");
```

### Game Capture

```csharp
// Capture any fullscreen game
using var game = new GameCapture("Game", GameCapture.CaptureMode.AnyFullscreen);

// Capture specific game
using var game = new GameCapture("Game", GameCapture.CaptureMode.SpecificWindow)
    .SetWindow("My Game");
```

### Image Source

```csharp
using var image = ImageSource.FromFile("logo.png");
```

### Media Source

```csharp
using var media = new MediaSource("Video", "video.mp4")
    .SetLooping(true);
```

## Encoders

### Video Encoders

```csharp
// x264 (CPU)
var encoder = VideoEncoder.CreateX264("Video", bitrate: 6000);

// NVENC H.264 (NVIDIA GPU)
var encoder = VideoEncoder.CreateNvencH264("Video", bitrate: 6000);

// NVENC HEVC (NVIDIA GPU)
var encoder = VideoEncoder.CreateNvencHevc("Video", bitrate: 6000);
```

### Audio Encoders

```csharp
// AAC
var encoder = AudioEncoder.CreateAac("Audio", bitrate: 192);

// CoreAudio AAC (Windows)
var encoder = AudioEncoder.CreateCoreAudioAac("Audio", bitrate: 192);
```

## Samples

See the `samples/` directory for complete examples:

- **ObsKit.NET.Sample.Recording** - Basic recording example
- **ObsKit.NET.Sample.ReplayBuffer** - Replay buffer example

## Troubleshooting

### "OBS runtime not found"
Ensure the OBS runtime is set up correctly with `obs.dll` in the application directory.

### "Failed to find file 'default.effect'"
The `data/libobs/` folder is missing or the data path is incorrect.

### "Source ID 'xxx' not found"
The required plugin is not loaded. Ensure `obs-plugins/64bit/` contains the necessary plugin DLLs.

### Recording fails to start
- Ensure `obs-ffmpeg-mux.exe` is in the application directory (same folder as your .exe)
- Check that video and audio encoders are properly configured
- Verify the output path is writable

### Module loading hangs
Use `.ForHeadlessOperation()` to exclude modules that require GUI (browser, frontend, websocket).

## License

This project wraps OBS Studio which is licensed under GPLv2. See the [OBS Studio license](https://github.com/obsproject/obs-studio/blob/master/COPYING) for details.
