using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Encoder capability flags (OBS_ENCODER_CAP_* from libobs/obs-encoder.h).
/// </summary>
[Flags]
public enum EncoderCaps : uint
{
    /// <summary>No capability flags.</summary>
    None = 0,

    /// <summary>The encoder is deprecated and a newer alternative exists.</summary>
    Deprecated = 1 << 0,

    /// <summary>The encoder receives GPU textures directly (hardware encoder).</summary>
    PassTexture = 1 << 1,

    /// <summary>The encoder supports changing its bitrate while active.</summary>
    DynamicBitrate = 1 << 2,

    /// <summary>The encoder is internal and should not be shown to users.</summary>
    Internal = 1 << 3,

    /// <summary>The encoder supports region-of-interest encoding.</summary>
    RegionOfInterest = 1 << 4,

    /// <summary>The encoder can scale the video itself.</summary>
    Scaling = 1 << 5
}

/// <summary>
/// The vendor/implementation family of an encoder.
/// </summary>
public enum EncoderVendor
{
    /// <summary>Unknown vendor.</summary>
    Unknown,

    /// <summary>CPU/software implementation (x264, FFmpeg, AOM, SVT, ...).</summary>
    Software,

    /// <summary>NVIDIA NVENC.</summary>
    Nvidia,

    /// <summary>AMD AMF/VCE.</summary>
    Amd,

    /// <summary>Intel Quick Sync Video.</summary>
    Intel,

    /// <summary>Apple VideoToolbox.</summary>
    Apple
}

/// <summary>
/// Metadata about an encoder type registered with OBS.
/// Use the static methods to discover which encoders are available at runtime.
/// </summary>
public sealed class EncoderInfo
{
    private EncoderInfo(string id)
    {
        Id = id;
        DisplayName = ObsEncoder.obs_encoder_get_display_name(id);
        Codec = ObsEncoder.obs_get_encoder_codec(id);
        Type = ObsEncoder.obs_get_encoder_type(id);
        Caps = (EncoderCaps)ObsEncoder.obs_get_encoder_caps(id);
        Vendor = DetectVendor(id);
    }

    /// <summary>Gets the encoder type ID (e.g. "jim_nvenc", "obs_x264").</summary>
    public string Id { get; }

    /// <summary>Gets the human-readable encoder name.</summary>
    public string? DisplayName { get; }

    /// <summary>Gets the codec produced by the encoder (e.g. "h264", "hevc", "av1", "aac").</summary>
    public string? Codec { get; }

    /// <summary>Gets whether this is a video or audio encoder.</summary>
    public ObsEncoderType Type { get; }

    /// <summary>Gets the encoder capability flags.</summary>
    public EncoderCaps Caps { get; }

    /// <summary>Gets the vendor/implementation family, derived from the encoder ID.</summary>
    public EncoderVendor Vendor { get; }

    /// <summary>Gets whether this is a video encoder.</summary>
    public bool IsVideo => Type == ObsEncoderType.Video;

    /// <summary>Gets whether this is an audio encoder.</summary>
    public bool IsAudio => Type == ObsEncoderType.Audio;

    /// <summary>Gets whether the encoder is deprecated.</summary>
    public bool IsDeprecated => (Caps & EncoderCaps.Deprecated) != 0;

    /// <summary>Gets whether the encoder is internal and should not be shown to users.</summary>
    public bool IsInternal => (Caps & EncoderCaps.Internal) != 0;

    /// <summary>
    /// Gets whether the encoder is hardware-accelerated.
    /// </summary>
    public bool IsHardware => (Caps & EncoderCaps.PassTexture) != 0 ||
                              Vendor is EncoderVendor.Nvidia or EncoderVendor.Amd or EncoderVendor.Intel or EncoderVendor.Apple;

    /// <summary>
    /// Gets whether the encoder can encode 10-bit HDR video (Rec. 2100 with P010).
    /// OBS supports HDR with HEVC and AV1 encoders; H.264 is 8-bit only.
    /// </summary>
    public bool SupportsHdr => IsVideo && Codec is "hevc" or "av1";

    /// <summary>
    /// Gets metadata for an encoder type, or null if it is not registered
    /// (e.g. the GPU/driver does not provide it).
    /// </summary>
    /// <param name="encoderId">The encoder type ID.</param>
    public static EncoderInfo? Get(string encoderId)
    {
        return IsAvailable(encoderId) ? new EncoderInfo(encoderId) : null;
    }

    /// <summary>
    /// Gets whether an encoder type is registered and usable on this system.
    /// </summary>
    /// <param name="encoderId">The encoder type ID.</param>
    public static bool IsAvailable(string encoderId)
    {
        return Obs.EnumerateEncoderTypes().Contains(encoderId);
    }

