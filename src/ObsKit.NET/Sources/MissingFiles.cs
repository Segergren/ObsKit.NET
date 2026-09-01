using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Sources;

/// <summary>
/// A file a source depends on that could not be found (e.g. a moved image or media file),
/// as reported by <see cref="Source.GetMissingFiles"/>.
/// </summary>
public sealed class MissingFile
{
    private readonly MissingFileCollection _owner;
    private readonly nint _file;

    internal MissingFile(MissingFileCollection owner, nint file, string path, string? sourceName)
    {
        _owner = owner;
        _file = file;
        Path = path;
        SourceName = sourceName;
    }

    /// <summary>The path the source is currently configured with.</summary>
    public string Path { get; }

    /// <summary>The name of the source that references the file.</summary>
    public string? SourceName { get; }

    /// <summary>
    /// Points the source at a replacement file. The source updates its settings immediately.
    /// </summary>
    /// <param name="newPath">The new file path.</param>
    public void Resolve(string newPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(newPath);
        _owner.ThrowIfDisposed();
        ObsSource.obs_missing_file_issue_callback(_file, newPath);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{SourceName}: {Path}";
}

/// <summary>
/// The missing files reported by a source (obs_missing_files_t). Dispose when done; the
/// entries are only valid while the collection is alive.
/// </summary>
public sealed class MissingFileCollection : IDisposable, IReadOnlyList<MissingFile>
{
    private nint _files;
    private readonly List<MissingFile> _entries = new();
    private bool _disposed;

    internal MissingFileCollection(nint files)
    {
        _files = files;
        if (files == nint.Zero)
            return;

        var count = (int)ObsSource.obs_missing_files_count(files);
        for (var i = 0; i < count; i++)
        {
            var file = ObsSource.obs_missing_files_get_file(files, i);
            if (file == nint.Zero)
                continue;
            _entries.Add(new MissingFile(
                this,
                file,
                ObsSource.obs_missing_file_get_path(file) ?? string.Empty,
                ObsSource.obs_missing_file_get_source_name(file)));
        }
    }

    /// <inheritdoc/>
    public int Count => _entries.Count;

    /// <inheritdoc/>
    public MissingFile this[int index] => _entries[index];

    /// <inheritdoc/>
    public IEnumerator<MissingFile> GetEnumerator() => _entries.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>Releases the native collection.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~MissingFileCollection()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // Entries hold only bmalloc'd strings and a borrowed source pointer, so destroying is
        // safe regardless of core state.
        if (_files != nint.Zero)
            ObsSource.obs_missing_files_destroy(_files);
        _files = nint.Zero;
    }
}
