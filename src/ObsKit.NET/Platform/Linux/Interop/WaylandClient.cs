using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.Linux.Interop;

/// <summary>
/// P/Invoke bindings for libwayland-client.
/// </summary>
[SupportedOSPlatform("linux")]
internal static partial class WaylandClient
{
    private const string LibWaylandClient = "libwayland-client.so.0";

    [LibraryImport(LibWaylandClient, EntryPoint = "wl_display_connect")]
    internal static partial nint wl_display_connect(nint name);
}
