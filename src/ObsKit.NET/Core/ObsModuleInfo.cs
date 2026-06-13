namespace ObsKit.NET.Core;

/// <summary>
/// Describes a loaded OBS plugin module.
/// </summary>
/// <param name="FileName">The module file name (e.g. "obs-browser.dll").</param>
/// <param name="Name">The module's full name, if the module provides one.</param>
/// <param name="Author">The module's author(s), if provided.</param>
/// <param name="Description">The module's description, if provided.</param>
public sealed record ObsModuleInfo(string FileName, string? Name, string? Author, string? Description);
