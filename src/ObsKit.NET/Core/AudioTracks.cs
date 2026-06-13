namespace ObsKit.NET.Core;

/// <summary>
/// Helpers for converting between 1-based audio track numbers (1-6, as shown in the
/// OBS UI) and the mixer bitmasks used by libobs.
/// </summary>
public static class AudioTracks
{
    /// <summary>
    /// The maximum number of audio tracks/mixers supported by OBS (MAX_AUDIO_MIXES).
    /// </summary>
    public const int MaxTracks = 6;

    /// <summary>
    /// A mask with all six audio tracks enabled.
    /// </summary>
    public const uint AllTracksMask = (1u << MaxTracks) - 1;

    /// <summary>
    /// Builds a mixer bitmask from 1-based track numbers.
    /// </summary>
    /// <param name="tracks">The track numbers (1-6).</param>
    /// <returns>The mixer bitmask.</returns>
    public static uint ToMask(params int[] tracks)
    {
        uint mask = 0;
        foreach (var track in tracks)
        {
            ValidateTrack(track);
            mask |= 1u << (track - 1);
        }
        return mask;
    }

    /// <summary>
    /// Builds a mixer bitmask covering tracks 1 through <paramref name="count"/>.
    /// </summary>
    /// <param name="count">The number of tracks (0-6).</param>
    /// <returns>The mixer bitmask.</returns>
    public static uint FirstTracksMask(int count)
    {
        if (count is < 0 or > MaxTracks)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Track count must be between 0 and {MaxTracks}.");

        return (1u << count) - 1;
    }

    /// <summary>
    /// Extracts the 1-based track numbers from a mixer bitmask.
    /// </summary>
    /// <param name="mask">The mixer bitmask.</param>
    /// <returns>The enabled track numbers in ascending order.</returns>
    public static IReadOnlyList<int> FromMask(uint mask)
    {
        var tracks = new List<int>(MaxTracks);
        for (var track = 1; track <= MaxTracks; track++)
        {
            if ((mask & (1u << (track - 1))) != 0)
                tracks.Add(track);
        }
        return tracks;
    }

    internal static void ValidateTrack(int track)
    {
        if (track is < 1 or > MaxTracks)
            throw new ArgumentOutOfRangeException(nameof(track), track, $"Track number must be between 1 and {MaxTracks}.");
    }
}
