using ObsKit.NET;
using ObsKit.NET.Outputs;
using ObsKit.NET.Sources;

Console.WriteLine("ObsKit.NET - Replay Buffer Example");
Console.WriteLine("===================================\n");

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
using var obs = Obs.Initialize(config => config
    .WithDataPath(dataPath)
    .WithModulePath(pluginBinPath, pluginDataPath)
    .ForHeadlessOperation()
    .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    .WithAudio(a => a.WithSampleRate(48000)));

Console.WriteLine($"OBS {Obs.Version} initialized\n");

// Create a scene with monitor capture
using var scene = Obs.Scenes.Create("Replay Scene");
using var monitor = MonitorCapture.FromPrimary();
scene.AddSource(monitor);
scene.SetAsProgram(); // Set as the output source

Console.WriteLine($"Scene created with {scene.ItemCount} source(s)");

// Set up replay buffer
var outputDir = Environment.CurrentDirectory;

using var replayBuffer = new ReplayBuffer("My Replay Buffer", maxSeconds: 30, maxSizeMb: 512)
    .SetDirectory(outputDir)
    .SetFilenameFormat("Replay %CCYY-%MM-%DD %hh-%mm-%ss")
    .WithDefaultEncoders(videoBitrate: 6000, audioBitrate: 192);

Console.WriteLine($"Replay buffer: {replayBuffer.MaxSeconds} seconds");
Console.WriteLine($"Output directory: {outputDir}\n");

// Start the replay buffer
Console.WriteLine("Starting replay buffer...\n");

if (!replayBuffer.Start())
{
    Console.WriteLine($"Failed to start: {replayBuffer.LastError}");
    return;
}

Console.WriteLine("Replay buffer is running!");
Console.WriteLine("  Press [S] to save the replay");
Console.WriteLine("  Press [Q] to quit\n");

// Main loop - wait for user input
while (true)
{
    var key = Console.ReadKey(intercept: true);

    if (key.Key == ConsoleKey.S)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Saving replay...");

        // Note: The Save() method triggers saving via OBS's proc handler.
        // In a real application, you might use hotkeys or the frontend API.
        // For this demo, we show the API exists but actual saving requires
        // additional OBS frontend integration.
        try
        {
            replayBuffer.Save();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Replay saved to {outputDir}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {ex.Message}");
            Console.WriteLine("  Note: Programmatic saving requires obs-frontend-api integration.");
            Console.WriteLine("  The buffer is still recording - use OBS hotkeys to save if configured.");
        }
    }
    else if (key.Key == ConsoleKey.Q)
    {
        break;
    }
}

// Stop the replay buffer
replayBuffer.Stop();

Console.WriteLine($"\nReplay buffer stopped!");
Console.WriteLine($"  Total frames buffered: {replayBuffer.TotalFrames}");
Console.WriteLine($"  Total bytes: {replayBuffer.TotalBytes:N0}");
