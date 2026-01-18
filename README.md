# ObsKit.NET

A modern .NET 9 wrapper for OBS Studio, providing a fluent C# API for video recording, streaming, and replay buffer functionality.

## Features

- **Cross-Platform** - Windows, Linux, and macOS support
- **Fluent API** - Clean, chainable configuration
- **Recording** - Record video to MP4, MKV, FLV, and more
- **Replay Buffer** - Keep a rolling buffer of the last N seconds
- **Sources** - Monitor capture, window capture, game capture, images, media files
- **Scenes** - Create and manage scenes with multiple sources
- **Encoders** - x264, NVENC (H.264/HEVC), AAC audio
- **Headless Operation** - Run without GUI dependencies

## Requirements

- .NET 9.0 or later
- OBS Studio runtime (see setup below)

### Platform-Specific Requirements

| Platform | Requirements |
|----------|-------------|
| Windows  | Windows 10/11 (64-bit) |
| Linux    | X11 (Wayland support is limited), PipeWire or PulseAudio |
| macOS    | macOS 11+ (Big Sur or later), Universal binary support |

## OBS Runtime Setup

ObsKit.NET requires OBS Studio binaries to function. These must be set up as a **standalone runtime** alongside your application.

### Using the Setup Script (Recommended)

Use the interactive setup script to download and configure the OBS runtime:

```bash
./tools/setup-obs-runtime.sh
```

The script will prompt you to:
1. Enter the OBS version (default: 31.0.0)
2. Select the platform (Windows, Linux, or macOS)
3. Choose the output directory

You can also pass arguments directly:

```bash
# Interactive mode
./tools/setup-obs-runtime.sh

# Specify version
./tools/setup-obs-runtime.sh 31.0.0

# Specify version and output path
./tools/setup-obs-runtime.sh 31.0.0 ./my-app/obs-runtime
```

---

## Windows Setup

### Automatic Setup

```bash
./tools/setup-obs-runtime.sh
# Select: 1) Windows (x64)
```

### Manual Setup

