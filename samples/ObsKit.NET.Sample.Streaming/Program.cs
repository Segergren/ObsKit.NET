using ObsKit.NET;
using ObsKit.NET.Outputs;
using ObsKit.NET.Sources;

Console.WriteLine("ObsKit.NET - Streaming Example");
Console.WriteLine("==============================\n");

// OBS runtime should be set up alongside the application.
// See README.md for instructions on setting up the OBS runtime.

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
using var obs = Obs.Initialize(config => config
    .WithDataPath(dataPath)
    .WithModulePath(pluginBinPath, pluginDataPath)
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(30)) // 30 FPS is common for streaming
    .WithAudio(a => a.WithSampleRate(48000)));

Console.WriteLine($"OBS {Obs.Version} initialized\n");

// Set up capture source (monitor capture in this example)
var primaryMonitor = MonitorCapture.AvailableMonitors.FirstOrDefault(m => m.IsPrimary)
                     ?? MonitorCapture.AvailableMonitors.First();
using var monitorSource = MonitorCapture.FromMonitor(primaryMonitor)
    .SetCaptureMethod(MonitorCaptureMethod.DesktopDuplication);

// Adjust video resolution to match monitor
Obs.SetVideo(v => v.Resolution((uint)primaryMonitor.Width, (uint)primaryMonitor.Height).Fps(30));

// Set up audio sources
using var audioInput = AudioInputCapture.FromDefault();
using var audioOutput = AudioOutputCapture.FromDefault();

// Create a scene with monitor capture
using var scene = Obs.Scenes.Create("Streaming Scene");
scene.AddSource(monitorSource);
scene.AddSource(audioInput);
scene.AddSource(audioOutput);
scene.SetAsProgram();

Console.WriteLine($"Scene created with {scene.ItemCount} source(s)\n");

// ========================================
// STREAMING CONFIGURATION
// ========================================
// Replace these values with your actual stream key and server!

// Option 1: Stream to a custom RTMP server
//const string rtmpServer = "rtmp://localhost/live"; // Your RTMP server URL
//const string streamKey = "test-stream-key";        // Your stream key

// Option 2: Stream to Twitch (uncomment and set your stream key)
const string twitchStreamKey = "live_xxxxxxxx_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

// Option 3: Stream to YouTube (uncomment and set your stream key)
// const string youtubeStreamKey = "xxxx-xxxx-xxxx-xxxx-xxxx";

// Create streaming output
using var streaming = new StreamingOutput("My Stream")
    // Configure destination - choose ONE of the following:

    // Custom RTMP server:
    //.ToCustomServer(rtmpServer, streamKey)

    // Or Twitch:
    .ToTwitch(twitchStreamKey)

    // Or YouTube:
    // .ToYouTube(youtubeStreamKey)

    // Or Facebook:
    // .ToFacebook(facebookStreamKey)

    // Or use a Service directly for full control:
    // .WithService(Service.CreateCustom(rtmpServer, streamKey))

    // Configure encoders
    // For streaming, lower bitrates are typically used (4500 kbps is common for 1080p)
    // This is the equivalent of setting the encoders above like
    // .WithVideoEncoder(VideoEncoder.CreateH264("Streaming Video", 4500, 1920, 1080, 30, "veryfast"))
    // .WithAudioEncoder(AudioEncoder.CreateAac("Streaming Audio", 160))
    .WithDefaultEncoders(videoBitrate: 4500, audioBitrate: 160, preset: "veryfast")

    // Or use NVENC if you have an NVIDIA GPU:
    // .WithNvencEncoders(videoBitrate: 6000, audioBitrate: 160)

    // Optional: Configure reconnection
    .WithReconnect(enabled: true, retryDelaySec: 10, maxRetries: 20)

    // Optional: Enable low latency mode for reduced delay
    .WithLowLatencyMode(enabled: true);

Console.WriteLine("Streaming Configuration:");
Console.WriteLine($"  Server: {streaming.Url}");
Console.WriteLine($"  Can connect: {streaming.Service?.CanConnect}");
Console.WriteLine();

// Connect to streaming events
using var startedHandler = streaming.OnStarted(() =>
{
    Console.WriteLine("[Event] Stream started successfully!");
});

using var stoppedHandler = streaming.OnStopped(code =>
{
    Console.WriteLine($"[Event] Stream stopped with code: {code}");
});

using var reconnectingHandler = streaming.OnReconnecting(() =>
{
    Console.WriteLine("[Event] Connection lost, attempting to reconnect...");
});

using var reconnectedHandler = streaming.OnReconnected(() =>
{
    Console.WriteLine("[Event] Reconnected successfully!");
});

// Start streaming
Console.WriteLine("Starting stream... Press any key to stop.\n");

if (!streaming.Start())
{
    Console.WriteLine($"Failed to start stream: {streaming.LastError}");
    return;
}

Console.WriteLine("Streaming in progress...\n");

// Display stats periodically
var cts = new CancellationTokenSource();
var statsTask = Task.Run(async () =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        await Task.Delay(2000, cts.Token).ConfigureAwait(false);
        if (streaming.IsActive)
        {
            var mbSent = streaming.TotalBytes / 1024.0 / 1024.0;
            Console.WriteLine($"  Stats: {streaming.TotalFrames} frames, {mbSent:F2} MB sent, {streaming.FramesDropped} dropped, congestion: {streaming.Congestion:P0}, delay: {streaming.ActiveDelay}");
        }
    }
}, cts.Token);

// Wait for user to stop
Console.ReadKey(intercept: true);
cts.Cancel();

// Stop streaming
Console.WriteLine("\nStopping stream...");
streaming.Stop();

Console.WriteLine("\nStream ended!");
Console.WriteLine($"  Total frames: {streaming.TotalFrames}");
Console.WriteLine($"  Total bytes: {streaming.TotalBytes:N0}");
Console.WriteLine($"  Frames dropped: {streaming.FramesDropped}");
