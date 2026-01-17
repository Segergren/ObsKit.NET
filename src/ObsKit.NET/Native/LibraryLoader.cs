using System.Reflection;
using System.Runtime.InteropServices;

namespace ObsKit.NET.Native;

/// <summary>
/// Handles platform-specific library loading for OBS native libraries.
/// </summary>
internal static partial class LibraryLoader
{
    private static bool _initialized;
    private static bool _dllDirectorySet;

    /// <summary>
    /// The name used in LibraryImport attributes.
    /// </summary>
    internal const string ObsLibraryName = "obs";

    /// <summary>
    /// Initialize the library loader with custom resolution.
    /// Automatically uses AppContext.BaseDirectory as the library path.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
            return;

        // Use the application's base directory as the library path
        var appPath = AppContext.BaseDirectory;
        CustomLibraryPath = appPath;

        if (Directory.Exists(appPath))
        {
            // On Windows, add the directory to the DLL search path
            // This is necessary because OBS loads additional DLLs at runtime (libobs-d3d11.dll, etc.)
            if (OperatingSystem.IsWindows() && !_dllDirectorySet)
            {
                AddDllDirectory(appPath);
                SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
                _dllDirectorySet = true;
            }

            // Add to PATH environment variable so OBS helper processes can be found
            // (e.g., obs-ffmpeg-mux.exe for recording)
            AddToPath(appPath);
        }

        NativeLibrary.SetDllImportResolver(typeof(LibraryLoader).Assembly, ResolveLibrary);
        _initialized = true;
    }

    private static void AddToPath(string directory)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

        // Check if already in PATH (case-insensitive on Windows)
        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var paths = currentPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in paths)
        {
            if (p.Equals(directory, comparison))
                return; // Already in PATH
        }

        // Prepend the directory to PATH
        var newPath = directory + separator + currentPath;
        Environment.SetEnvironmentVariable("PATH", newPath);
    }

    /// <summary>
    /// Custom library path set by the user.
    /// </summary>
    internal static string? CustomLibraryPath { get; private set; }

    private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != ObsLibraryName)
            return 0;

        // Try custom path first
        if (!string.IsNullOrEmpty(CustomLibraryPath))
        {
            var customPath = GetPlatformLibraryPath(CustomLibraryPath);
            if (NativeLibrary.TryLoad(customPath, out var customHandle))
                return customHandle;
        }

        // Try default platform library name
        var defaultName = GetDefaultLibraryName();
        if (NativeLibrary.TryLoad(defaultName, assembly, searchPath, out var handle))
            return handle;

        // Try common installation paths
        foreach (var path in GetCommonPaths())
        {
            if (File.Exists(path))
            {
                // Add the directory to DLL search path before loading
                var dir = Path.GetDirectoryName(path);
                if (OperatingSystem.IsWindows() && !_dllDirectorySet && dir != null)
                {
                    AddDllDirectory(dir);
                    SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
                    _dllDirectorySet = true;
                }

                if (NativeLibrary.TryLoad(path, out handle))
                    return handle;
            }
        }

        return 0;
    }

    private static string GetPlatformLibraryPath(string basePath)
    {
        var libraryName = GetDefaultLibraryName();
        return Path.Combine(basePath, libraryName);
    }

    private static string GetDefaultLibraryName()
    {
        if (OperatingSystem.IsWindows())
            return "obs.dll";
        if (OperatingSystem.IsLinux())
            return "libobs.so.0";
        if (OperatingSystem.IsMacOS())
            return "libobs.0.dylib";

        throw new PlatformNotSupportedException("Unsupported operating system");
    }

    private static IEnumerable<string> GetCommonPaths()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return @"C:\Program Files\obs-studio\bin\64bit\obs.dll";
            yield return @"C:\Program Files (x86)\obs-studio\bin\64bit\obs.dll";

            // Check PATH environment variable
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var dir in pathEnv.Split(';'))
                {
                    var obsPath = Path.Combine(dir.Trim(), "obs.dll");
                    if (File.Exists(obsPath))
                        yield return obsPath;
                }
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            yield return "/usr/lib/libobs.so.0";
            yield return "/usr/lib64/libobs.so.0";
            yield return "/usr/local/lib/libobs.so.0";
            yield return "/usr/lib/x86_64-linux-gnu/libobs.so.0";
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return "/Applications/OBS.app/Contents/Frameworks/libobs.0.dylib";
            yield return "/usr/local/lib/libobs.0.dylib";
            yield return "/opt/homebrew/lib/libobs.0.dylib";
        }
    }

    #region Windows DLL Directory APIs

    private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

    [LibraryImport("kernel32.dll", EntryPoint = "AddDllDirectory", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint AddDllDirectory(string newDirectory);

    [LibraryImport("kernel32.dll", EntryPoint = "SetDefaultDllDirectories")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetDefaultDllDirectories(uint directoryFlags);

    #endregion
}