1. Download OBS Studio from [GitHub Releases](https://github.com/obsproject/obs-studio/releases)
   - File: `OBS-Studio-XX.X.X-Windows-x64.zip`

2. Extract and restructure as follows:

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
        ├── win-capture.dll
        ├── win-wasapi.dll
        └── ... (other plugins)
```

**Important:** The `bin/64bit` contents must be in the **root** directory (same folder as your .exe).

---

## Linux Setup

### Automatic Setup (Recommended)

```bash
./tools/setup-obs-runtime.sh
# Select: 2) Linux (x64)
```

This downloads and sets up the OBS runtime:

```
obs-runtime-linux/
├── lib/
│   └── libobs.so.0, libobs-frontend-api.so, etc.
├── obs-plugins/
│   └── obs-ffmpeg.so, obs-x264.so, etc.
└── data/
    └── libobs/, obs-plugins/
```

Copy this to your application's output directory.

### Manual Setup

1. Download OBS Studio from [GitHub Releases](https://github.com/obsproject/obs-studio/releases)
   - Look for `OBS-Studio-XX.X.X-Ubuntu-x86_64.tar.xz` or similar

2. Extract and restructure:

```
YourApp/
├── YourApp                      # Your .NET executable
├── lib/
│   └── libobs.so.0              # From OBS archive
├── obs-plugins/
│   └── obs-ffmpeg.so, etc.      # Plugin .so files
└── data/
    ├── libobs/                  # Shader files
    └── obs-plugins/             # Plugin data
```

### Configuration

```csharp
var obsPath = AppContext.BaseDirectory;

using var obs = Obs.Initialize(config => config
    .WithDataPath(Path.Combine(obsPath, "data", "libobs"))
    .WithModulePath(
        Path.Combine(obsPath, "obs-plugins"),
        Path.Combine(obsPath, "data", "obs-plugins", "%module%"))
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    .WithAudio(a => a.WithSampleRate(48000)));
```

### Runtime Dependencies

Install required system libraries:

```bash
# Ubuntu/Debian
sudo apt install libx11-6 libxrandr2 libpipewire-0.3-0 libpulse0 \
                 libavcodec-extra libavformat-dev libswscale-dev

# Fedora
sudo dnf install libX11 libXrandr pipewire-libs pulseaudio-libs \
                 ffmpeg-libs

# Arch Linux
sudo pacman -S libx11 libxrandr pipewire-pulse ffmpeg
```

### Running Your Application

Set `LD_LIBRARY_PATH` to include the OBS libraries:

```bash
cd /path/to/your/app
export LD_LIBRARY_PATH="$PWD/lib:$LD_LIBRARY_PATH"
./YourApp
```

Or create a launcher script:

```bash
#!/bin/bash
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
export LD_LIBRARY_PATH="$SCRIPT_DIR/lib:$LD_LIBRARY_PATH"
exec "$SCRIPT_DIR/YourApp" "$@"
```

---

## macOS Setup

### Automatic Setup (Recommended)

```bash
./tools/setup-obs-runtime.sh
# Select: 3) macOS (Universal)
```

On macOS, this mounts the DMG and copies OBS.app:

```
obs-runtime-macos/
└── OBS.app/
    └── Contents/
        ├── Frameworks/
        │   └── libobs.0.dylib, etc.
        ├── PlugIns/
        │   └── obs-ffmpeg.so, obs-x264.so, etc.
        └── Resources/
            └── data/
```

Copy the `OBS.app` folder to your application's output directory.

### Manual Setup

1. Download OBS Studio from [GitHub Releases](https://github.com/obsproject/obs-studio/releases)
   - File: `OBS-Studio-XX.X.X-macOS-Universal.dmg`

2. Mount the DMG and copy `OBS.app` to your app's directory:

```
YourApp/
├── YourApp                      # Your .NET executable
└── OBS.app/
    └── Contents/
        ├── Frameworks/          # libobs.0.dylib, etc.
        ├── PlugIns/             # Plugin .so files
        └── Resources/
            └── data/            # libobs/, obs-plugins/
```

### Configuration

```csharp
var obsPath = Path.Combine(AppContext.BaseDirectory, "OBS.app", "Contents");

using var obs = Obs.Initialize(config => config
    .WithDataPath(Path.Combine(obsPath, "Resources", "data", "libobs"))
    .WithModulePath(
        Path.Combine(obsPath, "PlugIns"),
        Path.Combine(obsPath, "Resources", "data", "obs-plugins", "%module%"))
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    .WithAudio(a => a.WithSampleRate(48000)));
```

### Running Your Application

Set `DYLD_LIBRARY_PATH` to include the OBS frameworks:

```bash
cd /path/to/your/app
export DYLD_LIBRARY_PATH="$PWD/OBS.app/Contents/Frameworks:$DYLD_LIBRARY_PATH"
./YourApp
```

**Note:** macOS screen recording requires permission in System Preferences > Privacy & Security > Screen Recording.

---

## Files to Exclude (Optional)

To reduce size, exclude these browser/frontend files:

| Windows | Linux | Description |
|---------|-------|-------------|
| `obs-browser.dll` | `obs-browser.so` | Browser source (Chromium) |
| `frontend-tools.dll` | `frontend-tools.so` | OBS frontend integration |
| `obs-websocket.dll` | `obs-websocket.so` | WebSocket server |
| `libcef.dll` | `libcef.so` | Chromium Embedded Framework |
| `locales/` | `locales/` | Browser localization |

The setup script automatically excludes these files.

---

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
    .SetFormat(RecordingFormat.Mp4)
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
using var monitor = MonitorCapture.FromMonitor(1);

// List available monitors
foreach (var m in MonitorCapture.AvailableMonitors)
    Console.WriteLine($"{m.Index}: {m.Name} ({m.Width}x{m.Height})");
```

### Window Capture

```csharp
// Capture by window info
var windows = WindowCapture.AvailableWindows;
using var window = WindowCapture.FromWindow(windows[0]);

// List available windows
foreach (var w in WindowCapture.AvailableWindows)
    Console.WriteLine($"{w.Title} ({w.ProcessName})");
```

### Game Capture (Windows Only)

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
// x264 (CPU) - All platforms
var encoder = VideoEncoder.CreateX264("Video", bitrate: 6000);

// With rate control options
var encoder = VideoEncoder.CreateX264("Video",
    bitrate: 6000,
    rateControl: RateControl.CRF,
    cqLevel: 23);

// NVENC H.264 (NVIDIA GPU) - Windows/Linux
var encoder = VideoEncoder.CreateNvencH264("Video", bitrate: 6000);

// NVENC HEVC (NVIDIA GPU) - Windows/Linux
var encoder = VideoEncoder.CreateNvencHevc("Video", bitrate: 6000);
```

### Audio Encoders

```csharp
// AAC - All platforms
var encoder = AudioEncoder.CreateAac("Audio", bitrate: 192);

// CoreAudio AAC (Windows/macOS)
var encoder = AudioEncoder.CreateCoreAudioAac("Audio", bitrate: 192);
```

## Samples

See the `samples/` directory for complete examples:

- **ObsKit.NET.Sample.Recording** - Basic recording example
- **ObsKit.NET.Sample.ReplayBuffer** - Replay buffer example

## Troubleshooting

### "OBS runtime not found"
Ensure the OBS runtime is set up correctly with `obs.dll` (Windows), `libobs.so.0` (Linux), or `libobs.0.dylib` (macOS) in the correct location.

### "Failed to find file 'default.effect'"
The `data/libobs/` folder is missing or the data path is incorrect.

### "Source ID 'xxx' not found"
The required plugin is not loaded. Ensure the plugins directory contains the necessary plugin files.

### Recording fails to start
- Ensure `obs-ffmpeg-mux.exe` (Windows) is in the application directory
- Check that video and audio encoders are properly configured
- Verify the output path is writable

### Module loading hangs
Use `.ForHeadlessOperation()` to exclude modules that require GUI (browser, frontend, websocket).

### Linux: X11 errors
Ensure X11 libraries are installed and you're running in an X11 session (not pure Wayland).

### macOS: Library not found
Ensure OBS.app is properly installed or the Frameworks path is correct.

## Platform-Specific Notes

### Windows
- Game Capture only works on Windows (uses DirectX hooks)
- DXGI Desktop Duplication may fail in some scenarios; use Windows Graphics Capture (WGC) instead

### Linux
- Monitor/window capture uses PipeWire (recommended) or X11
- Wayland support is limited; X11 fallback is used for window enumeration
- PulseAudio or PipeWire required for audio capture

### macOS
- Desktop audio capture requires additional setup (macOS restricts system audio capture)
- Screen recording requires user permission (Privacy & Security settings)

## License

This project wraps OBS Studio which is licensed under GPLv2. See the [OBS Studio license](https://github.com/obsproject/obs-studio/blob/master/COPYING) for details.
