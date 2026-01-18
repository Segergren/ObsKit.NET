using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.MacOS.Interop;

/// <summary>
/// P/Invoke bindings for macOS CoreFoundation framework.
/// </summary>
[SupportedOSPlatform("macos")]
internal static partial class CoreFoundation
{
    private const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    #region CFArray

    /// <summary>
    /// Gets the count of elements in a CFArray.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFArrayGetCount")]
    internal static partial nint CFArrayGetCount(nint theArray);

    /// <summary>
    /// Gets the value at the specified index in a CFArray.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFArrayGetValueAtIndex")]
    internal static partial nint CFArrayGetValueAtIndex(nint theArray, nint idx);

    #endregion

    #region CFDictionary

    /// <summary>
    /// Gets a value from a CFDictionary.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFDictionaryGetValue")]
    internal static partial nint CFDictionaryGetValue(nint theDict, nint key);

    /// <summary>
    /// Gets a value from a CFDictionary if it exists.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFDictionaryGetValueIfPresent")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CFDictionaryGetValueIfPresent(nint theDict, nint key, out nint value);

    #endregion

    #region CFString

    /// <summary>
    /// Creates a CFString from a C string.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFStringCreateWithCString", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint CFStringCreateWithCString(nint alloc, string cStr, uint encoding);

    /// <summary>
    /// Gets the length of a CFString.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFStringGetLength")]
    internal static partial nint CFStringGetLength(nint theString);

    /// <summary>
    /// Gets the C string pointer from a CFString.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFStringGetCStringPtr")]
    internal static partial nint CFStringGetCStringPtr(nint theString, uint encoding);

    /// <summary>
    /// Gets the C string from a CFString into a buffer.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFStringGetCString")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CFStringGetCString(nint theString, nint buffer, nint bufferSize, uint encoding);

    /// <summary>
    /// Gets a string value from a CFString.
    /// </summary>
    internal static string? GetString(nint cfString)
    {
        if (cfString == 0)
            return null;

        var ptr = CFStringGetCStringPtr(cfString, kCFStringEncodingUTF8);
        if (ptr != 0)
        {
            return Marshal.PtrToStringUTF8(ptr);
        }

        // Fallback: copy string to buffer
        var length = CFStringGetLength(cfString);
        if (length == 0)
            return "";

        var bufferSize = length * 4 + 1; // UTF-8 can be up to 4 bytes per character
        var buffer = Marshal.AllocHGlobal((int)bufferSize);
        try
        {
            if (CFStringGetCString(cfString, buffer, bufferSize, kCFStringEncodingUTF8))
            {
                return Marshal.PtrToStringUTF8(buffer);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return null;
    }

    #endregion

    #region CFNumber

    /// <summary>
    /// Gets a value from a CFNumber.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFNumberGetValue")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CFNumberGetValue(nint number, int theType, out int valuePtr);

    /// <summary>
    /// Gets a value from a CFNumber.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFNumberGetValue")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CFNumberGetValue(nint number, int theType, out long valuePtr);

    /// <summary>
    /// Gets a value from a CFNumber.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFNumberGetValue")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CFNumberGetValue(nint number, int theType, out double valuePtr);

    internal static int? GetInt(nint cfNumber)
    {
        if (cfNumber == 0)
            return null;
        if (CFNumberGetValue(cfNumber, kCFNumberSInt32Type, out int value))
            return value;
        return null;
    }

    internal static long? GetLong(nint cfNumber)
    {
        if (cfNumber == 0)
            return null;
        if (CFNumberGetValue(cfNumber, kCFNumberSInt64Type, out long value))
            return value;
        return null;
    }

    internal static double? GetDouble(nint cfNumber)
    {
        if (cfNumber == 0)
            return null;
        if (CFNumberGetValue(cfNumber, kCFNumberFloat64Type, out double value))
            return value;
        return null;
    }

    #endregion

    #region CFBoolean

    /// <summary>
    /// Gets the value of a CFBoolean.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFBooleanGetValue")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CFBooleanGetValue(nint boolean);

    #endregion

    #region Memory

    /// <summary>
    /// Releases a Core Foundation object.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFRelease")]
    internal static partial void CFRelease(nint cf);

    /// <summary>
    /// Retains a Core Foundation object.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CFRetain")]
    internal static partial nint CFRetain(nint cf);

    #endregion

    #region CGRect from Dictionary

    /// <summary>
    /// Creates a CGRect from a dictionary.
    /// </summary>
    [LibraryImport(CoreFoundationLib, EntryPoint = "CGRectMakeWithDictionaryRepresentation")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CGRectMakeWithDictionaryRepresentation(nint dict, out CoreGraphics.CGRect rect);

    #endregion

    #region Constants

    // CFString encoding
    internal const uint kCFStringEncodingUTF8 = 0x08000100;
    internal const uint kCFStringEncodingASCII = 0x0600;

    // CFNumber types
    internal const int kCFNumberSInt32Type = 3;
    internal const int kCFNumberSInt64Type = 4;
    internal const int kCFNumberFloat64Type = 6;

    // Null allocator (use default)
    internal static readonly nint kCFAllocatorDefault = 0;

    #endregion
}
