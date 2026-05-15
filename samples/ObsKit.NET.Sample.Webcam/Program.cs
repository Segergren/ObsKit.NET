using ObsKit.NET;
using ObsKit.NET.Outputs;
using ObsKit.NET.Sources;

Console.WriteLine("ObsKit.NET - Webcam Recording Test");
Console.WriteLine("===================================\n");

var obsPath = AppContext.BaseDirectory;

if (!File.Exists(Path.Combine(obsPath, "obs.dll")))
{
    Console.WriteLine("ERROR: OBS runtime not found at " + obsPath);
    Console.WriteLine("Expected obs.dll alongside the executable. See README.md.");
    return 1;
}

using var obs = Obs.Initialize(config => config
    .WithDataPath(Path.Combine(obsPath, "data", "libobs"))
    .WithModulePath(
        Path.Combine(obsPath, "obs-plugins", "64bit"),
        Path.Combine(obsPath, "data", "obs-plugins", "%module%"))
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(30))
    .WithAudio(a => a.WithSampleRate(48000)));

Console.WriteLine($"OBS {Obs.Version} initialized\n");

// Enumerate webcams
var devices = WebcamCapture.ListDevices();
Console.WriteLine($"Available video capture devices ({devices.Count}):");
for (int i = 0; i < devices.Count; i++)
    Console.WriteLine($"  [{i}] {devices[i].Name}\n        id={devices[i].DeviceId}");

if (devices.Count == 0)
{
    Console.WriteLine("\nNo video capture devices found. Connect a webcam and try again.");
    return 1;
}

// Pick the Logitech 4K (BRIO) by name when present; otherwise fall back to the first device.
var chosen = devices.FirstOrDefault(d =>
                d.Name.Contains("BRIO", StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains("Logitech", StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains("4K", StringComparison.OrdinalIgnoreCase))
             ?? devices[0];

Console.WriteLine($"\nSelected: {chosen.Name}");

using var webcam = new WebcamCapture("Webcam", chosen.DeviceId);

// Give the device a moment to start producing frames; some webcams take a beat after open.
Console.WriteLine("Warming up capture device...");
for (int i = 0; i < 30 && webcam.Width == 0; i++)
    Thread.Sleep(100);

Console.WriteLine($"Capture stream: {webcam.Width}x{webcam.Height}");
if (webcam.Width == 0 || webcam.Height == 0)
{
    Console.WriteLine("WARN: webcam reports 0x0 — frames may not be flowing yet. Will continue anyway.");
}

// Match the canvas to the device's native resolution so we record what the camera produces.
var canvasW = webcam.Width > 0 ? webcam.Width : 1920u;
var canvasH = webcam.Height > 0 ? webcam.Height : 1080u;
Obs.SetVideo(v => v.Resolution(canvasW, canvasH).Fps(30));

// Build a scene with the webcam.
using var scene = Obs.Scenes.Create("Webcam Scene");
scene.AddSource(webcam);
scene.SetAsProgram();

Console.WriteLine($"Scene has {scene.ItemCount} source(s); canvas {canvasW}x{canvasH}\n");

// Sample a frame from the GPU before recording — confirms non-black content is reaching the canvas.
Thread.Sleep(500);
var probe = webcam.TakeScreenshot();
if (probe != null)
{
    long sum = 0;
    int sampleStride = Math.Max(1, probe.Pixels.Length / 4096);
    for (int i = 0; i < probe.Pixels.Length; i += sampleStride)
        sum += probe.Pixels[i];
    double avg = (double)sum / (probe.Pixels.Length / sampleStride);
    Console.WriteLine($"Pre-record screenshot: {probe.Width}x{probe.Height}, mean byte value = {avg:F1}");
    if (avg < 1.0)
        Console.WriteLine("WARN: screenshot looks black. The recording may still capture light if the device starts producing frames during record.");
}
else
{
    Console.WriteLine("Pre-record screenshot returned null.");
}

var outputPath = Path.Combine(Environment.CurrentDirectory, $"webcam_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
Console.WriteLine($"\nRecording to: {outputPath}");

using var recording = new RecordingOutput("Webcam Recording")
    .SetPath(outputPath)
    .SetFormat(RecordingFormat.Mp4)
    .WithDefaultEncoders(videoBitrate: 6000, audioBitrate: 192);

if (!recording.Start())
{
    Console.WriteLine($"Failed to start: {recording.LastError}");
    return 1;
}

// Record a fixed 5 seconds so the test is deterministic (no user input needed).
const int recordSeconds = 5;
Console.WriteLine($"Recording for {recordSeconds} seconds...");
for (int i = 0; i < recordSeconds; i++)
{
    Thread.Sleep(1000);
    Console.WriteLine($"  ...{i + 1}s  frames={recording.TotalFrames}  bytes={recording.TotalBytes:N0}");
}

var totalFrames = recording.TotalFrames;
var totalBytes = recording.TotalBytes;
recording.Stop(); // waits for completion, then auto-disposes when Obs.AutoDispose is true

Console.WriteLine($"\nStopped: frames={totalFrames}  bytes={totalBytes:N0}");

// Validate the output file exists and has plausible size.
if (!File.Exists(outputPath))
{
    Console.WriteLine("FAIL: no output file produced.");
    return 1;
}

var fi = new FileInfo(outputPath);
Console.WriteLine($"File: {outputPath} ({fi.Length:N0} bytes)");

if (totalFrames == 0)
{
    Console.WriteLine("FAIL: zero frames written to recording.");
    return 1;
}

Console.WriteLine("\nSUCCESS: webcam recording wrote frames. Open the MP4 to confirm content.");
return 0;