    /// <summary>
    /// Enumerates the items of a list-type property exposed by an encoder type —
    /// e.g. the valid presets, profiles, or rate controls for the user's hardware.
    /// Returns (display name, value) pairs.
    /// </summary>
    /// <param name="encoderId">The encoder type ID (e.g. <c>VideoEncoder.Types.NvencH264</c>).</param>
    /// <param name="propertyName">The property key (e.g. "preset", "profile", "rate_control").</param>
    public static IReadOnlyList<(string Name, string Value)> GetListPropertyItems(string encoderId, string propertyName)
    {
        var result = new List<(string, string)>();
        var props = ObsProperties.obs_get_encoder_properties(encoderId);
        if (props == 0)
            return result;

        try
        {
            var prop = ObsProperties.obs_properties_get(props, propertyName);
            if (prop == 0)
                return result;

            var count = ObsProperties.obs_property_list_item_count(prop);
            for (nuint i = 0; i < count; i++)
            {
                var name = ObsProperties.obs_property_list_item_name(prop, i) ?? string.Empty;
                var value = ObsProperties.obs_property_list_item_string(prop, i) ?? string.Empty;
                result.Add((name, value));
            }
        }
        finally
        {
            ObsProperties.obs_properties_destroy(props);
        }

        return result;
    }

    /// <summary>
    /// Introspects every configurable property of an encoder type, in display order, with
    /// each property's name, label, type, state, numeric range, and (for list properties)
    /// its selectable items — e.g. the presets, profiles, and rate-control options available
    /// for the user's hardware. Useful for building dynamic encoder configuration UIs.
    /// </summary>
    /// <param name="encoderId">The encoder type ID (e.g. <c>VideoEncoder.Types.NvencH264</c>).</param>
    public static IReadOnlyList<ObsPropertyInfo> GetProperties(string encoderId)
        => ObsPropertyReader.ReadAllAndDestroy(ObsProperties.obs_get_encoder_properties(encoderId));

    /// <summary>
    /// Gets metadata for all registered encoders.
    /// </summary>
    /// <param name="includeInternal">Whether to include internal encoders.</param>
    public static IReadOnlyList<EncoderInfo> GetAll(bool includeInternal = false)
    {
        var encoders = Obs.EnumerateEncoderTypes()
            .Select(id => new EncoderInfo(id));

        if (!includeInternal)
            encoders = encoders.Where(e => !e.IsInternal);

        return encoders.ToList();
    }

    /// <summary>
    /// Gets metadata for all registered video encoders.
    /// </summary>
    public static IReadOnlyList<EncoderInfo> GetVideoEncoders()
    {
        return GetAll().Where(e => e.IsVideo).ToList();
    }

    /// <summary>
    /// Gets metadata for all registered audio encoders.
    /// </summary>
    public static IReadOnlyList<EncoderInfo> GetAudioEncoders()
    {
        return GetAll().Where(e => e.IsAudio).ToList();
    }

    /// <summary>
    /// Finds an encoder capable of 10-bit HDR encoding, preferring the given encoder.
    /// If the preferred encoder is not HDR-capable (e.g. H.264), an HEVC or AV1
    /// encoder from the same vendor is preferred, then any available HEVC/AV1 encoder.
    /// </summary>
    /// <param name="preferredEncoderId">The user's preferred video encoder type ID.</param>
    /// <returns>An HDR-capable encoder, or null if none is available.</returns>
    public static EncoderInfo? FindHdrCapable(string preferredEncoderId)
    {
        var preferred = Get(preferredEncoderId);
        if (preferred is { SupportsHdr: true })
            return preferred;

        var candidates = GetVideoEncoders()
            .Where(e => e.SupportsHdr && !e.IsDeprecated)
            .ToList();

        if (candidates.Count == 0)
            return null;

        var vendor = preferred?.Vendor ?? EncoderVendor.Unknown;

        return candidates.FirstOrDefault(e => e.Vendor == vendor && e.Codec == "hevc")
            ?? candidates.FirstOrDefault(e => e.Vendor == vendor && e.Codec == "av1")
            ?? candidates.FirstOrDefault(e => e.Codec == "hevc")
            ?? candidates.FirstOrDefault(e => e.Codec == "av1");
    }

    private static EncoderVendor DetectVendor(string id)
    {
        var lower = id.ToLowerInvariant();

        if (lower.Contains("nvenc"))
            return EncoderVendor.Nvidia;
        if (lower.Contains("amf"))
            return EncoderVendor.Amd;
        if (lower.Contains("qsv"))
            return EncoderVendor.Intel;
        if (lower.Contains("videotoolbox") || lower.StartsWith("com.apple"))
            return EncoderVendor.Apple;
        if (lower.Contains("x264") || lower.Contains("x265") || lower.Contains("aom") ||
            lower.Contains("svt") || lower.Contains("openh264") || lower.StartsWith("ffmpeg_") ||
            lower.StartsWith("coreaudio_") || lower.Contains("mf_aac"))
            return EncoderVendor.Software;

        return EncoderVendor.Unknown;
    }

    public override string ToString() => $"{DisplayName ?? Id} ({Codec}, {Vendor})";
}
