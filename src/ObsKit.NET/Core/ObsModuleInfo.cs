namespace ObsKit.NET.Core;

/// <summary>
/// Describes a loaded OBS plugin module.
/// </summary>
/// <param name="FileName">The module file name (e.g. "obs-browser.dll").</param>
/// <param name="Name">The module's full name, if the module provides one.</param>
/// <param name="Author">The module's author(s), if provided.</param>
/// <param name="Description">The module's description, if provided.</param>
/// <param name="BinaryPath">Full path of the loaded module binary, if known.</param>
/// <param name="DataPath">Full path of the module's data directory, if known.</param>
public sealed record ObsModuleInfo(
    string FileName,
    string? Name,
    string? Author,
    string? Description,
    string? BinaryPath = null,
    string? DataPath = null);

/// <summary>
/// Describes a module file discovered in the module search paths (see <c>Obs.FindModules</c>),
/// whether or not it has been loaded.
/// </summary>
/// <param name="Name">The module name (file name without extension), as used by
/// <c>ObsConfiguration.ExcludeModule</c> and the safe/disabled module lists.</param>
/// <param name="BinaryPath">Full path of the module binary.</param>
/// <param name="DataPath">Full path of the module's data directory.</param>
public sealed record ObsModuleLocation(string Name, string BinaryPath, string DataPath);
