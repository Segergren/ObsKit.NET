using ObsKit.NET;
using ObsKit.NET.Outputs;
using ObsKit.NET.Signals;
using ObsKit.NET.Sources;

Console.WriteLine("ObsKit.NET - Recording Example");
Console.WriteLine("==============================\n");

// OBS runtime should be set up alongside the application.
// See README.md for instructions on setting up the OBS runtime.
//
// Expected structure:
//   MyApp/
//   ├── MyApp.exe
//   ├── obs.dll, obs-ffmpeg-mux.exe, libobs-d3d11.dll, etc.
//   ├── data/
//   │   └── libobs/
//   └── obs-plugins/
//       └── 64bit/

var obsPath = AppContext.BaseDirectory;
var dataPath = Path.Combine(obsPath, "data", "libobs");
var pluginBinPath = Path.Combine(obsPath, "obs-plugins", "64bit");
var pluginDataPath = Path.Combine(obsPath, "data", "obs-plugins", "%module%");

// Verify OBS runtime exists
if (!File.Exists(Path.Combine(obsPath, "obs.dll")))
{
    Console.WriteLine("ERROR: OBS runtime not found!");
    Console.WriteLine($"Expected obs.dll at: {obsPath}");
    Console.WriteLine("\nPlease set up the OBS runtime. See README.md for instructions.");
    return;
}

// Initialize OBS
// ForHeadlessOperation() excludes obs-browser and frontend-tools which can hang in CLI apps
// WithVideo/WithAudio configure initial video and audio settings
using var obs = Obs.Initialize(config => config
    .WithDataPath(dataPath)
    .WithModulePath(pluginBinPath, pluginDataPath)
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    .WithAudio(a => a.WithSampleRate(48000)));

// Get monitor info for dimensions
var primaryMonitor = MonitorCapture.AvailableMonitors.FirstOrDefault(m => m.IsPrimary)
                     ?? MonitorCapture.AvailableMonitors.First();
using var monitorSource = MonitorCapture.FromMonitor(primaryMonitor)
    .SetCaptureMethod(MonitorCaptureMethod.DesktopDuplication);

// You can also change video/audio settings after initialization using Obs.SetVideo/SetAudio.
// These use the same configuration options as WithVideo/WithAudio above.
// Note: Don't call these while recording - stop the output first.
//
// Example:
Obs.SetVideo(v => v.Resolution((uint)primaryMonitor.Width, (uint)primaryMonitor.Height).Fps(60));
Obs.SetAudio(a => a.WithSampleRate(48000));

Console.WriteLine($"OBS {Obs.Version} initialized\n");

// Create a scene with monitor capture
using var scene = Obs.Scenes.Create("Recording Scene");
scene.AddSource(monitorSource);
scene.SetAsProgram(); // Set as the output source for recording

Console.WriteLine($"Scene created with {scene.ItemCount} source(s)");

// Set up recording output
var outputPath = Path.Combine(Environment.CurrentDirectory, $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

using var recording = new RecordingOutput("My Recording")
    .SetPath(outputPath)
    .SetFormat(RecordingFormat.Mp4)
    .WithDefaultEncoders(videoBitrate: 6000, audioBitrate: 192);

Console.WriteLine($"Output: {outputPath}\n");

// Connect to output signals using strongly-typed enums
// Available signals: Start, Stop, Pause, Unpause, Starting, Stopping, etc.
using var stopSignal = recording.ConnectSignal(OutputSignal.Stop, calldata =>
{
    // The 'code' parameter indicates why the output stopped (0 = success)
    var code = Calldata.GetInt(calldata, "code");
    Console.WriteLine($"\n[Signal] Recording stopped with code: {code}");
});

// Start recording
Console.WriteLine("Starting recording... Press any key to stop.\n");

if (!recording.Start())
{
    Console.WriteLine($"Failed to start: {recording.LastError}");
    return;
}

Console.WriteLine("Recording in progress...");

// Wait for user to stop
Console.ReadKey(intercept: true);

// Stop recording
recording.Stop();

Console.WriteLine($"\nRecording stopped!");
Console.WriteLine($"  Total frames: {recording.TotalFrames}");
Console.WriteLine($"  Total bytes: {recording.TotalBytes:N0}");
Console.WriteLine($"  File: {outputPath}");